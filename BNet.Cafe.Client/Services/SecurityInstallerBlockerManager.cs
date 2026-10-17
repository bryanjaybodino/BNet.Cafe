using Microsoft.Win32;
using System;
using System.Security.Principal;
using System.Windows.Forms;

namespace BNet.Cafe.Client.Services
{
    public static class SecurityInstallerBlockerManager
    {
        // Path to control Windows Installer (MSI) restrictions
        private const string MsiInstallerPath = @"SOFTWARE\Policies\Microsoft\Windows\Installer";

        // Always use the 64-bit registry view, even if the app is compiled as x86.
        private static RegistryKey OpenHklm64() =>
            RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

        public static bool IsElevated()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                if (identity.IsSystem) return true;
                return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        /// <summary>
        /// Blocks software installations (MSI packages) safely without disrupting core system apps or startup.
        /// Must run as Administrator / SYSTEM.
        /// </summary>
        public static bool DisableInstallers()
        {
            try
            {
                if (!IsElevated()) return false;

                using (var hklm = OpenHklm64())
                {
                    // Disable Windows Installer for all users (Value 2 = Disable for all users)
                    using (var msiKey = hklm.CreateSubKey(MsiInstallerPath))
                    {
                        msiKey?.SetValue("DisableMSI", 2, RegistryValueKind.DWord);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Re-enables software installations.
        /// </summary>
        public static bool EnableInstallers()
        {
            try
            {
                if (!IsElevated()) return false;

                using (var hklm = OpenHklm64())
                {
                    // Remove MSI restrictions
                    using (var msiKey = hklm.OpenSubKey(MsiInstallerPath, writable: true))
                    {
                        msiKey?.DeleteValue("DisableMSI", throwOnMissingValue: false);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool AreInstallersDisabled()
        {
            try
            {
                using (var hklm = OpenHklm64())
                {
                    using (var key = hklm.OpenSubKey(MsiInstallerPath, writable: false))
                    {
                        return key?.GetValue("DisableMSI") is int val && val >= 1;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public static void HandleServerCommand(string command)
        {
            if (string.Equals(command, "DISABLE_INSTALLERS", StringComparison.OrdinalIgnoreCase))
            {
                if (DisableInstallers())
                    ShowNotification("App Security", "Software installations have been successfully blocked.");
                else
                    ShowNotification("App Security", "Failed to block installers. Make sure the client runs as Administrator.");
            }
            else if (string.Equals(command, "ENABLE_INSTALLERS", StringComparison.OrdinalIgnoreCase))
            {
                if (EnableInstallers())
                    ShowNotification("App Security", "Software installations have been enabled.");
                else
                    ShowNotification("App Security", "Failed to enable installers. Make sure the client runs as Administrator.");
            }
        }

        // ---------- helpers ----------

        private static void ShowNotification(string title, string message)
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.ServiceNotification);
        }
    }
}