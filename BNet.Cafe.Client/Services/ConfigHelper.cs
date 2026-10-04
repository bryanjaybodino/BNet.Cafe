using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Xml;

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

        private static string GetOrCreateSetting(string key, string defaultValue)
        {
            string value = ConfigurationManager.AppSettings[key];

            if (!string.IsNullOrWhiteSpace(value))
                return value;

            SaveAppSettingWithoutRemovingComments(key, defaultValue);
            return defaultValue;
        }

        public static bool IsAccountCreationAllowed
        {
            get
            {
                string value = GetOrCreateSetting("AccountCreationAllowed", "true");
                return !string.Equals(value?.Trim(), "false", StringComparison.OrdinalIgnoreCase);
            }
        }

        public static bool IsDesktopSlideShow
        {
            get
            {
                string value = GetOrCreateSetting("DesktopSlideShow", "false");
                return !string.Equals(value?.Trim(), "false", StringComparison.OrdinalIgnoreCase);
            }
        }

        public static bool ResetShutdownCountdown
        {
            get
            {
                string value = GetOrCreateSetting("ResetShutdownCountdown", "false");
                return !string.Equals(value?.Trim(), "false", StringComparison.OrdinalIgnoreCase);
            }
        }
        public static string WebSocketUrl
        {
            get
            {
                string url = ConfigurationManager.AppSettings["WebSocketUrl"];

                if (!string.IsNullOrWhiteSpace(url))
                {
                    return url;
                }

                // Discover server IP if key is empty
                IPAddress gateway = GetDefaultGateway();
                string serverIp = null;

                if (gateway != null)
                {
                    serverIp = DiscoverServerIp(gateway, targetPort: 2050);
                }

                if (!string.IsNullOrEmpty(serverIp))
                {
                    string discoveredWsUrl = $"ws://{serverIp}:2050";
                    SaveAppSettingWithoutRemovingComments("WebSocketUrl", discoveredWsUrl);
                    return discoveredWsUrl;
                }

                // Fallback default
                return GetOrCreateSetting("WebSocketUrl", "ws://localhost:8080");
            }
        }

        public static string AutoShutDownInterval
        {
            get
            {
                string rawValue = GetOrCreateSetting("AutoShutDownInterval", "300");

                if (int.TryParse(rawValue?.Trim(), out int timeout))
                {
                    int validTimeout = Math.Max(60, timeout);
                    return validTimeout.ToString();
                }

                return "300";
            }
        }

        public static string AppUrl
        {
            get
            {
                string url = ConfigurationManager.AppSettings["AppUrl"];

                if (!string.IsNullOrWhiteSpace(url))
                {
                    return url;
                }

                // Discover server IP if key is empty
                IPAddress gateway = GetDefaultGateway();

                if (gateway != null)
                {
                    string serverIp = DiscoverServerIp(gateway, targetPort: 2000);
                    if (!string.IsNullOrEmpty(serverIp))
                    {
                        string discoveredUrl = $"http://{serverIp}:2000/BNet.Cafe.Server/";
                        SaveAppSettingWithoutRemovingComments("AppUrl", discoveredUrl);
                        return discoveredUrl;
                    }
                }

                // Fallback default
                return GetOrCreateSetting("AppUrl", "http://localhost:5000");
            }
        }


        static string foundIp = "";
        private static string DiscoverServerIp(IPAddress gateway, int targetPort)
        {
            if (!string.IsNullOrWhiteSpace(foundIp))
            {
                return foundIp;
            }
            byte[] bytes = gateway.GetAddressBytes();
            string gatewayIpStr = gateway.ToString();


            Parallel.For(1, 255, (i, loopState) =>
            {
                string targetIp = $"{bytes[0]}.{bytes[1]}.{bytes[2]}.{i}";

                // Skip scanning the gateway address directly
                if (targetIp == gatewayIpStr)
                    return;

                // Probe TCP port 2000 to verify the server application is active
                if (IsPortOpen(targetIp, targetPort, timeoutMs: 250))
                {
                    foundIp = targetIp;
                    loopState.Stop();
                }
            });
            return foundIp;
        }

        private static bool IsPortOpen(string host, int port, int timeoutMs)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    IAsyncResult result = client.BeginConnect(host, port, null, null);
                    bool success = result.AsyncWaitHandle.WaitOne(timeoutMs);

                    if (success && client.Connected)
                    {
                        client.EndConnect(result);
                        return true;
                    }
                }
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Updates or adds a setting directly into the executable configuration file
        /// using XmlDocument to ensure original comments and layout are retained.
        /// </summary>
        private static void SaveAppSettingWithoutRemovingComments(string key, string value)
        {
            try
            {
                string configPath = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                if (!File.Exists(configPath))
                    return;

                XmlDocument doc = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                doc.Load(configPath);

                XmlNode appSettingsNode = doc.SelectSingleNode("//appSettings");
                if (appSettingsNode == null)
                    return;

                XmlElement settingElement = appSettingsNode.SelectSingleNode($"add[@key='{key}']") as XmlElement;

                if (settingElement != null)
                {
                    settingElement.SetAttribute("value", value);
                }
                else
                {
                    XmlElement newElement = doc.CreateElement("add");
                    newElement.SetAttribute("key", key);
                    newElement.SetAttribute("value", value);
                    appSettingsNode.AppendChild(newElement);
                }

                doc.Save(configPath);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch { }
        }

        public static string FtpServerPath
        {
            get
            {
                string webSocketUrl = WebSocketUrl;

                if (Uri.TryCreate(webSocketUrl, UriKind.Absolute, out Uri uri))
                {
                    return $"ftp://{uri.Host}/";
                }

                return string.Empty;
            }
        }

        public static IPAddress GetDefaultGateway()
        {
            // Try getting default gateway first
            var gateway = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up &&
                            n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties().GatewayAddresses)
                .Select(g => g.Address)
                .FirstOrDefault(a => a != null &&
                                     !a.Equals(IPAddress.Any) &&
                                     a.AddressFamily == AddressFamily.InterNetwork);

            if (gateway != null) return gateway;

            // Fallback: If no gateway is configured, return the local IP address
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up &&
                            n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties().UnicastAddresses)
                .Where(u => u.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(u.Address))
                .Select(u => u.Address)
                .FirstOrDefault();
        }
    }
}