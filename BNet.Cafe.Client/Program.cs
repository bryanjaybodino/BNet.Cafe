using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BNet.Cafe.Client
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        [DllImport("shcore.dll", SetLastError = true)]
        private static extern bool SetProcessDpiAwareness(int awareness);
        static void Main()
        {
            // Set DPI awareness before creating forms
            try
            {
                SetProcessDpiAwareness(2); // PROCESS_PER_MONITOR_DPI_AWARE
            }
            catch
            {
                // Fallback for older Windows versions
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ClientScreen());
        }
    }
}
