using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace BNet.Cafe.Client
{
    internal static class Program
    {
        private static readonly string MutexName = @"Global\BNet_Cafe_Client_SingleInstance_Mutex";
        private static Mutex _mutex;

        [STAThread]
        static void Main(string[] args)
        {
            // Set unhandled exception mode to catch Windows Forms thread exceptions
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            // Catch exceptions thrown on UI threads (including async MainForm_Load)
            Application.ThreadException += (sender, e) =>
            {
                HandleFatalException(e.Exception);
            };

            // Catch non-UI thread exceptions
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                {
                    HandleFatalException(ex);
                }
            };

            if (args.Contains(WatchdogManager.WatchdogArgument))
            {
                WatchdogManager.RunWatchdogLoop();
                return;
            }

            _mutex = new Mutex(true, MutexName, out bool createdNew);

            if (!createdNew)
            {
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

        private static void HandleFatalException(Exception ex)
        {
            if (ex is FileNotFoundException fnfEx)
            {
                MessageBox.Show(
                    $"Missing required assembly file: {fnfEx.FileName}\n\nPlease place the missing DLL in the application directory.",
                    @"Missing Dependency",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            else
            {
                MessageBox.Show(
                    $"Fatal application error: {ex.Message}",
                    @"Startup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }

            // Terminate process immediately to stop watchdog loop
            KillBNetClient();
        }



        public static void KillBNetClient()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "taskkill.exe",
                    Arguments = "/IM BNet.Cafe.Client.exe /F",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                Process.Start(psi);
            }
            catch
            {
                // Handle exception if taskkill fails or permissions are denied
            }
        }
    }
}