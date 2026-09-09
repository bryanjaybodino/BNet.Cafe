using System;
using System.Linq;
using System.Windows.Forms;

namespace BNet.Cafe.Client
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Contains(WatchdogManager.WatchdogArgument))
            {
                WatchdogManager.RunWatchdogLoop();
                return;
            }

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
    }
}