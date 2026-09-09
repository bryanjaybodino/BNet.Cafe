using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace BNet.Cafe.Websocket
{
    public class WebSocketSession
    {
        private readonly WebSocket _ws;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

        public WebSocketSession(WebSocket ws) => _ws = ws;

        public bool IsAlive =>
            _ws.State == WebSocketState.Open || _ws.State == WebSocketState.CloseReceived;

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

        public async Task<bool> TrySendFrameAsync(byte[] data)
        {
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