using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BNet.Cafe.Websocket
{
    public class Setup
    {
        private HttpListener _server;
        private int _activeConnections = 0;
        private const int MaxConnections = 50;

        private const int AgentRecvBufSize = 64 * 1024;
        private const int BrowserRecvBufSize = 64 * 1024;

        private static readonly byte[] PausePayload = { 0x40 };
        private static readonly byte[] ResumePayload = { 0x41 };
        private static readonly byte[] PongPayload = { 0x31 };
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        private readonly ConcurrentDictionary<string, WebSocketSession> _agentSessions
            = new ConcurrentDictionary<string, WebSocketSession>();

        private readonly ConcurrentDictionary<string, List<WebSocketSession>> _browserSessions
            = new ConcurrentDictionary<string, List<WebSocketSession>>();

        private readonly ConcurrentDictionary<WebSocket, WebSocketSession> _allBrowserSessions
            = new ConcurrentDictionary<WebSocket, WebSocketSession>();

        private readonly ConcurrentDictionary<string, byte[]> _latestFrames
            = new ConcurrentDictionary<string, byte[]>();

        private readonly ConcurrentDictionary<string, int> _agentSubscriberCount
            = new ConcurrentDictionary<string, int>();

        private readonly ConcurrentDictionary<string, string> _latestActivity
            = new ConcurrentDictionary<string, string>();

        private readonly object _subscriberLock = new object();

        private readonly ClientManager _clientManager = new ClientManager();
        private readonly InputQueueManager _inputQueueManager = new InputQueueManager();

        public void StartWebsocket()
        {
            try
            {
                int port = 2050;
                string prefix = $"http://*:{port}/";

                PortManager.RemoveUrlAcl(prefix);
                PortManager.AddUrlAcl(prefix);
                PortManager.OpenFirewallPort(port, $"WebSocketPort_{port}");

                _server = new HttpListener();
                _server.Prefixes.Add(prefix);
                _server.Start();

                Task.Run(() => AcceptLoop());
            }
            catch
            {
                throw;
            }
        }

        public void StopWebsocket()
        {
            try
            {
                _server?.Stop();
                _server?.Close();
            }
            catch { }
        }

        private async Task AcceptLoop()
        {
            while (_server != null && _server.IsListening)
            {
                try
                {
                    var ctx = await _server.GetContextAsync();
                    _ = Task.Run(() => RouteRequest(ctx));
                }
                catch (HttpListenerException)
                {
                    break;
                }
                catch
                {
                    await Task.Delay(1000);
                }
            }
        }

        private async Task RouteRequest(HttpListenerContext ctx)
        {
            try
            {
                ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                string path = ctx.Request.Url.AbsolutePath;

                if (ctx.Request.IsWebSocketRequest)
                {
                    if (Interlocked.CompareExchange(ref _activeConnections, 0, 0) >= MaxConnections)
                    {
                        ctx.Response.StatusCode = 503;
                        ctx.Response.Close();
                        return;
                    }

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
                    if (path == "/ws/server")
                    {
                        var wsCtx = await ctx.AcceptWebSocketAsync(null);
                        Interlocked.Increment(ref _activeConnections);
                        _ = Task.Run(() => HandleServerWebSocket(wsCtx.WebSocket));
                        return;
                    }

                    ctx.Response.StatusCode = 400;
                    ctx.Response.Close();
                    return;
                }

                if (path == "/text" && ctx.Request.HttpMethod == "GET")
                {
                    byte[] b = Utf8.GetBytes(_clientManager.ClientListJson);
                    ctx.Response.ContentType = "application/json";
                    ctx.Response.ContentLength64 = b.Length;
                    await ctx.Response.OutputStream.WriteAsync(b, 0, b.Length);
                    ctx.Response.Close();
                    return;
                }

                if (path == "/sse" && ctx.Request.HttpMethod == "GET")
                {
                    ctx.Response.ContentType = "text/event-stream";
                    ctx.Response.Headers.Add("Cache-Control", "no-cache");
                    ctx.Response.Headers.Add("Connection", "keep-alive");

                    using (var writer = new StreamWriter(ctx.Response.OutputStream, Utf8))
                    {
                        string lastJson = null;

                        while (_server.IsListening)
                        {
                            string currentJson = _clientManager.ClientListJson;

                            if (currentJson != lastJson)
                            {
                                lastJson = currentJson;
                                await writer.WriteAsync($"data: {currentJson}\n\n");
                                await writer.FlushAsync();
                            }

                            await Task.Delay(1000);
                        }
                    }
                    return;
                }

                ctx.Response.StatusCode = 404;
                ctx.Response.Close();
            }
            catch
            {
                try { ctx.Response.Close(); } catch { }
            }
        }

        private async Task HandleAgentWebSocket(WebSocket ws)
        {
            string agentName = null;
            var session = new WebSocketSession(ws);
            var buf = new byte[AgentRecvBufSize];

            try
            {
                while (ws.State == WebSocketState.Open)
                {
                    byte[] msg = await WebSocketHelper.ReceiveFullMessageAsync(ws, buf);
                    if (msg == null) break;
                    if (msg.Length == 0) continue;

                    byte msgType = msg[0];

                    if (msgType == 0x01)
                    {
                        if (msg.Length < 5) continue;
                        int nameLen = BitConverter.ToInt32(msg, 1);
                        if (msg.Length < 5 + nameLen) continue;

                        string screenKey = Utf8.GetString(msg, 5, nameLen);
                        int jpegOffset = 5 + nameLen;
                        int jpegLen = msg.Length - jpegOffset;

                        byte[] compressed = new byte[jpegLen];
                        Buffer.BlockCopy(msg, jpegOffset, compressed, 0, jpegLen);
                        byte[] jpegBytes = ByteCompressor.Decompress(compressed);

                        byte[] payload = new byte[1 + jpegBytes.Length];
                        payload[0] = 0x10;
                        Buffer.BlockCopy(jpegBytes, 0, payload, 1, jpegBytes.Length);

                        _latestFrames[screenKey] = payload;
                        _ = Task.Run(() => PushPayloadToBrowsers(screenKey, payload));
                    }
                    else if (msgType == 0x03)
                    {
                        string actJson = Utf8.GetString(msg, 1, msg.Length - 1);
                        var info = JsonConvert.DeserializeObject<TextContent>(actJson);

                        if (info != null && !string.IsNullOrEmpty(info.ClientName))
                        {
                            agentName = info.ClientName.ToUpper();
                            _agentSessions[agentName] = session;

                            _clientManager.UpdateClientList(info);
                            await _inputQueueManager.FlushInputQueueToAgentAsync(agentName, session);
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
                    else if (msgType == 0x22)
                    {
                        try { await session.SendAsync(PongPayload); } catch { }
                    }
                }
            }
            catch { }
            finally
            {
                Interlocked.Decrement(ref _activeConnections);
                await WebSocketHelper.CloseWebSocketSafelyAsync(ws);

                if (agentName != null)
                {
                    _agentSessions.TryRemove(agentName, out _);

                    foreach (var key in _latestFrames.Keys
                        .Where(k => k.EndsWith("_" + agentName, StringComparison.OrdinalIgnoreCase)).ToList())
                    {
                        _latestFrames.TryRemove(key, out _);
                    }

                    _inputQueueManager.RemoveAgentQueue(agentName);
                    _clientManager.RemoveClient(agentName);
                    _ = BroadcastClientList();
                }
            }
        }

        private async Task HandleBrowserWebSocket(WebSocket ws)
        {
            var session = new WebSocketSession(ws);
            string subKey = null;
            var buf = new byte[BrowserRecvBufSize];

            _allBrowserSessions[ws] = session;
            await _clientManager.SendClientListToAsync(session);

            foreach (var kv in _latestActivity)
            {
                if (string.IsNullOrEmpty(kv.Value)) continue;
                byte[] jb = Utf8.GetBytes(kv.Value);
                byte[] rp = new byte[1 + jb.Length];
                rp[0] = 0x12;
                Buffer.BlockCopy(jb, 0, rp, 1, jb.Length);
                try { await session.SendAsync(rp); } catch { }
            }

            try
            {
                while (ws.State == WebSocketState.Open)
                {
                    byte[] msg = await WebSocketHelper.ReceiveFullMessageAsync(ws, buf);
                    if (msg == null) break;
                    if (msg.Length == 0) continue;

                    byte msgType = msg[0];

                    if (msgType == 0x20)
                    {
                        if (subKey == null) continue;
                        string json = Utf8.GetString(msg, 1, msg.Length - 1);
                        var input = JsonConvert.DeserializeObject<RemoteInput>(json);
                        if (input != null)
                            await _inputQueueManager.RouteInputToAgentAsync(ExtractClientName(subKey), input, _agentSessions);
                    }
                    else if (msgType == 0x21)
                    {
                        string newKey = Utf8.GetString(msg, 1, msg.Length - 1).Trim().Replace(" ", "");
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
                        lock (list)
                        {
                            added = !list.Contains(session);
                            if (added) list.Add(session);
                        }

                        if (added) await AdjustSubscriberCount(newAgent, +1);

                        if (newAgent != null && _agentSessions.TryGetValue(newAgent, out var agentSess))
                            await SendActiveScreensToAgent(newAgent, agentSess);

                        if (oldAgent != null && oldAgent != newAgent &&
                            _agentSessions.TryGetValue(oldAgent, out var oldAgentSess))
                            await SendActiveScreensToAgent(oldAgent, oldAgentSess);

                        if (_latestFrames.TryGetValue(newKey, out var latestPayload))
                            try { await session.SendAsync(latestPayload); } catch { }
                    }
                    else if (msgType == 0x22)
                    {
                        try { await session.SendAsync(PongPayload); } catch { }
                    }
                }
            }
            catch { }
            finally
            {
                Interlocked.Decrement(ref _activeConnections);
                await WebSocketHelper.CloseWebSocketSafelyAsync(ws);
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

        private async Task HandleServerWebSocket(WebSocket ws)
        {
            var session = new WebSocketSession(ws);
            var buf = new byte[BrowserRecvBufSize];

            try
            {
                while (ws.State == WebSocketState.Open)
                {
                    byte[] msg = await WebSocketHelper.ReceiveFullMessageAsync(ws, buf);
                    if (msg == null) break;
                    if (msg.Length == 0) continue;

                    byte msgType = msg[0];

                    if (msgType == 0x02)
                    {
                        string json = Utf8.GetString(msg, 1, msg.Length - 1);
                        var textMsg = JsonConvert.DeserializeObject<ServerTextMessage>(json);

                        if (textMsg != null && !string.IsNullOrEmpty(textMsg.TargetClient))
                        {
                            await SendTextMessageToAgent(textMsg.TargetClient.ToUpper(), textMsg.Message);
                        }
                    }
                    else if (msgType == 0x22)
                    {
                        try { await session.SendAsync(PongPayload); } catch { }
                    }
                }
            }
            catch { }
            finally
            {
                Interlocked.Decrement(ref _activeConnections);
                await WebSocketHelper.CloseWebSocketSafelyAsync(ws);
            }
        }

        private async Task SendTextMessageToAgent(string clientName, string message)
        {
            if (_agentSessions.TryGetValue(clientName, out var agentSession))
            {
                byte[] jb = Utf8.GetBytes(message);
                byte[] payload = new byte[1 + jb.Length];
                payload[0] = 0x02;
                Buffer.BlockCopy(jb, 0, payload, 1, jb.Length);
                await agentSession.SendAsync(payload);
            }
        }

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

        private async Task BroadcastClientList()
        {
            await _clientManager.BroadcastClientListAsync(_allBrowserSessions.Values);
        }

        private async Task BroadcastActivity(string actJson)
        {
            if (string.IsNullOrEmpty(actJson)) return;

            byte[] jb = Utf8.GetBytes(actJson);
            byte[] payload = new byte[1 + jb.Length];
            payload[0] = 0x12;
            Buffer.BlockCopy(jb, 0, payload, 1, jb.Length);

            var browsers = _allBrowserSessions.Values.ToList();
            if (browsers.Count == 0) return;

            await Task.WhenAll(browsers.Select(async s =>
            {
                try { await s.SendAsync(payload); } catch { }
            }));
        }

        private static string ExtractClientName(string screenKey)
        {
            if (screenKey == null) return null;
            var parts = screenKey.Split('_');
            return parts.Length >= 3 ? string.Join("_", parts.Skip(2)).ToUpper() : null;
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
    }
}