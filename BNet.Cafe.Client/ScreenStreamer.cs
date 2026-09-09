using BNet.Cafe.Client.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BNet.Cafe.Client
{
    public class ScreenStreamer
    {
        private const int TargetFrameMs = 25;
        private const long JpegQuality = 22L;
        private const long JpegQualityBacklog = 8L;
        private const double ImageScale = 0.70;
        private const double ImageScaleBacklog = 0.40;

        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        private byte[][] _screenKeyBytes;
        private int _cachedScreenCount = 0;

        private readonly ConcurrentDictionary<int, uint> _lastFrameHash = new ConcurrentDictionary<int, uint>();

        public void RebuildScreenKeyCache(int count, string clientName)
        {
            if (count == _cachedScreenCount) return;
            _cachedScreenCount = count;
            var keys = new byte[count][];
            string user = clientName.ToUpper();
            for (int i = 0; i < count; i++)
                keys[i] = Utf8.GetBytes($"screen_{i + 1}_{user}");
            _screenKeyBytes = keys;
        }

        public void ClearCache()
        {
            _lastFrameHash.Clear();
        }

        public void RemoveKeysNotIn(HashSet<int> activeIndices)
        {
            foreach (var key in _lastFrameHash.Keys.ToArray())
            {
                if (!activeIndices.Contains(key))
                    _lastFrameHash.TryRemove(key, out _);
            }
        }

        public async Task CaptureLoopAsync(
            AgentSession session,
            Func<bool> isPaused,
            Func<bool> isBacklogged,
            Func<int[]> getRequestedIndices,
            ConcurrentDictionary<int, byte[]> pendingFrames,
            SemaphoreSlim resumeSignal,
            SemaphoreSlim framePending,
            string clientName)
        {
            var compressor = new ImageCompressor();

            while (session.IsOpen)
            {
                if (isPaused())
                {
                    await resumeSignal.WaitAsync(500);
                    continue;
                }

                long frameStart = Environment.TickCount;

                try
                {
                    int totalScreens = ScreenCaptured.GetScreenCount();
                    RebuildScreenKeyCache(totalScreens, clientName);

                    int[] indices = getRequestedIndices();
                    if (indices.Length == 0 || isBacklogged()) goto NextFrame;

                    bool backlogged = isBacklogged();
                    long quality = backlogged ? JpegQualityBacklog : JpegQuality;
                    double scale = backlogged ? ImageScaleBacklog : ImageScale;

                    await Task.WhenAll(indices.Select(async screenIdx =>
                    {
                        if (screenIdx >= totalScreens) screenIdx = 0;

                        List<Bitmap> captured = await Task.Run(
                            () => ScreenCaptured.TakeScreenshot(isMouseVisible: true, screenIndex: screenIdx))
                            .ConfigureAwait(false);

                        if (captured == null || captured.Count == 0) return;
                        Bitmap bmp = captured[0];

                        if (isPaused() || !session.IsOpen) { bmp.Dispose(); return; }

                        byte[] nameBytes = _screenKeyBytes[screenIdx];
                        byte[] payload;

                        using (var ms = new MemoryStream(nameBytes.Length + 32 * 1024))
                        {
                            ms.WriteByte(0x01);
                            ms.Write(BitConverter.GetBytes(nameBytes.Length), 0, 4);
                            ms.Write(nameBytes, 0, nameBytes.Length);

                            using (var jpeg = new MemoryStream(32 * 1024))
                            {
                                compressor.CompressInto(bmp, jpeg, quality, scale);
                                byte[] compressed = ByteCompressor.Compress(jpeg.ToArray());
                                ms.Write(compressed, 0, compressed.Length);
                            }

                            payload = ms.ToArray();
                        }
                        bmp.Dispose();

                        if (!session.IsOpen || isPaused()) return;

                        int jpegOffset = 1 + 4 + nameBytes.Length;
                        uint hash = SampleHash(payload, jpegOffset, payload.Length - jpegOffset);

                        if (_lastFrameHash.TryGetValue(screenIdx, out uint prevHash) && prevHash == hash)
                            return;

                        _lastFrameHash[screenIdx] = hash;
                        pendingFrames[screenIdx] = payload;

                        if (framePending.CurrentCount == 0)
                            framePending.Release();
                    }));
                }
                catch (Exception ex) when (!(ex is System.Net.WebSockets.WebSocketException)) { }
                catch { return; }

            NextFrame:
                int elapsed = (int)(Environment.TickCount - frameStart);
                int sleep = Math.Max(0, TargetFrameMs - elapsed);
                if (isBacklogged()) sleep = Math.Max(sleep, 50);
                if (sleep > 0) await Task.Delay(sleep);
            }
        }

        private static uint SampleHash(byte[] data, int offset, int length)
        {
            uint h = 2166136261u;
            int end = offset + length;
            for (int i = offset; i < end; i += 64)
                h = (h ^ data[i]) * 16777619u;
            return h;
        }
    }
}