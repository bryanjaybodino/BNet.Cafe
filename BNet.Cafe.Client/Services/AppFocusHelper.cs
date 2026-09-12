using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BNet.Cafe.Client.Services
{
    internal class AppFocusHelper
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const int GWL_STYLE = -16;
        private const int WS_BORDER = 0x00800000;
        private const int WS_CAPTION = 0x00C00000;
        private const int WS_THICKFRAME = 0x00040000;

        private const int SW_RESTORE = 9;
        private const uint SWP_FRAMECHANGED = 0x0020;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private static readonly IntPtr HWND_TOP = IntPtr.Zero;

        /// <summary>
        /// Focuses a target app, removes its borders/title bar, and resizes it to full screen.
        /// </summary>
        public static bool FocusAndFullScreenApp(string processName)
        {
            Process process = Process.GetProcessesByName(processName).FirstOrDefault();

            if (process != null)
            {
                IntPtr handle = process.MainWindowHandle;

                if (handle != IntPtr.Zero)
                {
                    // 1. Restore window if minimized
                    ShowWindow(handle, SW_RESTORE);

                    // 2. Remove borders and title bar
                    int style = GetWindowLong(handle, GWL_STYLE);
                    style &= ~(WS_CAPTION | WS_THICKFRAME | WS_BORDER);
                    SetWindowLong(handle, GWL_STYLE, style);

                    // 3. Stretch window to cover the screen
                    int screenWidth = Screen.PrimaryScreen.Bounds.Width;
                    int screenHeight = Screen.PrimaryScreen.Bounds.Height;

                    SetWindowPos(
                        handle,
                        HWND_TOP,
                        0, 0, screenWidth, screenHeight,
                        SWP_FRAMECHANGED | SWP_SHOWWINDOW
                    );

                    // 4. Bring window to the foreground and focus
                    return SetForegroundWindow(handle);
                }
            }

            return false;
        }
    }
}