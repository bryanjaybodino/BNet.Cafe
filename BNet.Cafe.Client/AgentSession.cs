using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BNet.Cafe.Client
{
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
