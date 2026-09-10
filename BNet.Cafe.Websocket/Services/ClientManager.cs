using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BNet.Cafe.Websocket
{
    public class ClientManager
    {
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);
        private readonly List<TextContent> _clientList = new List<TextContent>();
        private readonly HashSet<string> _connectedClients = new HashSet<string>();
        private readonly object _clientLock = new object();

        public string ClientListJson { get; private set; } = "[]";

        public void UpdateClientList(TextContent info)
        {
            lock (_clientLock)
            {
                var existingClient = _clientList.FirstOrDefault(c =>
                    string.Equals(c.ClientName, info.ClientName, StringComparison.OrdinalIgnoreCase));

                if (existingClient == null)
                {
                    _connectedClients.Add(info.ClientName);
                    _clientList.Add(info);
                }
                else
                {
                    existingClient.TimeStart = info.TimeStart;
                    existingClient.TimeEnd = info.TimeEnd;
                    existingClient.IPAddress = info.IPAddress;
                }

                ClientListJson = JsonConvert.SerializeObject(_clientList, Formatting.Indented);
            }
        }

        public void RemoveClient(string agentName)
        {
            lock (_clientLock)
            {
                _connectedClients.Remove(agentName);
                var ex = _clientList.FirstOrDefault(c =>
                    string.Equals(c.ClientName, agentName, StringComparison.OrdinalIgnoreCase));
                if (ex != null) _clientList.Remove(ex);
                ClientListJson = JsonConvert.SerializeObject(_clientList, Formatting.Indented);
            }
        }

        public async Task BroadcastClientListAsync(IEnumerable<WebSocketSession> sessions)
        {
            byte[] payload = GetClientListPayload();
            var targetSessions = sessions.ToList();
            if (targetSessions.Count == 0) return;

            await Task.WhenAll(targetSessions.Select(async s =>
            {
                try { await s.SendAsync(payload); } catch { }
            }));
        }

        public async Task SendClientListToAsync(WebSocketSession session)
        {
            byte[] payload = GetClientListPayload();
            try { await session.SendAsync(payload); } catch { }
        }

        private byte[] GetClientListPayload()
        {
            lock (_clientLock)
            {
                byte[] jb = Utf8.GetBytes(ClientListJson);
                byte[] payload = new byte[1 + jb.Length];
                payload[0] = 0x11;
                Buffer.BlockCopy(jb, 0, payload, 1, jb.Length);
                return payload;
            }
        }
    }
}