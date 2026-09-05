using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BNet.Cafe.Server.BNetWebsocket
{

    public class RemoteInput
    {
        public string TargetAgent { get; set; }
        public string Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Button { get; set; }
        public int Delta { get; set; }
        public string Key { get; set; }
        public string Code { get; set; }
        public int KeyCode { get; set; }
        public bool Ctrl { get; set; }
        public bool Alt { get; set; }
        public bool Shift { get; set; }
        public bool Meta { get; set; }
        public int ScreenIndex { get; set; }
        public int FrameW { get; set; }
        public int FrameH { get; set; }
    }
}