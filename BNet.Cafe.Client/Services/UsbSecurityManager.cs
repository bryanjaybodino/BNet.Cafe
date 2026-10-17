using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Windows.Forms;

namespace BNet.Cafe.Client.Services
{
    public static class UsbSecurityManager
    {
        private const string UsbStorPath = @"SYSTEM\CurrentControlSet\Services\USBSTOR";
        private const string MtpDriverPath = @"SYSTEM\CurrentControlSet\Services\WUDFWpdMtp";
        private const string RemovableStorageRoot = @"SOFTWARE\Policies\Microsoft\Windows\RemovableStorageDevices";
        private const string DeviceInstallPath = @"SOFTWARE\Policies\Microsoft\Windows\DeviceInstall\Restrictions";

        // Device SETUP class GUID for Windows Portable Devices (phones / MTP / PTP)
        private const string WpdSetupClassGuid = "{eec5ad98-8080-425f-922a-dabf3de3f69a}";

        // Class GUIDs understood by the "Removable Storage Access" policy
        private static readonly string[] BlockedClassGuids =
        {
            "{53f56307-b6bf-11d0-94f2-00a0c91efb8b}", // Removable disks (flash drives / external HDD)
            "{6AC27878-A6FA-4155-BA85-F98F491D4F33}", // WPD devices 1
            "{F33FDC04-D1AC-4E8D-9A30-19BBD4B108AE}"  // WPD devices 2
        };

        // ---------- plug-in detection ----------

        /// <summary>
        /// Raised (on a background thread) when a USB storage device or phone is plugged in
        /// while blocking is active. The argument is the device name.
        /// </summary>
        public static event Action<string> BlockedDeviceDetected;

        private static ManagementEventWatcher _watcher;
        private static readonly object WatchLock = new object();
        private static DateTime _lastAlertUtc = DateTime.MinValue;

        private static readonly string[] StorageServices =
            { "USBSTOR", "UASPStor", "WUDFWpdMtp", "WUDFWpdFs", "WpdUpFltr" };

        private static readonly string[] StorageClasses =
            { "WPD", "DiskDrive", "CDROM", "USBSTOR" };

        // USB class codes in hardware/compatible IDs: Class_08 = mass storage,
        // Class_06 = still image (PTP, iPhone), MS_COMP_MTP = Android file transfer
        private static readonly string[] StorageIdMarkers =
            { "Class_08", "Class_06", "MS_COMP_MTP" };

        /// <summary>
        /// Starts watching for newly plugged-in devices. Call once at app start.
        /// </summary>
        public static void StartMonitoring()
        {
            lock (WatchLock)
            {
                if (_watcher != null) return;
                try
                {
                    var query = new WqlEventQuery(
                        "__InstanceCreationEvent",
                        TimeSpan.FromSeconds(1),
                        "TargetInstance ISA 'Win32_PnPEntity'");

                    _watcher = new ManagementEventWatcher(query);
                    _watcher.EventArrived += OnPnpEntityCreated;
                    _watcher.Start();
                }
                catch
                {
                    _watcher = null;
                }
            }
        }

        public static void StopMonitoring()
        {
            lock (WatchLock)
            {
                if (_watcher == null) return;
                try
                {
                    _watcher.EventArrived -= OnPnpEntityCreated;
                    _watcher.Stop();
                    _watcher.Dispose();
                }
                catch { }
                _watcher = null;
            }
        }

        private static void OnPnpEntityCreated(object sender, EventArrivedEventArgs e)
        {
            try
            {
                if (!IsUsbDisabled()) return;

                var dev = e.NewEvent["TargetInstance"] as ManagementBaseObject;
                if (dev == null) return;

                string deviceId = dev["DeviceID"] as string ?? "";
                string name = dev["Name"] as string ?? "USB device";
                string pnpClass = dev["PNPClass"] as string ?? "";
                string service = dev["Service"] as string ?? "";

                if (deviceId.IndexOf("USB", StringComparison.OrdinalIgnoreCase) < 0 &&
                    deviceId.IndexOf("WPDBUSENUM", StringComparison.OrdinalIgnoreCase) < 0)
                    return;

                if (!LooksLikeStorageOrPhone(dev, deviceId, pnpClass, service)) return;

                // One plug-in creates several device nodes; only alert once per few seconds
                if ((DateTime.UtcNow - _lastAlertUtc).TotalSeconds < 5) return;
                _lastAlertUtc = DateTime.UtcNow;

                BlockedDeviceDetected?.Invoke(name);
            }
            catch { }
        }

