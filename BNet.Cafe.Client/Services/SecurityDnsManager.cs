using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;

namespace BNet.Cafe.Client.Services
{
    public static class SecurityDnsManager
    {
        // Cloudflare Family / CleanBrowsing Adult Filter DNS (Enables Anti-Porn blocking)
        private static readonly string[] PornBlockDnsPrimary = { "1.1.1.3", "1.0.0.3" };

        // Cloudflare Gaming / Fast Performance DNS (Used when Anti-Porn is Disabled)
        private static readonly string[] FastPerformanceDnsPrimary = { "1.1.1.1", "1.0.0.1" };

        /// <summary>
        /// Checks if the primary network adapter is currently using the porn-blocking DNS.
        /// </summary>
        public static bool IsAntiPornEnabled()
        {
            try
            {
                var ipv4Addresses = GetActiveDnsAddresses();
                if (ipv4Addresses != null && ipv4Addresses.Length > 0)
                {
                    // Check if current DNS matches the filtering/blocking primary DNS
                    return PornBlockDnsPrimary.Contains(ipv4Addresses[0]);
                }
            }
            catch
            {
                // Default fallback if check fails
            }
            return false;
        }

        /// <summary>
        /// Enables anti-porn website blocking by setting family-safe DNS.
        /// </summary>
        public static bool EnableAntiPorn()
        {
            return SetDns(PornBlockDnsPrimary[0], PornBlockDnsPrimary[1]);
        }

        /// <summary>
        /// Disables anti-porn blocking and sets high-performance gaming/Cloudflare DNS.
        /// </summary>
        public static bool DisableAntiPorn()
        {
            return SetDns(FastPerformanceDnsPrimary[0], FastPerformanceDnsPrimary[1]);
        }

        private static bool SetDns(string primaryDns, string secondaryDns)
        {
            try
            {
                // Find the active network interface name connected to the internet
                string interfaceName = GetActiveInterfaceName();
                if (string.IsNullOrEmpty(interfaceName)) return false;

                // PowerShell command to set IPv4 DNS servers statically on the active adapter
                string psCommand = $"Set-DnsClientServerAddress -InterfaceAlias \"{interfaceName}\" -ServerAddresses ('{primaryDns}','{secondaryDns}')";

                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -WindowStyle Hidden -Command \"{psCommand}\"",
                    Verb = "runas",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                using (var process = Process.Start(psi))
                {
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private static string GetActiveInterfaceName()
        {
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up &&
                    nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                {
                    var props = nic.GetIPProperties();
                    // Make sure it has a gateway (active internet connection)
                    if (props.GatewayAddresses.Count > 0)
                    {
                        return nic.Name;
                    }
                }
            }
            return null;
        }

        private static string[] GetActiveDnsAddresses()
        {
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up &&
                    nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                {
                    var props = nic.GetIPProperties();
                    if (props.GatewayAddresses.Count > 0)
                    {
                        return props.DnsAddresses
                            .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            .Select(ip => ip.ToString())
                            .ToArray();
                    }
                }
            }
            return new string[0];
        }
    }
}