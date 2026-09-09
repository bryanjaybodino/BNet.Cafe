using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace BNet.Cafe.Websocket
{
    public static class WebSocketHelper
    {
        public static async Task CloseWebSocketSafelyAsync(WebSocket ws)
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
            finally
            {
                try { ws.Abort(); } catch { }
                ws.Dispose();
            }
        }

        public static async Task<byte[]> ReceiveFullMessageAsync(WebSocket ws, byte[] buffer)
        {
            WebSocketReceiveResult result;
            try
            {
                result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            catch { return null; }

            if (result.MessageType == WebSocketMessageType.Close) return null;

            if (result.EndOfMessage)
            {
                var single = new byte[result.Count];
                Buffer.BlockCopy(buffer, 0, single, 0, result.Count);
                return single;
            }

            using (var ms = new MemoryStream())
            {
                ms.Write(buffer, 0, result.Count);
                do
                {
                    try
                    {
                        result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    }
                    catch { return null; }
                    if (result.MessageType == WebSocketMessageType.Close) return null;
                    ms.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);

                return ms.ToArray();
            }
        }
    }
}