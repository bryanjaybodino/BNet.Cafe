using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace BNet.Cafe.Client.Services
{
    public static class SecurityStartupManager
    {
        private const string AppRegistryName = "BNetCafeClient";

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
                    key?.SetValue(AppRegistryName, null);
                    key?.DeleteValue(AppRegistryName, false);
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