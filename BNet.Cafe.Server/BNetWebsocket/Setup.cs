using BNet.WebSocket.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BNet.Cafe.Server.BNetWebsocket
{
    public class Setup
    {
        private Connection _connection = new Connection(2050);


        private static readonly JsonSerializerSettings _camel = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        string certPath = "";
        public void StartWebsocket()
        {

            int port = 2050; // default secure WebSocket port
            _connection = new Connection(port);

            LogCertificateInfo();
            _connection.StartAsync();
            _connection.OnReceived += Connection_OnReceived;
            _connection.OnBinaryReceived += Connection_OnBinaryReceived;

        }

        private void LogCertificateInfo()
        {
            if (!File.Exists(certPath)) return;
            _connection.LoadCertificate(certPath, "");

            const string logFolder = @"C:\BNet.Cafe";
            string logFile = System.IO.Path.Combine(logFolder, "websocket_certificate.txt");
            Directory.CreateDirectory(logFolder);

            var sb = new StringBuilder();
            using (var cert = new X509Certificate2(certPath, ""))
            {
                sb.AppendLine(string.Format("[{0}] Certificate Info:", DateTime.Now));
                sb.AppendLine(string.Format("  Valid From : {0}", cert.NotBefore));
                sb.AppendLine(string.Format("  Valid To   : {0}", cert.NotAfter));

                bool isExpired = DateTime.Now < cert.NotBefore || DateTime.Now > cert.NotAfter;
                if (isExpired)
                    sb.AppendLine(string.Format("[{0}] WARNING: Certificate is expired or not yet valid.", DateTime.Now));

                using (var chain = new X509Chain())
                {
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    if (!chain.Build(cert))
                    {
                        sb.AppendLine(string.Format("[{0}] WARNING: Certificate chain is invalid.", DateTime.Now));
                        foreach (var status in chain.ChainStatus)
                            sb.AppendLine(string.Format("  Chain Status: {0} - {1}", status.Status, status.StatusInformation));
                    }
                }
            }
            File.AppendAllText(logFile, sb.ToString() + Environment.NewLine);
        }
 
        private void Connection_OnReceived(object sender, EventHandlers.ReceivedEventArgs e)
        {
            // Fire-and-forget with explicit exception guard — never async void
            Task.Run(async () =>
            {
               
            });
        }

        private void Connection_OnBinaryReceived(object sender, EventHandlers.BinaryReceivedEventArgs e)
        {
            Task.Run(async () =>
            {
               
            });
        }
    }
}