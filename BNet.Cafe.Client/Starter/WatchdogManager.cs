using System;
using System.Diagnostics;
using System.IO;
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

                string currentExe = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(currentExe)) return;

                string processName = Path.GetFileNameWithoutExtension(currentExe);

                var currentProcesses = Process.GetProcessesByName(processName);
                if (currentProcesses.Length <= 1)
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = currentExe,
                        Arguments = WatchdogArgument,
                        CreateNoWindow = true,
                        UseShellExecute = true, // Must be true to break the process tree group
                        WindowStyle = ProcessWindowStyle.Hidden
                    };

                    Process.Start(startInfo);
                }
            }
            catch { }
        }

        public static void RunWatchdogLoop()
        {
            if (EnvironmentHelper.IsDevelopment) return;

            string currentExe = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(currentExe)) return;

            string processName = Path.GetFileNameWithoutExtension(currentExe);
            int currentPid = Process.GetCurrentProcess().Id;

            while (true)
            {
                Thread.Sleep(1500);

                // Find all running instances except this watchdog instance itself
                var matchingProcesses = Process.GetProcessesByName(processName);
                bool mainAppRunning = false;

                foreach (var proc in matchingProcesses)
                {
                    if (proc.Id != currentPid)
                    {
                        mainAppRunning = true;
                        proc.Dispose();
                        break;
                    }
                    proc.Dispose();
                }

                // If the main application is no longer running, restart it
                if (!mainAppRunning)
                {
                    // Allow time for mutexes and file locks to be released by Windows
                    Thread.Sleep(1000);

                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = currentExe,
                            UseShellExecute = true
                        });
                    }
                    catch { }

                    // Exit watchdog loop after initiating restart
                    break;
                }
            }
        }
    }
}