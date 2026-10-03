using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BNet.Cafe.Client.Services
{
    internal class CommandPromptService
    {
        public static void ShutdownSystem()
        {
            try
            {
               // Process.Start("shutdown", "/s /t 10 /f /c \"No physical activity detected. PC shutting down.\"");
               // Application.Exit();
            }
            catch { }
        }
        public static void RestartSystem()
        {
            try
            {
                Process.Start("shutdown", "/r /t 10 /f /c \"Restart requested by server.\"");
                Application.Exit();
            }
            catch { }
        }
    }
}
