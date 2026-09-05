using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BNet.Cafe.Server.BNetWebsocket
{
    public class TextContent
    {
        public string ScreenCount { get; set; }
        public string MachineName { get; set; }
        public string WorkGroup { get; set; }
        public string OSVersion { get; set; }
        public string OSArchitecture { get; set; }
        public string SerialNumber { get; set; }
        public string ProcessorCount { get; set; }
        public string AccountName { get; set; }
        public string Windows { get; set; }
        public string WindowsVersion { get; set; }
    }
}