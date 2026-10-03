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

        private static string GetOrCreateSetting(string key, string defaultValue)
        {
            string value = ConfigurationManager.AppSettings[key];

            if (!string.IsNullOrWhiteSpace(value))
                return value;

            Configuration config = ConfigurationManager
                .OpenExeConfiguration(ConfigurationUserLevel.None);

            if (config.AppSettings.Settings[key] == null)
            {
                config.AppSettings.Settings.Add(key, defaultValue);
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }

            return defaultValue;
        }

        public static bool IsAccountCreationAllowed
        {
            get
            {
                string value = GetOrCreateSetting("AccountCreationAllowed", "true");

                // Returns false only if the value equals "false" (case-insensitive); defaults to true otherwise
                return !string.Equals(value?.Trim(), "false", StringComparison.OrdinalIgnoreCase);
            }
        }
        public static bool IsDesktopSlideShow
        {
            get
            {
                string value = GetOrCreateSetting("DesktopSlideShow", "false");
                // Returns false only if the value equals "false" (case-insensitive); defaults to true otherwise
                return !string.Equals(value?.Trim(), "false", StringComparison.OrdinalIgnoreCase);
            }
        }
        
        public static bool IsOverlayFreeze
        {
            get
            {
                string value = GetOrCreateSetting("OverlayFreeze", "false");
                return !string.Equals(value?.Trim(), "false", StringComparison.OrdinalIgnoreCase);
            }
        }

        public static string WebSocketUrl
        {
            get
            {
                return GetOrCreateSetting(
                    "WebSocketUrl",
                    "ws://localhost:8080");
            }
        }

        public static string AutoShutDownInterval
        {
            get
            {
                string rawValue = GetOrCreateSetting("AutoShutDownInterval", "300");

                if (int.TryParse(rawValue?.Trim(), out int timeout))
                {
                    // Enforce minimum threshold of 60 seconds (1 minute)
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
                return GetOrCreateSetting(
                    "AppUrl",
                    "http://localhost:5000");
            }
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
    }
}