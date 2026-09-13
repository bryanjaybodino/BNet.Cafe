using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BNet.Cafe.Websocket
{
    public class InputQueueManager
    {
        private const int InputQueueCap = 15;
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        private readonly ConcurrentDictionary<string, ConcurrentQueue<RemoteInput>> _inputQueues
            = new ConcurrentDictionary<string, ConcurrentQueue<RemoteInput>>();

        public async Task RouteInputToAgentAsync(
            string clientName,
            RemoteInput input,
            ConcurrentDictionary<string, WebSocketSession> agentSessions)
        {
            if (clientName == null) return;

            if (agentSessions.TryGetValue(clientName, out var agentSession))
            {
                string json = JsonConvert.SerializeObject(new[] { input });
                byte[] jb = Utf8.GetBytes(json);
                byte[] payload = new byte[1 + jb.Length];
                payload[0] = 0x30;
                Buffer.BlockCopy(jb, 0, payload, 1, jb.Length);
                try { await agentSession.SendAsync(payload); } catch { }
            }
            else
            {
                var q = _inputQueues.GetOrAdd(clientName, _ => new ConcurrentQueue<RemoteInput>());

                // Coalesce mousemove: replace trailing mousemove if new input is also mousemove
                if (input.Type?.ToLower() == "mousemove" && q.Count > 0)
                {
                    // Drop old queued moves if queue is backing up
                    while (q.Count >= InputQueueCap) q.TryDequeue(out _);
                }

                q.Enqueue(input);
            }
        }

        public async Task FlushInputQueueToAgentAsync(string clientName, WebSocketSession session)
        {
            if (!_inputQueues.TryGetValue(clientName, out var q)) return;

            var batch = new List<RemoteInput>();
            while (q.TryDequeue(out var item)) batch.Add(item);
            if (batch.Count == 0) return;

            string json = JsonConvert.SerializeObject(batch);
            byte[] jb = Utf8.GetBytes(json);
            byte[] payload = new byte[1 + jb.Length];
            payload[0] = 0x30;
            Buffer.BlockCopy(jb, 0, payload, 1, jb.Length);
            try { await session.SendAsync(payload); } catch { }
        }

        public void RemoveAgentQueue(string clientName)
        {
            _inputQueues.TryRemove(clientName, out _);
        }
    }
}