using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNet.Cafe.Websocket
{
    public class ServerTextMessage
    {
        public string TargetClient { get; set; } // Name of the target client (e.g., "pc1")
        public string Message { get; set; }      // Text content to send
    }
}
