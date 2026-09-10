using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BNet.Cafe.Client.Services
{
    internal class NetworkUtility
    {
        private static string _cachedIpAddress;
        private static readonly object _lock = new object();

        public static string GetLocalIPAddress()
        {
            if (!string.IsNullOrEmpty(_cachedIpAddress))
            {
                return _cachedIpAddress;
            }

            lock (_lock)
            {
                if (!string.IsNullOrEmpty(_cachedIpAddress))
                {
                    return _cachedIpAddress;
                }

                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus == OperationalStatus.Up &&
                        ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        var ipProps = ni.GetIPProperties();

                        foreach (UnicastIPAddressInformation ip in ipProps.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                _cachedIpAddress = ip.Address.ToString();
                                return _cachedIpAddress;
                            }
                        }
                    }
                }

                // Fallback to loopback address instead of throwing an exception
                _cachedIpAddress =  "127.0.0.1";
                return _cachedIpAddress;
            }
        }

        public static void ClearCache()
        {
            lock (_lock)
            {
                _cachedIpAddress = null;
            }
        }
    }
}