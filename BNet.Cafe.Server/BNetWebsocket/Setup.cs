using BNet.WebSocket.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BNet.Cafe.Server.BNetWebsocket
{
    public class Setup
    {
        private Connection _connection;
        private string _certPath = "";

        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        // Control Payloads
        private static readonly byte[] PausePayload = { 0x40 };
        private static readonly byte[] ResumePayload = { 0x41 };
        private static readonly byte[] PongPayload = { 0x31 };

        // State Tracking
        private readonly ConcurrentDictionary<string, byte[]> _latestFrames = new ConcurrentDictionary<string, byte[]>();
        private readonly ConcurrentDictionary<string, int> _agentSubscriberCount = new ConcurrentDictionary<string, int>();

        private readonly List<TextContent> _clientList = new List<TextContent>();
        private readonly HashSet<string> _connectedClients = new HashSet<string>();
        private string _clientListJson = "[]";
        private readonly object _clientLock = new object();

        private readonly ConcurrentDictionary<string, ConcurrentQueue<RemoteInput>> _inputQueues = new ConcurrentDictionary<string, ConcurrentQueue<RemoteInput>>();
        private readonly ConcurrentDictionary<string, string> _latestActivity = new ConcurrentDictionary<string, string>();

        public void StartWebsocket(int port = 2050)
        {
            _connection = new Connection(port);
            LogCertificateInfo();

            _connection.OnReceived += Connection_OnReceived;
            _connection.OnBinaryReceived += Connection_OnBinaryReceived;

            // Fire-and-forget start as per Connection API pattern
            _ = _connection.StartAsync();
        }

        private void LogCertificateInfo()
        {
            if (string.IsNullOrWhiteSpace(_certPath) || !File.Exists(_certPath)) return;
            _connection.LoadCertificate(_certPath, "");

            const string logFolder = @"C:\BNet.Cafe";
            string logFile = Path.Combine(logFolder, "websocket_certificate.txt");
            Directory.CreateDirectory(logFolder);

            var sb = new StringBuilder();
            using (var cert = new X509Certificate2(_certPath, ""))
            {
                sb.AppendLine($"[{DateTime.Now}] Certificate Info:");
                sb.AppendLine($"  Valid From : {cert.NotBefore}");
                sb.AppendLine($"  Valid To   : {cert.NotAfter}");

                if (DateTime.Now < cert.NotBefore || DateTime.Now > cert.NotAfter)
                    sb.AppendLine($"[{DateTime.Now}] WARNING: Certificate is expired or not yet valid.");
            }
            File.AppendAllText(logFile, sb.ToString() + Environment.NewLine);
        }

        // ─────────────────────────────────────────────────────────────────────
        // WEBSOCKET EVENT HANDLERS
        // ─────────────────────────────────────────────────────────────────────

        private void Connection_OnReceived(object sender, EventHandlers.ReceivedEventArgs e)
        {
            Task.Run(async () =>
            {
                if (string.IsNullOrEmpty(e.Message)) return;
                byte[] data = Utf8.GetBytes(e.Message);
                await ProcessBinaryMessage(data);
            });
        }

        private void Connection_OnBinaryReceived(object sender, EventHandlers.BinaryReceivedEventArgs e)
        {
            Task.Run(async () =>
            {
                if (e.Data == null || e.Data.Length == 0) return;
                await ProcessBinaryMessage(e.Data);
            });
        }

        private async Task ProcessBinaryMessage(byte[] msg)
        {
            if (msg == null || msg.Length == 0) return;
            byte msgType = msg[0];

            switch (msgType)
            {
                case 0x01: // FRAME [type][4:nameLen][name][jpeg]
                    if (msg.Length < 5) return;
                    int nameLen = BitConverter.ToInt32(msg, 1);
                    if (msg.Length < 5 + nameLen) return;

                    string screenKey = Utf8.GetString(msg, 5, nameLen);
                    int jpegOffset = 5 + nameLen;
                    int jpegLen = msg.Length - jpegOffset;

                    byte[] jpegBytes = new byte[jpegLen];
                    Buffer.BlockCopy(msg, jpegOffset, jpegBytes, 0, jpegLen);

                    byte[] payload = new byte[1 + jpegBytes.Length];
                    payload[0] = 0x10;
                    Buffer.BlockCopy(jpegBytes, 0, payload, 1, jpegBytes.Length);

                    _latestFrames[screenKey] = payload;

                    // Route to room subscribers
                    await _connection.SendBinaryToRoomAsync(screenKey, payload);
                    break;

                case 0x02: // TEXT_INFO
                    string json = Utf8.GetString(msg, 1, msg.Length - 1);
                    var info = JsonConvert.DeserializeObject<TextContent>(json);
                    if (info == null) return;

                    string agentName = info.AccountName?.ToLower();

                    UpdateClientList(info);
                    await FlushInputQueueToAgent(agentName);
                    await BroadcastClientList();

                    int subs = _agentSubscriberCount.GetOrAdd(agentName, 0);

                    if (subs == 0)
                        await _connection.SendBinaryToRoomAsync(agentName, PausePayload);
                    else
                    {
                        await _connection.SendBinaryToRoomAsync(agentName, ResumePayload);
                        await SendActiveScreensToAgent(agentName);
                    }

                    if (_latestActivity.TryGetValue(agentName, out var cached))
                        await BroadcastActivity(cached);
                    break;

                case 0x03: // ACTIVITY
                    string actJson = Utf8.GetString(msg, 1, msg.Length - 1);
                    await BroadcastActivity(actJson);
                    break;

                case 0x20: // INPUT
                    string inputJson = Utf8.GetString(msg, 1, msg.Length - 1);
                    var input = JsonConvert.DeserializeObject<RemoteInput>(inputJson);
                    if (input != null && !string.IsNullOrEmpty(input.TargetAgent))
                        await RouteInputToAgent(input.TargetAgent.ToLower(), input);
                    break;

                case 0x21: // SUBSCRIBE
                    string targetRoom = Utf8.GetString(msg, 1, msg.Length - 1).Trim();
                    string agentRoom = ExtractClientName(targetRoom);

                    if (agentRoom != null)
                    {
                        int newCount = _agentSubscriberCount.AddOrUpdate(agentRoom, 1, (_, old) => old + 1);
                        if (newCount == 1)
                        {
                            await _connection.SendBinaryToRoomAsync(agentRoom, ResumePayload);
                        }
                        await SendActiveScreensToAgent(agentRoom);
                    }

                    if (_latestFrames.TryGetValue(targetRoom, out var latestPayload))
                    {
                        await _connection.SendBinaryToRoomAsync(targetRoom, latestPayload);
                    }
                    break;

                case 0x22: // PING
                    await _connection.SendBinaryAsync(PongPayload);
                    break;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // ROUTING & MESSAGING HELPERS
        // ─────────────────────────────────────────────────────────────────────

        private async Task SendActiveScreensToAgent(string agentName)
        {
            if (string.IsNullOrEmpty(agentName)) return;

            var activeIndices = new HashSet<int> { 0 }; // Default active primary display

            string json = JsonConvert.SerializeObject(activeIndices.OrderBy(i => i).ToArray());
            byte[] jb = Utf8.GetBytes(json);
            byte[] payload = new byte[1 + jb.Length];
            payload[0] = 0x43;
            Buffer.BlockCopy(jb, 0, payload, 1, jb.Length);

            await _connection.SendBinaryToRoomAsync(agentName, payload);
        }

        private async Task RouteInputToAgent(string clientName, RemoteInput input)
        {
            if (string.IsNullOrEmpty(clientName)) return;

            string json = JsonConvert.SerializeObject(new[] { input });
            byte[] jb = Utf8.GetBytes(json);
            byte[] payload = new byte[1 + jb.Length];
            payload[0] = 0x30;
            Buffer.BlockCopy(jb, 0, payload, 1, jb.Length);

            await _connection.SendBinaryToRoomAsync(clientName, payload);
        }

        private async Task FlushInputQueueToAgent(string clientName)
        {
            if (string.IsNullOrEmpty(clientName) || !_inputQueues.TryGetValue(clientName, out var q)) return;
            var batch = new List<RemoteInput>();
            while (q.TryDequeue(out var item)) batch.Add(item);
            if (batch.Count == 0) return;

            string json = JsonConvert.SerializeObject(batch);
            byte[] jb = Utf8.GetBytes(json);
            byte[] payload = new byte[1 + jb.Length];
            payload[0] = 0x30;
            Buffer.BlockCopy(jb, 0, payload, 1, jb.Length);

            await _connection.SendBinaryToRoomAsync(clientName, payload);
        }

        private void UpdateClientList(TextContent info)
        {
            lock (_clientLock)
            {
                if (!_connectedClients.Contains(info.AccountName))
                {
                    _connectedClients.Add(info.AccountName);
                    _clientList.Add(info);
                }
                else
                {
                    var ex = _clientList.FirstOrDefault(c => string.Equals(c.AccountName, info.AccountName, StringComparison.OrdinalIgnoreCase));
                    if (ex != null)
                    {
                        ex.ScreenCount = info.ScreenCount;
                        ex.Windows = info.Windows;
                        ex.WindowsVersion = info.WindowsVersion;
                        ex.OSArchitecture = info.OSArchitecture;
                        ex.SerialNumber = info.SerialNumber;
                        ex.MachineName = info.MachineName;
                        ex.WorkGroup = info.WorkGroup;
                        ex.OSVersion = info.OSVersion;
                        ex.ProcessorCount = info.ProcessorCount;
                    }
                }
                _clientListJson = JsonConvert.SerializeObject(_clientList, Formatting.Indented);
            }
        }

        private async Task BroadcastClientList()
        {
            byte[] payload;
            lock (_clientLock)
            {
                byte[] jb = Utf8.GetBytes(_clientListJson);
                payload = new byte[1 + jb.Length];
                payload[0] = 0x11;
                Buffer.BlockCopy(jb, 0, payload, 1, jb.Length);
            }

            await _connection.SendBinaryAsync(payload);
        }

        private async Task BroadcastActivity(string actJson)
        {
            if (string.IsNullOrEmpty(actJson)) return;
            byte[] jb = Utf8.GetBytes(actJson);
            byte[] payload = new byte[1 + jb.Length];
            payload[0] = 0x12;
            Buffer.BlockCopy(jb, 0, payload, 1, jb.Length);

            await _connection.SendBinaryAsync(payload);
        }

        private static string ExtractClientName(string screenKey)
        {
            if (screenKey == null) return null;
            var parts = screenKey.Split('_');
            return parts.Length >= 3 ? string.Join("_", parts.Skip(2)).ToLower() : null;
        }
    }
}