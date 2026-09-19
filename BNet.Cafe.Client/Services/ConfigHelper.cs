using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace BNet.Cafe.Client.Services
{
    internal class ConfigHelper
    {
        private static readonly Lazy<string> _cachedClientName = new Lazy<string>(ResolveClientName);

        public static string GetClientNameFromIP()
        {
            return _cachedClientName.Value;
        }

        private static string ResolveClientName()
        {
            try
            {
                string configName = ConfigurationManager.AppSettings["ClientName"];
                if (!string.IsNullOrEmpty(configName))
                {
                    return configName.ToUpper().Replace(" ", "");
                }
                else
                {
                    // Inspect local network adapters directly (No DNS lookup delay when offline)
                    var localIp = NetworkInterface.GetAllNetworkInterfaces()
                        .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                                     ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                        .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                        .FirstOrDefault(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork
                                           && !IPAddress.IsLoopback(ip.Address))?.Address;

                    if (localIp != null)
                    {
                        string[] octets = localIp.ToString().Split('.');
                        if (octets.Length == 4 && int.TryParse(octets[3], out int lastOctet))
                        {
                            if (lastOctet > 100)
                            {
                                int pcNumber = lastOctet - 100;
                                return $"PC-{pcNumber:D2}";
                            }
                            else
                            {
                                return $"PC-{lastOctet:D2}";
                            }
                        }
                    }
                }
            }
            catch { }

            return Environment.MachineName;
        }
        public static string WebSocketUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["WebSocketUrl"];
            }
        }
        public static string AppUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["AppUrl"];
            }
        }

        public static string FtpServerPath
        {
            get
            {
                string webSocketUrl = ConfigurationManager.AppSettings["WebSocketUrl"];

                if (Uri.TryCreate(webSocketUrl, UriKind.Absolute, out Uri uri))
                {
                    return $"ftp://{uri.Host}/";
                }

                return string.Empty;
            }
        }
    }
}