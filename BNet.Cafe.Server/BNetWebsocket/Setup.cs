using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BNet.Cafe.Server.BNetWebsocket
{
    public class Setup
    {
        // ── Message types ─────────────────────────────────────────────────────────
        //  ALL MESSAGES ARE NOW BINARY (WebSocketMessageType.Binary everywhere).
        //  Text payloads (JSON) are UTF-8 encoded and sent as binary frames.
        //
        //  C→S   0x01 FRAME        binary  [type][4:nameLen][name][jpeg]
        //        0x02 TEXT_INFO    binary  [type][utf8 json TextContent]
        //        0x03 ACTIVITY     binary  [type][utf8 json ActiveWindowInfo]
        //  S→B   0x10 FRAME        binary  [type][jpeg]
        //        0x11 CLIENT_LIST  binary  [type][utf8 json array]
        //        0x12 ACTIVITY     binary  [type][utf8 json ActiveWindowInfo]
        //  B→S   0x20 INPUT        binary  [type][utf8 json RemoteInput]
        //        0x21 SUBSCRIBE    binary  [type][utf8 screenKey]
        //        0x22 PING         binary
        //  S→C   0x30 INPUT_BATCH  binary  [type][utf8 json RemoteInput[]]
        //        0x31 PONG         binary
        //        0x40 PAUSE        binary  stop capturing
        //        0x41 RESUME       binary  start capturing
        //        0x43 SET_SCREENS  binary  [type][utf8 json int[]]  0-based indices to stream
        //
        // PERFORMANCE CHANGES:
        //  • Agent FRAME handler (0x01) — browser payload is built ONCE from the
        //    incoming message body (direct slice), eliminating a full jpeg[] copy
        //    that existed in the original code.
        //  • ReceiveFullMessage — fast path for single-chunk messages skips the
        //    MemoryStream allocation (the common case for control messages).
        //  • AgentRecvBufSize raised to 256 KB so large JPEG frames arrive in
        //    a single ReceiveAsync call and hit the fast path above.


        private HttpListener _server;

        private int _activeConnections = 0;
        private const int MaxConnections = 50;

        // Input queue hard cap — prevents unbounded growth when agent is offline
        private const int InputQueueCap = 15;

        private readonly ConcurrentDictionary<string, WebSocketSession> _agentSessions
            = new ConcurrentDictionary<string, WebSocketSession>();

        // screenKey → list of browser sessions subscribed to that key
        private readonly ConcurrentDictionary<string, List<WebSocketSession>> _browserSessions
            = new ConcurrentDictionary<string, List<WebSocketSession>>();

        private readonly ConcurrentDictionary<WebSocket, WebSocketSession> _allBrowserSessions
            = new ConcurrentDictionary<WebSocket, WebSocketSession>();

        // _latestFrames stores the most-recent browser payload (0x10 + jpeg) per screenKey.
        // We store the full payload so late-joining browsers can receive it immediately
        // without rebuilding.
        private readonly ConcurrentDictionary<string, byte[]> _latestFrames
            = new ConcurrentDictionary<string, byte[]>();

        private readonly ConcurrentDictionary<string, int> _agentSubscriberCount
            = new ConcurrentDictionary<string, int>();
        private readonly object _subscriberLock = new object();

        private readonly List<TextContent> _clientList = new List<TextContent>();
        private readonly HashSet<string> _connectedClients = new HashSet<string>();
        private string _clientListJson = "[]";
        private readonly object _clientLock = new object();

        private readonly ConcurrentDictionary<string, ConcurrentQueue<RemoteInput>> _inputQueues
            = new ConcurrentDictionary<string, ConcurrentQueue<RemoteInput>>();

        private readonly ConcurrentDictionary<string, string> _latestActivity
            = new ConcurrentDictionary<string, string>();

        private static readonly byte[] PausePayload = { 0x40 };
        private static readonly byte[] ResumePayload = { 0x41 };
        private static readonly byte[] PongPayload = { 0x31 };

        // Raised from 64 KB → 256 KB so large JPEG frames arrive in one
        // ReceiveAsync call and hit the fast path in ReceiveFullMessage.
        private const int AgentRecvBufSize = 64 * 1024;
        private const int BrowserRecvBufSize = 64 * 1024;

        // Shared UTF-8 codec — all messages are binary; text is UTF-8 in the body
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        // ─────────────────────────────────────────────────────────────────────
        // FORM LOAD
        // ─────────────────────────────────────────────────────────────────────

        public void StartWebsocket()
        {
            try
            {
                int port = 2050;
                string prefix = $"http://*:{port}/";

                // 1. Grant non-admin listening rights for the wildcard URL
                PortManager.AddUrlAcl(prefix);

                // 2. Open inbound firewall port
                PortManager.OpenFirewallPort(port, $"WebSocketPort_{port}");

                // 3. Start server
                _server = new HttpListener();
                _server.Prefixes.Add(prefix);
                _server.Start();

            }
            catch { }
            Task.Run(AcceptLoop);
        }

        private async Task AcceptLoop()
        {
            while (true)
            {
                try { var ctx = await _server.GetContextAsync(); _ = Task.Run(() => RouteRequest(ctx)); }
                catch { }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // ROUTER
        // ─────────────────────────────────────────────────────────────────────

        private async Task RouteRequest(HttpListenerContext ctx)
        {
            try
            {
                ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                string path = ctx.Request.Url.AbsolutePath;

                if (ctx.Request.IsWebSocketRequest)
                {
                    if (Interlocked.CompareExchange(ref _activeConnections, 0, 0) >= MaxConnections)
                    { ctx.Response.StatusCode = 503; ctx.Response.Close(); return; }

                    if (path == "/ws/agent")
                    {
                        var wsCtx = await ctx.AcceptWebSocketAsync(null);
                        Interlocked.Increment(ref _activeConnections);
                        _ = Task.Run(() => HandleAgentWebSocket(wsCtx.WebSocket));
                        return;
                    }
                    if (path == "/ws/browser")
                    {
                        var wsCtx = await ctx.AcceptWebSocketAsync(null);
                        Interlocked.Increment(ref _activeConnections);
                        _ = Task.Run(() => HandleBrowserWebSocket(wsCtx.WebSocket));
                        return;
                    }
                    ctx.Response.StatusCode = 400; ctx.Response.Close(); return;
                }
                ctx.Response.StatusCode = 404; ctx.Response.Close();
            }
            catch { try { ctx.Response.Close(); } catch { } }
        }

        // ─────────────────────────────────────────────────────────────────────
        // AGENT WebSocket
        // ─────────────────────────────────────────────────────────────────────

        private async Task HandleAgentWebSocket(WebSocket ws)
        {
            string agentName = null;
            var session = new WebSocketSession(ws);
            var buf = new byte[AgentRecvBufSize];

            try
            {
                while (ws.State == WebSocketState.Open)
                {
                    byte[] msg = await ReceiveFullMessage(ws, buf);
                    if (msg == null) break;
                    if (msg.Length == 0) continue;

                    byte msgType = msg[0];

                    if (msgType == 0x01) // FRAME
                    {
                        if (msg.Length < 5) continue;
                        int nameLen = BitConverter.ToInt32(msg, 1);
                        if (msg.Length < 5 + nameLen) continue;

                        string screenKey = Utf8.GetString(msg, 5, nameLen);
                        int jpegOffset = 5 + nameLen;
                        int jpegLen = msg.Length - jpegOffset;

                        // ── Single-allocation browser payload ──────────────────
                        // Build [0x10][jpeg] in one shot by slicing directly from
                        // the received message — no separate jpeg[] intermediate.
                        byte[] compressed = new byte[jpegLen];
                        Buffer.BlockCopy(msg, jpegOffset, compressed, 0, jpegLen);
                        byte[] jpegBytes = ByteCompressor.Decompress(compressed);

                        byte[] payload = new byte[1 + jpegBytes.Length];
                        payload[0] = 0x10;
                        Buffer.BlockCopy(jpegBytes, 0, payload, 1, jpegBytes.Length);

                        // Store the full payload as the latest frame so late-joining
                        // browsers get it immediately without rebuilding.
                        _latestFrames[screenKey] = payload;

                        _ = Task.Run(() => PushPayloadToBrowsers(screenKey, payload));
                    }
                    else if (msgType == 0x03) // ACTIVITY
                    {
                        string actJson = Utf8.GetString(msg, 1, msg.Length - 1);
                        var info = JsonConvert.DeserializeObject<TextContent>(actJson); // Maps to TextContent fields

                        if (info != null && !string.IsNullOrEmpty(info.AccountName))
                        {
                            agentName = info.AccountName.ToLower();
                            _agentSessions[agentName] = session;

                            // Keep client list and screen streaming state updated
                            UpdateClientList(info);
                            await FlushInputQueueToAgent(agentName, session);
                            _ = BroadcastClientList();

                            int subs;
                            lock (_subscriberLock) subs = _agentSubscriberCount.GetOrAdd(agentName, 0);

                            if (subs == 0)
                                try { await session.SendAsync(PausePayload); } catch { }
                            else
                            {
                                try { await session.SendAsync(ResumePayload); } catch { }
                                await SendActiveScreensToAgent(agentName, session);
                            }
                        }

                        if (agentName != null) _latestActivity[agentName] = actJson;
                        _ = BroadcastActivity(actJson);
                    }
                    else if (msgType == 0x22) // PING
                    {
                        try { await session.SendAsync(PongPayload); } catch { }
                    }
                }
            }
            catch { }
            finally
            {
                Interlocked.Decrement(ref _activeConnections);
                await CloseWebSocketSafely(ws);

                if (agentName != null)
                {
                    _agentSessions.TryRemove(agentName, out _);

                    foreach (var key in _latestFrames.Keys
                        .Where(k => k.EndsWith("_" + agentName, StringComparison.OrdinalIgnoreCase)).ToList())
                        _latestFrames.TryRemove(key, out _);

                    _inputQueues.TryRemove(agentName, out _);

                    lock (_clientLock)
                    {
                        _connectedClients.Remove(agentName);
                        var ex = _clientList.FirstOrDefault(c =>
                            string.Equals(c.AccountName, agentName, StringComparison.OrdinalIgnoreCase));
                        if (ex != null) _clientList.Remove(ex);
                        _clientListJson = JsonConvert.SerializeObject(_clientList, Formatting.Indented);
                    }
                    _ = BroadcastClientList();
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // BROWSER WebSocket
        // ─────────────────────────────────────────────────────────────────────

        private async Task HandleBrowserWebSocket(WebSocket ws)
        {
            var session = new WebSocketSession(ws);
            string subKey = null;
            var buf = new byte[BrowserRecvBufSize];

            _allBrowserSessions[ws] = session;
            await SendClientListTo(session);

            foreach (var kv in _latestActivity)
            {
                if (string.IsNullOrEmpty(kv.Value)) continue;
                byte[] jb = Utf8.GetBytes(kv.Value);
                byte[] rp = new byte[1 + jb.Length]; rp[0] = 0x12;
                Buffer.BlockCopy(jb, 0, rp, 1, jb.Length);
                try { await session.SendAsync(rp); } catch { }
            }

            try
            {
                while (ws.State == WebSocketState.Open)
                {
                    byte[] msg = await ReceiveFullMessage(ws, buf);
                    if (msg == null) break;
                    if (msg.Length == 0) continue;

                    byte msgType = msg[0];

                    if (msgType == 0x20) // INPUT — binary frame, UTF-8 JSON body
                    {
                        if (subKey == null) continue;
                        string json = Utf8.GetString(msg, 1, msg.Length - 1);
                        var input = JsonConvert.DeserializeObject<RemoteInput>(json);
                        if (input != null) await RouteInputToAgent(ExtractClientName(subKey), input);
                    }
                    else if (msgType == 0x21) // SUBSCRIBE — binary frame, UTF-8 key body
                    {
                        string newKey = Utf8.GetString(msg, 1, msg.Length - 1).Trim();
                        if (newKey == subKey) continue;

                        string oldAgent = subKey != null ? ExtractClientName(subKey) : null;
                        if (subKey != null)
                        {
                            bool wasIn = RemoveBrowserFromKey(subKey, session);
                            if (wasIn) await AdjustSubscriberCount(oldAgent, -1);
                        }

                        subKey = newKey;
                        string newAgent = ExtractClientName(newKey);
                        var list = _browserSessions.GetOrAdd(newKey, _ => new List<WebSocketSession>());
                        bool added;
                        lock (list) { added = !list.Contains(session); if (added) list.Add(session); }
                        if (added) await AdjustSubscriberCount(newAgent, +1);

                        if (newAgent != null && _agentSessions.TryGetValue(newAgent, out var agentSess))
                            await SendActiveScreensToAgent(newAgent, agentSess);

                        if (oldAgent != null && oldAgent != newAgent &&
                            _agentSessions.TryGetValue(oldAgent, out var oldAgentSess))
                            await SendActiveScreensToAgent(oldAgent, oldAgentSess);

                        // Send the latest cached frame immediately so the browser
                        // doesn't show a blank screen while waiting for the next capture.
                        // _latestFrames now stores the full browser payload (0x10 + jpeg).
                        if (_latestFrames.TryGetValue(newKey, out var latestPayload))
                            try { await session.SendAsync(latestPayload); } catch { }
                    }
                    else if (msgType == 0x22) // PING
                    {
                        try { await session.SendAsync(PongPayload); } catch { }
                    }
                }
            }
            catch { }
            finally
            {
                Interlocked.Decrement(ref _activeConnections);
                await CloseWebSocketSafely(ws);
                _allBrowserSessions.TryRemove(ws, out _);

                if (subKey != null)
                {
                    string agentName = ExtractClientName(subKey);
                    bool wasIn = RemoveBrowserFromKey(subKey, session);
                    if (wasIn) await AdjustSubscriberCount(agentName, -1);

                    if (agentName != null && _agentSessions.TryGetValue(agentName, out var agentSess))
                        await SendActiveScreensToAgent(agentName, agentSess);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // SET_SCREENS (0x43)
        // ─────────────────────────────────────────────────────────────────────

        private async Task SendActiveScreensToAgent(string agentName, WebSocketSession agentSession)
        {
            if (agentName == null || agentSession == null) return;

            var activeIndices = new HashSet<int>();
            foreach (var kv in _browserSessions)
            {
                if (!kv.Key.EndsWith("_" + agentName, StringComparison.OrdinalIgnoreCase)) continue;
                bool hasLive;
                lock (kv.Value) hasLive = kv.Value.Any(s => s.IsAlive);
                if (!hasLive) continue;
                activeIndices.Add(ExtractScreenIndex(kv.Key));
            }

            string json = JsonConvert.SerializeObject(activeIndices.OrderBy(i => i).ToArray());
            byte[] jb = Utf8.GetBytes(json);
            byte[] payload = new byte[1 + jb.Length];
            payload[0] = 0x43;
            Buffer.BlockCopy(jb, 0, payload, 1, jb.Length);

            try { await agentSession.SendAsync(payload); } catch { }
        }

        // ─────────────────────────────────────────────────────────────────────
        // WEBSOCKET CLOSE
        // ─────────────────────────────────────────────────────────────────────

        private static async Task CloseWebSocketSafely(WebSocket ws)
        {
            try
            {
                if (ws.State == WebSocketState.Open || ws.State == WebSocketState.CloseReceived)
                {
                    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cts.Token);
                }
            }
            catch { }
            finally { try { ws.Abort(); } catch { } ws.Dispose(); }
        }

        // ─────────────────────────────────────────────────────────────────────
        // SUBSCRIBER COUNT → PAUSE / RESUME
        // ─────────────────────────────────────────────────────────────────────

        private async Task AdjustSubscriberCount(string agentName, int delta)
        {
            if (agentName == null) return;
            int oldCount, newCount;
            lock (_subscriberLock)
            {
                oldCount = _agentSubscriberCount.GetOrAdd(agentName, 0);
                newCount = Math.Max(0, oldCount + delta);
                _agentSubscriberCount[agentName] = newCount;
            }
            if (oldCount == 0 && newCount == 1)
            {
                if (_agentSessions.TryGetValue(agentName, out var s))
                    try { await s.SendAsync(ResumePayload); } catch { }
            }
            else if (oldCount > 0 && newCount == 0)
            {
                if (_agentSessions.TryGetValue(agentName, out var s))
                    try { await s.SendAsync(PausePayload); } catch { }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // FRAME PUSH
        // TrySendFrameAsync uses WaitAsync(0) — slow browsers drop frames
        // instead of blocking the server or other browsers.
        // ─────────────────────────────────────────────────────────────────────

        private async Task PushPayloadToBrowsers(string screenKey, byte[] payload)
        {
            if (!_browserSessions.TryGetValue(screenKey, out var sessions)) return;
            List<WebSocketSession> snapshot;
            lock (sessions) snapshot = sessions.ToList();
            if (snapshot.Count == 0) return;

            var dead = new List<WebSocketSession>();
            await Task.WhenAll(snapshot.Select(async s =>
            {
                bool ok = await s.TrySendFrameAsync(payload);
                if (!ok && !s.IsAlive) dead.Add(s);
            }));

            if (dead.Count > 0)
            {
                lock (sessions) foreach (var d in dead) sessions.Remove(d);
                string agentName = ExtractClientName(screenKey);
                foreach (var _ in dead) await AdjustSubscriberCount(agentName, -1);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // INPUT ROUTING
        // ─────────────────────────────────────────────────────────────────────

        private async Task RouteInputToAgent(string clientName, RemoteInput input)
        {
            if (clientName == null) return;
            if (_agentSessions.TryGetValue(clientName, out var agentSession))
            {
                string json = JsonConvert.SerializeObject(new[] { input });
                byte[] jb = Utf8.GetBytes(json);
                byte[] payload = new byte[1 + jb.Length]; payload[0] = 0x30;
                Buffer.BlockCopy(jb, 0, payload, 1, jb.Length);
                try { await agentSession.SendAsync(payload); } catch { }
            }
            else
            {
                var q = _inputQueues.GetOrAdd(clientName, _ => new ConcurrentQueue<RemoteInput>());
                while (q.Count >= InputQueueCap) q.TryDequeue(out _);
                q.Enqueue(input);
            }
        }

        private async Task FlushInputQueueToAgent(string clientName, WebSocketSession session)
        {
            if (!_inputQueues.TryGetValue(clientName, out var q)) return;
            var batch = new List<RemoteInput>();
            while (q.TryDequeue(out var item)) batch.Add(item);
            if (batch.Count == 0) return;
            string json = JsonConvert.SerializeObject(batch);
            byte[] jb = Utf8.GetBytes(json);
            byte[] payload = new byte[1 + jb.Length]; payload[0] = 0x30;
            Buffer.BlockCopy(jb, 0, payload, 1, jb.Length);
            try { await session.SendAsync(payload); } catch { }
        }

        // ─────────────────────────────────────────────────────────────────────
        // CLIENT LIST
        // ─────────────────────────────────────────────────────────────────────

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
                    var ex = _clientList.FirstOrDefault(c =>
                        string.Equals(c.AccountName, info.AccountName, StringComparison.OrdinalIgnoreCase));
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
                payload = new byte[1 + jb.Length]; payload[0] = 0x11;
                Buffer.BlockCopy(jb, 0, payload, 1, jb.Length);
            }
            var browsers = _allBrowserSessions.Values.ToList();
            if (browsers.Count == 0) return;
            await Task.WhenAll(browsers.Select(async s =>
            { try { await s.SendAsync(payload); } catch { } }));
        }

        private async Task BroadcastActivity(string actJson)
        {
            if (string.IsNullOrEmpty(actJson)) return;
            byte[] jb = Utf8.GetBytes(actJson);
            byte[] payload = new byte[1 + jb.Length]; payload[0] = 0x12;
            Buffer.BlockCopy(jb, 0, payload, 1, jb.Length);
            var browsers = _allBrowserSessions.Values.ToList();
            if (browsers.Count == 0) return;
            await Task.WhenAll(browsers.Select(async s =>
            { try { await s.SendAsync(payload); } catch { } }));
        }

        private async Task SendClientListTo(WebSocketSession session)
        {
            byte[] payload;
            lock (_clientLock)
            {
                byte[] jb = Utf8.GetBytes(_clientListJson);
                payload = new byte[1 + jb.Length]; payload[0] = 0x11;
                Buffer.BlockCopy(jb, 0, payload, 1, jb.Length);
            }
            try { await session.SendAsync(payload); } catch { }
        }

        // ─────────────────────────────────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────────────────────────────────

        private static string ExtractClientName(string screenKey)
        {
            if (screenKey == null) return null;
            var parts = screenKey.Split('_');
            return parts.Length >= 3 ? string.Join("_", parts.Skip(2)).ToLower() : null;
        }

        private static int ExtractScreenIndex(string screenKey)
        {
            if (screenKey == null) return 0;
            var parts = screenKey.Split('_');
            if (parts.Length >= 2 && int.TryParse(parts[1], out int n))
                return Math.Max(0, n - 1);
            return 0;
        }

        private bool RemoveBrowserFromKey(string key, WebSocketSession session)
        {
            if (_browserSessions.TryGetValue(key, out var list))
                lock (list) return list.Remove(session);
            return false;
        }

        /// <summary>
        /// Receive one complete WebSocket message.
        ///
        /// Fast path: if the entire message arrives in the first ReceiveAsync call
        /// (the common case for control messages and most JPEG frames given the
        /// 256 KB buffer), we skip the MemoryStream allocation entirely.
        /// </summary>
        private static async Task<byte[]> ReceiveFullMessage(WebSocket ws, byte[] buffer)
        {
            WebSocketReceiveResult result;
            try
            {
                result = await ws.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            catch { return null; }

            if (result.MessageType == WebSocketMessageType.Close) return null;

            // Fast path — single-chunk message (the common case)
            if (result.EndOfMessage)
            {
                var single = new byte[result.Count];
                Buffer.BlockCopy(buffer, 0, single, 0, result.Count);
                return single;
            }

            // Slow path — multi-chunk message
            using (var ms = new MemoryStream())
            {
                ms.Write(buffer, 0, result.Count);
                do
                {
                    try
                    {
                        result = await ws.ReceiveAsync(
                            new ArraySegment<byte>(buffer), CancellationToken.None);
                    }
                    catch { return null; }
                    if (result.MessageType == WebSocketMessageType.Close) return null;
                    ms.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);
                return ms.ToArray();
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Thread-safe WebSocket sender — ALL frames sent as Binary.
    //
    // SendAsync         : blocking send (for control/data messages)
    // TrySendFrameAsync : non-blocking — drops the frame if busy (video frames)
    // ─────────────────────────────────────────────────────────────────────────

    public class WebSocketSession
    {
        private readonly WebSocket _ws;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

        public WebSocketSession(WebSocket ws) => _ws = ws;

        public bool IsAlive =>
            _ws.State == WebSocketState.Open || _ws.State == WebSocketState.CloseReceived;

        /// <summary>Blocking binary send — for control/metadata messages.</summary>
        public async Task<bool> SendAsync(byte[] data)
        {
            await _sendLock.WaitAsync();
            try
            {
                if (_ws.State != WebSocketState.Open) return false;
                await _ws.SendAsync(new ArraySegment<byte>(data),
                    WebSocketMessageType.Binary,
                    endOfMessage: true,
                    cancellationToken: CancellationToken.None);
                return true;
            }
            catch { return false; }
            finally { _sendLock.Release(); }
        }

        /// <summary>
        /// Non-blocking frame send.  Returns false immediately if busy or dead.
        /// Dropped frames are intentional — caller must NOT re-queue them.
        /// </summary>
        public async Task<bool> TrySendFrameAsync(byte[] data)
        {
            // WaitAsync(0) returns immediately if busy -> drops old frame
            if (!await _sendLock.WaitAsync(0)) return false;
            try
            {
                if (_ws.State != WebSocketState.Open) return false;
                await _ws.SendAsync(new ArraySegment<byte>(data),
                    WebSocketMessageType.Binary, true, CancellationToken.None);
                return true;
            }
            finally { _sendLock.Release(); }
        }
    }
}