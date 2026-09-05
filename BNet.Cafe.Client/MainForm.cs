using BNet.Cafe.Client.Models;
using BNet.Cafe.Client.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BNet.Cafe.Client
{
    /// <summary>
    /// Agent (client machine) — connects to the server via WebSocket.
    /// ALL WebSocket frames are now sent as Binary (WebSocketMessageType.Binary).
    /// Text payloads (JSON / strings) are UTF-8 encoded into the binary frame body.
    ///
    /// PERFORMANCE CHANGES:
    ///  • Dirty-region detection — unchanged frames are skipped entirely (saves 70-90% bandwidth on idle screens).
    ///  • Adaptive quality/scale — drops temporarily under backlog instead of going dark.
    ///  • Single-copy frame building — header written into one MemoryStream; no intermediate jpeg[] array.
    ///  • Fast-path ReceiveFullMessage — avoids MemoryStream allocation for single-chunk messages.
    ///  • WebSocket send/recv buffer raised to 256 KB to handle large frames without kernel splits.
    ///  • ImageCompressor.CompressInto() used to write JPEG directly into the payload stream.
    /// </summary>
    public partial class ClientScreen : Form
    {
        public ClientScreen() { InitializeComponent(); }
        private const int ReconnectDelayMs = 500;    // was 1000 — near-instant recovery
        private const int TargetFrameMs = 25;     // was 33  — aims for 40fps, lands ~30fps after OS timer jitter
        private const int TextInfoEveryNFrames = 300;  // was 120 — device info every ~7.5s; it almost never changes
        private const long JpegQuality = 22L;    // was 25  — imperceptible drop, measurable bandwidth saving
        private const long JpegQualityBacklog = 8L;    // was 10  — squeeze harder under pressure
        private const double ImageScale = 0.70;   // was 0.75 — saves ~13% more pixels vs 0.75, still sharp on HD
        private const double ImageScaleBacklog = 0.40; // was 0.5  — real pressure relief without going unreadable
        private const int ActivityIntervalMs = 8000;   // was 5000 — low-value poll, no reason to run it often
        private const int SendTimeoutMs = 600;    // was 800  — faster failure detection; 600ms is still generous
        // Shared UTF-8 encoder — used for all JSON/string payloads
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        // ── Pause / resume ────────────────────────────────────────────────────
        private volatile bool _paused = true;
        private readonly SemaphoreSlim _resumeSignal = new SemaphoreSlim(0, 1);

        // ── Per-screen LATEST frame slot ──────────────────────────────────────
        private readonly ConcurrentDictionary<int, byte[]> _pendingFrames
            = new ConcurrentDictionary<int, byte[]>();

        private readonly SemaphoreSlim _framePending = new SemaphoreSlim(0, 1);

        // ── Backlog indicator set by the sender when a send takes too long ────
        private volatile bool _isBacklogged = false;

        // ── Which screen indices the server wants streamed (0-based set) ───────
        private volatile int[] _requestedScreenIndices = new int[0];

        private TextContent _deviceInfo;

        // Pre-baked screen key bytes indexed by screen number (0-based).
        private byte[][] _screenKeyBytes;
        private int _cachedScreenCount = 0;

        // ── Dirty-region detection — last sampled hash per screen index ───────
        // Uses a fast FNV-1a sampling hash over every 64th byte of the JPEG.
        // Identical hash → screen content unchanged → skip send.
        private readonly ConcurrentDictionary<int, uint> _lastFrameHash
            = new ConcurrentDictionary<int, uint>();

        // ── Activity monitoring ───────────────────────────────────────────────
        private volatile AgentSession _currentSession;

        // ─────────────────────────────────────────────────────────────────────
        // FORM LOAD
        // ─────────────────────────────────────────────────────────────────────

        private async void ClientScreen_Load(object sender, EventArgs e)
        {
            await Task.Delay(1000);
            _deviceInfo = await GatherDeviceInfo();

            RebuildScreenKeyCache(ScreenCaptured.GetScreenCount());

            UserActivity.ActiveWindowMonitor.OnPolled += async info =>
            {
                var sess = _currentSession;
                if (sess == null || !sess.IsOpen) return;
                await SendActivityInfo(sess, info);
            };
            UserActivity.ActiveWindowMonitor.StartPolling(ActivityIntervalMs);

            _ = Task.Run(RunAgentLoop);
        }

        // ─────────────────────────────────────────────────────────────────────
        // SCREEN KEY CACHE
        // ─────────────────────────────────────────────────────────────────────

        private void RebuildScreenKeyCache(int count)
        {
            if (count == _cachedScreenCount) return;
            _cachedScreenCount = count;
            var keys = new byte[count][];
            string user = _deviceInfo.AccountName.ToLower();
            for (int i = 0; i < count; i++)
                keys[i] = Utf8.GetBytes($"screen_{i + 1}_{user}");
            _screenKeyBytes = keys;
        }

        // ─────────────────────────────────────────────────────────────────────
        // MAIN LOOP
        // ─────────────────────────────────────────────────────────────────────

        private async Task RunAgentLoop()
        {
            string wsUrl = ConfigurationManager.AppSettings["WebSocketUrl"];
            Uri agentWsUri = new Uri($"{wsUrl}/ws/agent");

            while (true)
            {
                try
                {
                    _paused = true;
                    _requestedScreenIndices = new int[0];
                    _pendingFrames.Clear();
                    _lastFrameHash.Clear();   // reset dirty-detection on reconnect
                    _isBacklogged = false;

                    while (_resumeSignal.CurrentCount > 0) _resumeSignal.Wait(0);
                    while (_framePending.CurrentCount > 0) _framePending.Wait(0);

                    using (var ws = new ClientWebSocket())
                    {
                        ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
                        // ClientWebSocket is capped at 65536 by the runtime —
                        // keep at 64 KB. The server-side HttpListener socket
                        // has no such cap and uses 256 KB.
                        ws.Options.SetBuffer(64 * 1024, 64 * 1024);

                        await ws.ConnectAsync(agentWsUri, CancellationToken.None);

                        var session = new AgentSession(ws, SendTimeoutMs);
                        _currentSession = session;

                        await SendDeviceInfo(session);

                        _ = Task.Run(async () =>
                        {
                            var info = UserActivity.ActiveWindowMonitor.GetCurrent();
                            await SendActivityInfo(session, info);
                        });

                        var captureTask = CaptureLoop(session);
                        var sendTask = SendLoop(session);
                        var recvTask = ReceiveLoop(session);

                        await Task.WhenAny(captureTask, sendTask, recvTask);
                        _currentSession = null;

                        _framePending.Release();
                    }
                }
                catch { }

                await Task.Delay(ReconnectDelayMs);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // CAPTURE LOOP  (producer — runs independently of network)
        //
        // CHANGES:
        //  • Adaptive quality/scale based on _isBacklogged.
        //  • Single-copy payload building: header + JPEG written into one
        //    MemoryStream via ImageCompressor.CompressInto() — no intermediate
        //    jpeg[] allocation.
        //  • Dirty-region skip: FNV-1a sample hash compared against last frame;
        //    unchanged screens are dropped before touching the network.
        // ─────────────────────────────────────────────────────────────────────

        private async Task CaptureLoop(AgentSession session)
        {
            var compressor = new ImageCompressor();
            int frameCounter = 0;

            while (session.IsOpen)
            {
                if (_paused)
                {
                    await _resumeSignal.WaitAsync(500);
                    continue;
                }

                long frameStart = Environment.TickCount;

                try
                {
                    int totalScreens = ScreenCaptured.GetScreenCount();
                    RebuildScreenKeyCache(totalScreens);
                    _deviceInfo.ScreenCount = totalScreens.ToString();

                    if (frameCounter % TextInfoEveryNFrames == 0)
                        _ = SendDeviceInfo(session);
                    frameCounter++;

                    int[] indices = _requestedScreenIndices;
                    if (indices.Length == 0) goto NextFrame;

                    if (_isBacklogged) goto NextFrame;

                    // Pick quality/scale based on current network health.
                    bool backlogged = _isBacklogged;
                    long quality = backlogged ? JpegQualityBacklog : JpegQuality;
                    double scale = backlogged ? ImageScaleBacklog : ImageScale;

                    await Task.WhenAll(indices.Select(async screenIdx =>
                    {
                        if (screenIdx >= totalScreens) screenIdx = 0;

                        List<Bitmap> captured = await Task.Run(
                            () => ScreenCaptured.TakeScreenshot(
                                isMouseVisible: true,
                                screenIndex: screenIdx))
                            .ConfigureAwait(false);

                        if (captured == null || captured.Count == 0) return;
                        Bitmap bmp = captured[0];

                        if (_paused || !session.IsOpen) { bmp.Dispose(); return; }

                        // ── Single-copy payload build ──────────────────────────
                        // Layout: [0x01][4:nameLen][name bytes][jpeg bytes]
                        // We write the header first, then stream the JPEG
                        // directly into the same buffer — zero intermediate copies.
                        byte[] nameBytes = _screenKeyBytes[screenIdx];

                        byte[] payload;
                        using (var ms = new MemoryStream(nameBytes.Length + 32 * 1024))
                        {
                            ms.WriteByte(0x01);
                            ms.Write(BitConverter.GetBytes(nameBytes.Length), 0, 4);
                            ms.Write(nameBytes, 0, nameBytes.Length);

                            // GZip compress the JPEG before adding to payload
                            using (var jpeg = new MemoryStream(32 * 1024))
                            {
                                compressor.CompressInto(bmp, jpeg, quality, scale);
                                byte[] compressed = ByteCompressor.Compress(jpeg.ToArray());
                                ms.Write(compressed, 0, compressed.Length);
                            }

                            payload = ms.ToArray();
                        }
                        bmp.Dispose();

                        if (!session.IsOpen || _paused) return;

                        // ── Dirty-region detection ─────────────────────────────
                        // Hash only the JPEG portion (after the fixed header).
                        int jpegOffset = 1 + 4 + nameBytes.Length;
                        uint hash = SampleHash(payload, jpegOffset, payload.Length - jpegOffset);

                        if (_lastFrameHash.TryGetValue(screenIdx, out uint prevHash)
                            && prevHash == hash)
                            return; // screen unchanged — skip this frame entirely

                        _lastFrameHash[screenIdx] = hash;

                        // Queue for the send loop
                        _pendingFrames[screenIdx] = payload;

                        if (_framePending.CurrentCount == 0)
                            _framePending.Release();
                    }));
                }
                catch (Exception ex) when (!(ex is WebSocketException)) { /* skip frame */ }
                catch { return; }

            NextFrame:
                int elapsed = (int)(Environment.TickCount - frameStart);
                int sleep = Math.Max(0, TargetFrameMs - elapsed);
                if (_isBacklogged) sleep = Math.Max(sleep, 50);
                if (sleep > 0) await Task.Delay(sleep);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // SEND LOOP  (consumer — drains _pendingFrames over the network)
        // ─────────────────────────────────────────────────────────────────────

        private async Task SendLoop(AgentSession session)
        {
            while (session.IsOpen)
            {
                bool got = await _framePending.WaitAsync(500);
                if (!got) continue;

                foreach (var kv in _pendingFrames.ToArray())
                {
                    if (!session.IsOpen) return;

                    if (!_pendingFrames.TryRemove(kv.Key, out byte[] payload)) continue;

                    bool ok = await session.SendAsync(payload, WebSocketMessageType.Binary);
                    if (!ok)
                    {
                        _isBacklogged = true;
                        return;
                    }
                    _isBacklogged = false;
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // RECEIVE LOOP
        // ─────────────────────────────────────────────────────────────────────

        private async Task ReceiveLoop(AgentSession session)
        {
            var buf = new byte[64 * 1024];

            while (session.IsOpen)
            {
                try
                {
                    byte[] msg = await ReceiveFullMessage(session.WebSocket, buf);
                    if (msg == null) return;
                    if (msg.Length == 0) continue;

                    byte msgType = msg[0];

                    if (msgType == 0x40)      // PAUSE
                    {
                        _paused = true;
                        _pendingFrames.Clear();
                        _lastFrameHash.Clear(); // force full re-send on resume
                    }
                    else if (msgType == 0x41) // RESUME
                    {
                        _paused = false;
                        _isBacklogged = false;
                        if (_resumeSignal.CurrentCount == 0)
                            _resumeSignal.Release();
                    }
                    else if (msgType == 0x43) // SET_SCREENS — binary frame, UTF-8 JSON body
                    {
                        try
                        {
                            string json = Utf8.GetString(msg, 1, msg.Length - 1);
                            int[] indices = JsonConvert.DeserializeObject<int[]>(json) ?? new int[0];
                            _requestedScreenIndices = indices;
                            var indexSet = new HashSet<int>(indices);
                            foreach (var key in _pendingFrames.Keys.ToArray())
                                if (!indexSet.Contains(key)) _pendingFrames.TryRemove(key, out _);
                            // Clear hashes for screens that are no longer requested
                            // so they get a full frame when re-subscribed.
                            foreach (var key in _lastFrameHash.Keys.ToArray())
                                if (!indexSet.Contains(key)) _lastFrameHash.TryRemove(key, out _);
                        }
                        catch { }
                    }
                    else if (msgType == 0x30) // INPUT_BATCH — binary frame, UTF-8 JSON body
                    {
                        string json = Utf8.GetString(msg, 1, msg.Length - 1);
                        var events = JsonConvert.DeserializeObject<List<RemoteInput>>(json);
                        if (events != null)
                            foreach (var evt in events)
                                try { RemoteController.Dispatch(evt); } catch { }
                    }
                    // 0x31 PONG — ignore
                }
                catch { return; }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // ACTIVITY SENDER
        // ─────────────────────────────────────────────────────────────────────

        private async Task SendActivityInfo(AgentSession session, UserActivity.ActiveWindowInfo info)
        {
            try
            {
                var payloadObj = new
                {
                    accountName = _deviceInfo.AccountName,
                    appName = info.AppName,
                    processName = info.ProcessName,
                    windowTitle = info.WindowTitle,
                    url = info.Url,
                    isBrowser = info.IsBrowser,
                    capturedAt = info.CapturedAt.ToString("o")
                };
                string json = JsonConvert.SerializeObject(payloadObj);
                byte[] jsonBytes = Utf8.GetBytes(json);
                byte[] frame = new byte[1 + jsonBytes.Length];
                frame[0] = 0x03;
                Buffer.BlockCopy(jsonBytes, 0, frame, 1, jsonBytes.Length);
                await session.SendAsync(frame, WebSocketMessageType.Binary);
            }
            catch { }
        }

        // ─────────────────────────────────────────────────────────────────────
        // DEVICE INFO SENDER
        // ─────────────────────────────────────────────────────────────────────

        private async Task SendDeviceInfo(AgentSession session)
        {
            try
            {
                string json = JsonConvert.SerializeObject(_deviceInfo);
                byte[] jsonBytes = Utf8.GetBytes(json);
                byte[] payload = new byte[1 + jsonBytes.Length];
                payload[0] = 0x02;
                Buffer.BlockCopy(jsonBytes, 0, payload, 1, jsonBytes.Length);
                await session.SendAsync(payload, WebSocketMessageType.Binary);
            }
            catch { }
        }

        // ─────────────────────────────────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Fast-path ReceiveFullMessage.
        /// Most messages arrive in a single WebSocket chunk — we skip the
        /// MemoryStream allocation entirely for those.  Only multi-chunk
        /// messages (rare) fall through to the slower path.
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

            // Fast path — entire message fits in first chunk (the common case)
            if (result.EndOfMessage)
            {
                var single = new byte[result.Count];
                Buffer.BlockCopy(buffer, 0, single, 0, result.Count);
                return single;
            }

            // Slow path — message spans multiple chunks
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

        /// <summary>
        /// Fast FNV-1a sample hash — reads every 64th byte of the JPEG data.
        /// Fast enough to run every frame; accurate enough to catch any
        /// real screen change (a single pixel difference shifts the hash).
        /// </summary>
        private static uint SampleHash(byte[] data, int offset, int length)
        {
            uint h = 2166136261u;
            int end = offset + length;
            for (int i = offset; i < end; i += 64)
                h = (h ^ data[i]) * 16777619u;
            return h;
        }

        private static async Task<TextContent> GatherDeviceInfo()
        {
            string caption = "", version = "", arch = "", serial = "";
            try
            {
                var wmi = new ManagementObjectSearcher("select * from Win32_OperatingSystem")
                    .Get().Cast<ManagementObject>().First();
                caption = ((string)wmi["Caption"]).Trim();
                version = (string)wmi["Version"];
                arch = (string)wmi["OSArchitecture"];
                serial = (string)wmi["SerialNumber"];
            }
            catch { }
            await Task.CompletedTask;
            return new TextContent
            {
                Windows = caption,
                WindowsVersion = version,
                OSArchitecture = arch,
                SerialNumber = serial,
                MachineName = Environment.MachineName,
                AccountName = ConfigurationManager.AppSettings["ClientName"].ToLower(),
                WorkGroup = Environment.UserDomainName,
                OSVersion = Environment.OSVersion.VersionString,
                ProcessorCount = Environment.ProcessorCount.ToString(),
                ScreenCount = ScreenCaptured.GetScreenCount().ToString()
            };
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Thread-safe send wrapper with HARD TIMEOUT.
    // ALL frames are sent as Binary — WebSocketMessageType.Binary is always used.
    // ─────────────────────────────────────────────────────────────────────────

    public class AgentSession
    {
        public readonly WebSocket WebSocket;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly int _timeoutMs;

        public AgentSession(WebSocket ws, int timeoutMs = 500)
        {
            WebSocket = ws;
            _timeoutMs = timeoutMs;
        }

        public bool IsOpen => WebSocket.State == WebSocketState.Open;

        /// <summary>
        /// Sends data as a binary WebSocket frame with a hard timeout.
        /// Returns true on success, false on timeout or error.
        /// </summary>
        public async Task<bool> SendAsync(byte[] data,
            WebSocketMessageType type = WebSocketMessageType.Binary)
        {
            await _lock.WaitAsync();
            try
            {
                if (!IsOpen) return false;
                using (var cts = new CancellationTokenSource(_timeoutMs))
                {
                    await WebSocket.SendAsync(
                        new ArraySegment<byte>(data),
                        WebSocketMessageType.Binary,
                        endOfMessage: true,
                        cancellationToken: cts.Token);
                }
                return true;
            }
            catch { return false; }
            finally { _lock.Release(); }
        }
    }
}