using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace BNet.Cafe.Client
{
    internal static class Program
    {
        [STAThread]
        [DllImport("shcore.dll", SetLastError = true)]
        private static extern bool SetProcessDpiAwareness(int awareness);

        static void Main(string[] args)
        {
            // MODE 1: Secret background watchdog loop
            if (args.Contains("--watchdog"))
            {
                RunWatchdogLoop();
                return;
            }

            // MODE 2: Normal WinForms GUI Application
            try
            {
                SetProcessDpiAwareness(2);
            }
            catch { }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Launch secondary instance of this EXE in watchdog mode
            StartSelfWatchdog();

            Application.Run(new MainForm());
        }

        private static void StartSelfWatchdog()
        {
            try
            {
                if (IsDevelopment)
                {
                    // Don't run watchdog in development mode
                    return;
                }
                string currentExe = Process.GetCurrentProcess().MainModule.FileName;
                string processName = Path.GetFileNameWithoutExtension(currentExe);

                // Only launch watchdog if one isn't already running
                var currentProcesses = Process.GetProcessesByName(processName);
                if (currentProcesses.Length <= 1)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = currentExe,
                        Arguments = "--watchdog",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden
                    });
                }
            }
            catch { }
        }

        private static void RunWatchdogLoop()
        {
            if(IsDevelopment)
            {
                // Don't run watchdog in development mode
                return;
            }
            string currentExe = Process.GetCurrentProcess().MainModule.FileName;
            string processName = Path.GetFileNameWithoutExtension(currentExe);

            while (true)
            {
                Thread.Sleep(1000); // Poll process state every 1 second

                var instances = Process.GetProcessesByName(processName);

                // If only 1 instance exists, the main UI app was closed or killed
                if (instances.Length <= 1)
                {
                    // Relaunch the main UI application
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = currentExe,
                        UseShellExecute = true
                    });

                    // Terminate this background watchdog so the newly spawned app can take over
                    break;
                }
            }
        }
        public static bool IsDevelopment
        {
            get
            {
                string exePath = AppDomain.CurrentDomain.BaseDirectory.ToLower();
                if (exePath.Contains(@"\bin"))
                    return true;

                return false;
            }
        }
    }
}