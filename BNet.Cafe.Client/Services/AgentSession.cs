using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace BNet.Cafe.Client
{
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

        public async Task<bool> SendAsync(byte[] data, WebSocketMessageType type = WebSocketMessageType.Binary)
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