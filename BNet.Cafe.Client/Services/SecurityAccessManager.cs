using BNet.Cafe.Client.Models; // Adjust namespace as necessary for ActiveWindowInfo
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Policy;
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
        /// Kills TaskManager processes and safely closes restricted File Explorer windows.
        /// </summary>
        /// <returns>True if restricted access was detected and handled; otherwise, false.</returns>
        public static bool EnforceRestrictions(Repositories.UserActivity.ActiveWindowInfo info)
        {
            string processName = info?.ProcessName ?? string.Empty;
            string windowTitle = info?.WindowTitle ?? string.Empty;

            string userStartup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string commonStartup = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);

            bool isTaskManager = processName.Equals("Taskmgr", StringComparison.OrdinalIgnoreCase);
            bool isControlPanel = windowTitle.IndexOf("Programs and Features", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isStartupFolder = windowTitle.IndexOf("Startup", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   windowTitle.IndexOf(userStartup, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   windowTitle.IndexOf(commonStartup, StringComparison.OrdinalIgnoreCase) >= 0;

            // Checks root C:\ drive and critical Windows directories while leaving user folders (e.g., C:\Anyfolders) open
            bool isRestrictedPath = IsRestrictedPathWindow(windowTitle);

            if (isTaskManager || isControlPanel || isStartupFolder || isRestrictedPath)
            {
                try
                {
                    // Kill Task Manager instances
                    foreach (var proc in Process.GetProcessesByName("Taskmgr"))
                    {
                        proc.Kill();
                        MessageBox.Show(
                            "You have no access to open the task manager",
                            "User Restriction",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1,
                            MessageBoxOptions.ServiceNotification
                        );
                    }

                    // Safely close target Explorer windows
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
        /// Enumerates open windows to close specific target Explorer windows safely without killing explorer.exe.
        /// </summary>
        public static void CloseTargetExplorerWindows()
        {
            string userStartup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string commonStartup = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);

            EnumWindows((hWnd, lParam) =>
            {
                // Verify window class is standard CabinetWClass (File Explorer window)
                StringBuilder classBuilder = new StringBuilder(256);
                GetClassName(hWnd, classBuilder, classBuilder.Capacity);

                if (classBuilder.ToString().Equals("CabinetWClass", StringComparison.OrdinalIgnoreCase))
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
                            MessageBox.Show(
                                "You have no access to open " + title,
                                "User Restriction",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error,
                                MessageBoxDefaultButton.Button1,
                                MessageBoxOptions.ServiceNotification
                            );
                        }
                    }
                }
                return true; // Continue enumeration
            }, IntPtr.Zero);
        }

        /// <summary>
        /// Helper method to identify if the Explorer window represents the root C:\ drive or critical Windows folders.
        /// </summary>
        private static bool IsRestrictedPathWindow(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return false;

            string trimmedTitle = title.Trim();

            // 1. Root C:\ drive match
            bool isRootCDrive = trimmedTitle.Equals(@"C:\", StringComparison.OrdinalIgnoreCase) ||
                                trimmedTitle.Equals("C:", StringComparison.OrdinalIgnoreCase) ||
                                trimmedTitle.Equals("Local Disk (C:)", StringComparison.OrdinalIgnoreCase) ||
                                trimmedTitle.Equals("OS (C:)", StringComparison.OrdinalIgnoreCase) ||
                                trimmedTitle.EndsWith(@":\ (C:)", StringComparison.OrdinalIgnoreCase);

            if (isRootCDrive) return true;

            // 2. Critical Windows System Folders
            string[] criticalFolders = new string[]
            {
                "Program Files",
                "Program Files (x86)",
                "Windows",
                "System32",
                "SysWOW64",
                "ProgramData",
                "System Volume Information",
                "$Recycle.Bin",
                "PerfLogs"
            };

            foreach (string folder in criticalFolders)
            {
                if (trimmedTitle.Equals(folder, StringComparison.OrdinalIgnoreCase) ||
                    trimmedTitle.IndexOf(@"C:\" + folder, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    trimmedTitle.IndexOf(@"\" + folder, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}