        private static bool LooksLikeStorageOrPhone(
            ManagementBaseObject dev, string deviceId, string pnpClass, string service)
        {
            if (StorageClasses.Any(c => string.Equals(c, pnpClass, StringComparison.OrdinalIgnoreCase)))
                return true;

            if (StorageServices.Any(s => string.Equals(s, service, StringComparison.OrdinalIgnoreCase)))
                return true;

            if (deviceId.StartsWith("USBSTOR", StringComparison.OrdinalIgnoreCase) ||
                deviceId.IndexOf("WPDBUSENUM", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            var ids = ((dev["HardwareID"] as string[]) ?? new string[0])
                .Concat((dev["CompatibleID"] as string[]) ?? new string[0]);

            return ids.Any(id => StorageIdMarkers.Any(m =>
                id.IndexOf(m, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        // ---------- blocking ----------

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
        /// Blocks USB storage and smartphones (MTP/PTP). Must run as admin / SYSTEM.
        /// </summary>
        public static bool DisableUsb()
        {
            try
            {
                if (!IsElevated()) return false;

                using (var hklm = OpenHklm64())
                {
                    // 1. Stop the USB mass-storage driver (4 = disabled)
                    SetStart(hklm, UsbStorPath, 4);

                    // 2. Stop the Android MTP driver from loading (4 = disabled)
                    SetStart(hklm, MtpDriverPath, 4);

                    // 3. Removable Storage Access policy
                    using (var root = hklm.CreateSubKey(RemovableStorageRoot))
                        root?.SetValue("Deny_All", 1, RegistryValueKind.DWord);

                    foreach (var guid in BlockedClassGuids)
                    {
                        using (var key = hklm.CreateSubKey($@"{RemovableStorageRoot}\{guid}"))
                        {
                            if (key == null) continue;
                            key.SetValue("Deny_Read", 1, RegistryValueKind.DWord);
                            key.SetValue("Deny_Write", 1, RegistryValueKind.DWord);
                            key.SetValue("Deny_Execute", 1, RegistryValueKind.DWord);
                        }
                    }

                    // 4. Device Installation Restrictions: deny the WPD (phone) class, retroactively
                    using (var restrictions = hklm.CreateSubKey(DeviceInstallPath))
                    {
                        restrictions?.SetValue("DenyDeviceClasses", 1, RegistryValueKind.DWord);
                        restrictions?.SetValue("DenyDeviceClassesRetroactive", 1, RegistryValueKind.DWord);
                    }
                    using (var list = hklm.CreateSubKey(DeviceInstallPath + @"\DenyDeviceClasses"))
                        list?.SetValue("1", WpdSetupClassGuid, RegistryValueKind.String);
                }

                // 5. Disable any phone that is plugged in right now
                RunPowerShell("Get-PnpDevice -Class WPD -PresentOnly -ErrorAction SilentlyContinue | " +
                              "Disable-PnpDevice -Confirm:$false -ErrorAction SilentlyContinue");

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Re-enables USB storage and smartphones.
        /// </summary>
        public static bool EnableUsb()
        {
            try
            {
                if (!IsElevated()) return false;

                using (var hklm = OpenHklm64())
                {
                    SetStart(hklm, UsbStorPath, 3);   // 3 = manual (default)
                    SetStart(hklm, MtpDriverPath, 3);

                    hklm.DeleteSubKeyTree(RemovableStorageRoot, throwOnMissingSubKey: false);

                    hklm.DeleteSubKeyTree(DeviceInstallPath + @"\DenyDeviceClasses", throwOnMissingSubKey: false);
                    using (var restrictions = hklm.OpenSubKey(DeviceInstallPath, writable: true))
                    {
                        restrictions?.DeleteValue("DenyDeviceClasses", throwOnMissingValue: false);
                        restrictions?.DeleteValue("DenyDeviceClassesRetroactive", throwOnMissingValue: false);
                    }
                }

                // Re-enable phone devices that were disabled earlier
                RunPowerShell("Get-PnpDevice -Class WPD -ErrorAction SilentlyContinue | " +
                              "Enable-PnpDevice -Confirm:$false -ErrorAction SilentlyContinue");

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsUsbDisabled()
        {
            try
            {
                using (var hklm = OpenHklm64())
                {
                    bool storageOff;
                    using (var key = hklm.OpenSubKey(UsbStorPath, writable: false))
                        storageOff = key?.GetValue("Start") is int start && start == 4;

                    bool wpdDenied;
                    using (var key = hklm.OpenSubKey(DeviceInstallPath, writable: false))
                        wpdDenied = key?.GetValue("DenyDeviceClasses") is int deny && deny == 1;

                    return storageOff && wpdDenied;
                }
            }
            catch
            {
                return false;
            }
        }

        public static void HandleServerCommand(string command)
        {
            if (string.Equals(command, "DISABLE_USB", StringComparison.OrdinalIgnoreCase))
            {
                if (DisableUsb())
                    ShowNotification("USB Security",
                        "USB storage and mobile devices have been blocked.\nIf a phone is already connected, unplug and reconnect it.");
                else
                    ShowNotification("USB Security",
                        "Failed to block USB. Make sure the client runs as Administrator.");
            }
            else if (string.Equals(command, "ENABLE_USB", StringComparison.OrdinalIgnoreCase))
            {
                if (EnableUsb())
                    ShowNotification("USB Security", "USB storage and mobile devices have been enabled.");
                else
                    ShowNotification("USB Security",
                        "Failed to enable USB. Make sure the client runs as Administrator.");
            }
        }

        // ---------- helpers ----------

        private static void SetStart(RegistryKey hklm, string servicePath, int value)
        {
            using (var key = hklm.OpenSubKey(servicePath, writable: true))
            {
                key?.SetValue("Start", value, RegistryValueKind.DWord);
            }
        }

        private static void RunPowerShell(string script)
        {
            try
            {
                // A 32-bit process on 64-bit Windows must use Sysnative to reach 64-bit PowerShell
                string windir = Environment.GetEnvironmentVariable("windir") ?? @"C:\Windows";
                string sysDir = (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess)
                    ? "Sysnative" : "System32";
                string exe = $@"{windir}\{sysDir}\WindowsPowerShell\v1.0\powershell.exe";

                var psi = new ProcessStartInfo(exe,
                    $"-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -Command \"{script}\"")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(psi)?.WaitForExit(15000);
            }
            catch { }
        }

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