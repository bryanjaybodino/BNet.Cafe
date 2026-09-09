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

                string currentExe = Process.GetCurrentProcess().MainModule.FileName;
                string processName = Path.GetFileNameWithoutExtension(currentExe);

                var currentProcesses = Process.GetProcessesByName(processName);
                if (currentProcesses.Length <= 1)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = currentExe,
                        Arguments = WatchdogArgument,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden
                    });
                }
            }
            catch { }
        }

        public static void RunWatchdogLoop()
        {
            if (EnvironmentHelper.IsDevelopment) return;

            string currentExe = Process.GetCurrentProcess().MainModule.FileName;
            string processName = Path.GetFileNameWithoutExtension(currentExe);

            while (true)
            {
                Thread.Sleep(1000);

                var instances = Process.GetProcessesByName(processName);

                if (instances.Length <= 1)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = currentExe,
                        UseShellExecute = true
                    });

                    break;
                }
            }
        }
    }
}