using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Web;

namespace BNet.Cafe.Server.Services
{
    public class NetworkUtility
    {
        public static string GetLocalIPAddress()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Filter for active Operational status and non-loopback interfaces
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    var ipProps = ni.GetIPProperties();

                    foreach (UnicastIPAddressInformation ip in ipProps.UnicastAddresses)
                    {
                        // Filter for IPv4 addresses
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return ip.Address.ToString();
                        }
                    }
                }
            }
            throw new Exception("No active network adapter with an IPv4 address was found.");
        }
    }
}