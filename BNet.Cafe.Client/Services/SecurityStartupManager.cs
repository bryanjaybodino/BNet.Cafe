using Microsoft.Win32;
using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace BNet.Cafe.Client.Services
{
    public static class SecurityStartupManager
    {
        private const string AppRegistryName = "BNetCafeClient";

        // Checks if the local machine's IP address ends with .100 or .200 (Your Server IPs)
        private static bool IsServerIpAddress()
        {
            try
            {
                string hostName = Dns.GetHostName();
                IPHostEntry ipEntry = Dns.GetHostEntry(hostName);

                foreach (IPAddress ip in ipEntry.AddressList)
                {
                    // Check for standard IPv4 addresses
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        string ipStr = ip.ToString();

                        // Check if the IP ends with .100 or .200
                        if (ipStr.EndsWith(".100") || ipStr.EndsWith(".200"))
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
                // If it fails to query network cards, default to false (safe)
            }

            return false;
        }

        public static bool IsStartupEnabled()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false))
                {
                    object value = key?.GetValue(AppRegistryName);
                    return value != null;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool EnableStartup()
        {
            // SAFETY CHECK: Block startup registration if the machine's IP ends with .100 or .200 (Server)
            if (IsServerIpAddress())
            {
                MessageBox.Show(
                    "Startup registration is blocked because this machine's IP address indicates it is the server (.100 or .200).",
                    "Safety Block",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return false;
            }

            try
            {
                string exePath = Application.ExecutablePath;
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    key?.SetValue(AppRegistryName, $"\"{exePath}\"");
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool DisableStartup()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null)
                    {
                        if (key.GetValue(AppRegistryName) != null)
                        {
                            key.DeleteValue(AppRegistryName, false);
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}