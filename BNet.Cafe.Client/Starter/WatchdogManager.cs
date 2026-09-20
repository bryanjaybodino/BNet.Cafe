using System;
using System.Diagnostics;
using System.IO;
using System.Management; // Add reference to System.Management in your project
using System.Threading;

namespace BNet.Cafe.Client
{
    public static class WatchdogManager
    {
        public const string WatchdogArgument = "--watchdog";

        public static void StartSelfWatchdog()
        {
            try
            {
                if (EnvironmentHelper.IsDevelopment) return;

                string currentExe = Process.GetCurrentProcess().MainModule.FileName;
                int currentPid = Process.GetCurrentProcess().Id;

                string commandLine = $"\"{currentExe}\" {WatchdogArgument} {currentPid}";

                // Use WMI to launch the process under 'wmiprvse.exe' instead of as a child process
                using (var processClass = new ManagementClass("Win32_Process"))
                {
                    var inParams = processClass.GetMethodParameters("Create");
                    inParams["CommandLine"] = commandLine;
                    processClass.InvokeMethod("Create", inParams, null);
                }
            }
            catch { }
        }

        public static void RunWatchdogLoop(int parentPid)
        {
            if (EnvironmentHelper.IsDevelopment) return;

            string currentExe = Process.GetCurrentProcess().MainModule.FileName;

            try
            {
                // Wait for the main app process to terminate
                Process parentProcess = Process.GetProcessById(parentPid);
                parentProcess.WaitForExit();
            }
            catch (ArgumentException)
            {
                // Main process already exited
            }

            // Wait 500ms to allow Windows to release file handles & Mutex
            Thread.Sleep(500);

            // Restart the main application via WMI as well so it starts clean
            try
            {
                using (var processClass = new ManagementClass("Win32_Process"))
                {
                    var inParams = processClass.GetMethodParameters("Create");
                    inParams["CommandLine"] = $"\"{currentExe}\"";
                    processClass.InvokeMethod("Create", inParams, null);
                }
            }
            catch
            {
                // Fallback to standard process launch if WMI fails
                Process.Start(new ProcessStartInfo
                {
                    FileName = currentExe,
                    UseShellExecute = true
                });
            }
        }
    }
}