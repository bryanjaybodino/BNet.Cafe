using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace BNet.Cafe.Client
{
    internal static class Program
    {
        // Unique app identifier string for the Mutex
        private static readonly string MutexName = @"Global\BNet_Cafe_Client_SingleInstance_Mutex";
        private static Mutex _mutex;

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Contains(WatchdogManager.WatchdogArgument))
            {
                WatchdogManager.RunWatchdogLoop();
                return;
            }

            // Create system-wide mutex to check if another instance is running
            _mutex = new Mutex(true, MutexName, out bool createdNew);

            if (!createdNew)
            {
                // Another instance is already running; exit immediately
                _mutex.Dispose();
                return;
            }

            try
            {
                try
                {
                    NativeMethods.SetProcessDpiAwareness(2);
                }
                catch { }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                WatchdogManager.StartSelfWatchdog();

                Application.Run(new MainForm());
            }
            finally
            {
                // Ensure the mutex is released when the main app shuts down
                if (_mutex != null)
                {
                    try
                    {
                        _mutex.ReleaseMutex();
                    }
                    catch { }
                    _mutex.Dispose();
                }
            }
        }
    }
}