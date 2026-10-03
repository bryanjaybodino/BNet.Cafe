using BNet.Cafe.Client.Models; // Adjust namespace as necessary for ActiveWindowInfo
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace BNet.Cafe.Client.Services
{
    public static class SecurityAccessManager
    {
        // Win32 API Imports
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        private const uint WM_CLOSE = 0x0010;

        /// <summary>
        /// Scans active processes and titles against security rules. 
        /// Kills TaskManager, CMD, and PowerShell processes and safely closes restricted windows.
        /// </summary>
        /// <returns>True if restricted access was detected and handled; otherwise, false.</returns>
        public static bool EnforceRestrictions(Repositories.UserActivity.ActiveWindowInfo info)
        {
            string processName = info?.ProcessName ?? string.Empty;
            string windowTitle = info?.WindowTitle ?? string.Empty;

            string userStartup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string commonStartup = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);

            // Process & Window Checks
            bool isTaskManager = processName.Equals("Taskmgr", StringComparison.OrdinalIgnoreCase);

            bool isCmdPrompt = processName.Equals("cmd", StringComparison.OrdinalIgnoreCase) ||
                               processName.Equals("cmd.exe", StringComparison.OrdinalIgnoreCase) ||
                               windowTitle.IndexOf("Command Prompt", StringComparison.OrdinalIgnoreCase) >= 0;

            bool isPowerShell = processName.Equals("powershell", StringComparison.OrdinalIgnoreCase) ||
                                processName.Equals("powershell_ise", StringComparison.OrdinalIgnoreCase) ||
                                processName.Equals("pwsh", StringComparison.OrdinalIgnoreCase) || // PowerShell Core (v6+)
                                windowTitle.IndexOf("Windows PowerShell", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                windowTitle.IndexOf("PowerShell", StringComparison.OrdinalIgnoreCase) >= 0;

            bool isControlPanel = windowTitle.IndexOf("Programs and Features", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isStartupFolder = windowTitle.IndexOf("Startup", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   windowTitle.IndexOf(userStartup, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   windowTitle.IndexOf(commonStartup, StringComparison.OrdinalIgnoreCase) >= 0;

            // Checks root C:\ drive and critical Windows/Network directories
            bool isRestrictedPath = IsRestrictedPathWindow(windowTitle);

            if (isTaskManager || isCmdPrompt || isPowerShell || isControlPanel || isStartupFolder || isRestrictedPath)
            {
                try
                {
                    // Kill Task Manager instances
                    KillProcessByName("Taskmgr", "Task Manager");

                    // Kill Command Prompt instances
                    KillProcessByName("cmd", "Command Prompt");

                    // Kill PowerShell instances (Windows PowerShell, ISE, and PowerShell Core)
                    KillProcessByName("powershell", "PowerShell");
                    KillProcessByName("powershell_ise", "PowerShell ISE");
                    KillProcessByName("pwsh", "PowerShell");

                    // Safely close target windows across all top-level classes
                    CloseTargetExplorerWindows();
                }
                catch
                {
                    // Handle or log process cleanup exceptions if needed
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Kills all active processes with the target name and displays a standard restriction message.
        /// </summary>
        private static void KillProcessByName(string processName, string displayName)
        {
            var processes = Process.GetProcessesByName(processName);
            if (processes.Length > 0)
            {
                foreach (var proc in processes)
                {
                    try
                    {
                        proc.Kill();
                    }
                    catch
                    {
                        // Process may have already exited
                    }
                }
                ShowRestrictionMessage($"You have no access to open {displayName}.");
            }
        }

        /// <summary>
        /// Enumerates open windows to close specific target Explorer, Dialog, or Settings windows safely.
        /// </summary>
        public static void CloseTargetExplorerWindows()
        {
            string userStartup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string commonStartup = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);

            EnumWindows((hWnd, lParam) =>
            {
                StringBuilder titleBuilder = new StringBuilder(256);
                GetWindowText(hWnd, titleBuilder, titleBuilder.Capacity);
                string title = titleBuilder.ToString();

                if (!string.IsNullOrEmpty(title))
                {
                    bool isTargetWindow = title.IndexOf("Programs and Features", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                         title.IndexOf("Startup", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                         title.IndexOf(userStartup, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                         title.IndexOf(commonStartup, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                         IsRestrictedPathWindow(title);

                    if (isTargetWindow)
                    {
                        SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                        ShowRestrictionMessage("You have no access to open " + title);
                    }
                }
                return true; // Continue enumeration
            }, IntPtr.Zero);
        }

        /// <summary>
        /// Helper method to identify if the window title represents restricted drive paths, system folders, or network applets.
        /// </summary>
        private static bool IsRestrictedPathWindow(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return false;

            string trimmedTitle = title.Trim();

            // 1. Root C:\ drive match
            bool isRootCDrive = trimmedTitle.Equals(@"C:\", StringComparison.OrdinalIgnoreCase) ||
                                trimmedTitle.Equals(@"C:\ - File Explorer", StringComparison.OrdinalIgnoreCase) ||
                                trimmedTitle.Equals("C:", StringComparison.OrdinalIgnoreCase) ||
                                trimmedTitle.Equals("Local Disk (C:)", StringComparison.OrdinalIgnoreCase) ||
                                trimmedTitle.Equals("OS (C:)", StringComparison.OrdinalIgnoreCase) ||
                                trimmedTitle.EndsWith(@":\ (C:)", StringComparison.OrdinalIgnoreCase);

            if (isRootCDrive) return true;

            // 2. Critical Windows System Folders & Network Control Applets
            string[] criticalFolders = new string[]
            {
                "Network and Sharing Center",
                "Network and Internet",
                "Control Panel",
                "System and Security",
                "Wi-Fi Status",
                "Wi-Fi Properties",
                "Ethernet Status",
                "Ethernet Properties",
                "Network Connections",
                "Program Files",
                "Program Files (x86)",
                "C:\\Windows",
                "System32",
                "SysWOW64",
                "ProgramData",
                "System Volume Information",
                "$Recycle.Bin",
                "PerfLogs"
            };

            foreach (string folder in criticalFolders)
            {
                // Simple substring match handles titles like "Control Panel\Network and Internet\Network and Sharing Center"
                if (trimmedTitle.IndexOf(folder, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Helper to standardise service notification message boxes.
        /// </summary>
        private static void ShowRestrictionMessage(string message)
        {
            MessageBox.Show(
                message,
                "User Restriction",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.ServiceNotification
            );
        }
    }
}