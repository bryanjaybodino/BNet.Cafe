using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Threading;

namespace BNet.Cafe.Websocket
{
    internal static class Program
    {
        private static readonly AutoResetEvent KeepAliveEvent =
            new AutoResetEvent(false);

        static void Main(string[] args)
        {
            string taskName = "CafeWebsocketServer_AutoStart";

            // ---- 1. If the Scheduled Task already exists, skip the UAC prompt block entirely! ----
            // This ensures that on future boots or runs, it launches straight through without checking or prompting.
            if (!TaskExists(taskName))
            {
                // ---- 2. First-run only: Ensure we are running as Administrator to create the task ----
                if (!IsAdministrator())
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = Assembly.GetExecutingAssembly().Location,
                            Arguments = string.Join(" ", args),
                            Verb = "runas", // Triggers UAC prompt ONLY ONCE ever
                            UseShellExecute = true
                        });
                    }
                    catch { }
                    return; // Exit non-admin instance
                }

                // ---- 3. Register the task on first run ----
                RegisterHighestPrivilegeTask(taskName);
            }

            // ---- 4. Self-copy to a bin subdirectory if running loose in root ----
            string exePath = Assembly.GetExecutingAssembly().Location;
            string exeDir = Path.GetDirectoryName(exePath);

            string physicalDir;
            if (Path.GetFileName(exeDir).Equals("bin", StringComparison.OrdinalIgnoreCase))
                physicalDir = Directory.GetParent(exeDir).FullName + Path.DirectorySeparatorChar;
            else
                physicalDir = exeDir + Path.DirectorySeparatorChar;

            string binDir = Path.Combine(physicalDir, "bin");
            Directory.CreateDirectory(binDir);

            string targetExe = Path.Combine(binDir, Path.GetFileName(exePath));
            if (!string.Equals(Path.GetFullPath(exePath), Path.GetFullPath(targetExe), StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    File.Copy(exePath, targetExe, true);
                }
                catch { }
            }

            try
            {
                Setup server = new Setup();
                server.StartWebsocket();

                // Keep process alive
                KeepAliveEvent.WaitOne();
            }
            catch (Exception ex)
            {
                File.AppendAllText("websocket_crash_log.txt", $"{DateTime.Now}: Error: {ex}\n\n");
            }
        }

        private static bool IsAdministrator()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        private static bool TaskExists(string taskName)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "schtasks",
                    Arguments = $"/query /tn \"{taskName}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(psi))
                {
                    process.WaitForExit();
                    return process.ExitCode == 0; // Exit code 0 means the task exists
                }
            }
            catch
            {
                return false;
            }
        }

        private static void RegisterHighestPrivilegeTask(string taskName)
        {
            try
            {
                string exePath = Assembly.GetExecutingAssembly().Location;

                var psi = new ProcessStartInfo
                {
                    FileName = "schtasks",
                    Arguments = $"/create /tn \"{taskName}\" /tr \"\\\"{exePath}\\\"\" /sc ONLOGON /rl HIGHEST /f",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(psi))
                {
                    process.WaitForExit();
                }
            }
            catch { }
        }
    }
}