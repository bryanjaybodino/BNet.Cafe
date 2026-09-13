using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BNet.Cafe.Client.Repositories
{
    public class MouseCursor
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct CURSORINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hCursor;
            public POINT ptScreenPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int x; public int y; }

        [StructLayout(LayoutKind.Sequential)]
        private struct ICONINFO
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll")]
        private static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO info);

        [DllImport("user32.dll")]
        private static extern bool DrawIconEx(
            IntPtr hdc, int x, int y, IntPtr hIcon,
            int cx, int cy, uint istepIfAniCur,
            IntPtr hbrFlickerFreeDraw, uint diFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        private const int CURSOR_SHOWING = 0x00000001;
        private const uint DI_NORMAL = 0x0003;

        public void SetCursor(Graphics graphics, Rectangle screenBounds, int screenIndex)
        {
            IntPtr hCursor = IntPtr.Zero;
            uint foregroundThreadId = 0;
            uint currentThreadId = GetCurrentThreadId();
            bool attached = false;

            try
            {
                // Attach capture thread to the active window thread to capture text/I-beam cursors
                IntPtr hwnd = GetForegroundWindow();
                if (hwnd != IntPtr.Zero)
                {
                    foregroundThreadId = GetWindowThreadProcessId(hwnd, out _);
                    if (foregroundThreadId != 0 && foregroundThreadId != currentThreadId)
                    {
                        attached = AttachThreadInput(currentThreadId, foregroundThreadId, true);
                    }
                }

                CURSORINFO pci;
                pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));

                int screenX = Cursor.Position.X;
                int screenY = Cursor.Position.Y;

                if (GetCursorInfo(out pci) && pci.flags == CURSOR_SHOWING && pci.hCursor != IntPtr.Zero)
                {
                    hCursor = pci.hCursor;
                    screenX = pci.ptScreenPos.x;
                    screenY = pci.ptScreenPos.y;
                }

                if (hCursor == IntPtr.Zero) return;

                // Screen positioning setup
                int physOriginX = 0;
                int physOriginY = 0;
                Screen[] allScreens = Screen.AllScreens;
                if (allScreens != null && screenIndex >= 0 && screenIndex < allScreens.Length)
                {
                    Screen screen = allScreens[screenIndex];
                    if (screen.Bounds.Width > 0 && screen.Bounds.Height > 0)
                    {
                        try
                        {
                            decimal sf = ScreenScale.scale(screen);
                            physOriginX = (int)(screen.Bounds.X * sf / 100m);
                            physOriginY = (int)(screen.Bounds.Y * sf / 100m);
                        }
                        catch
                        {
                            physOriginX = screen.Bounds.X;
                            physOriginY = screen.Bounds.Y;
                        }
                    }
                }

                int bitmapX = screenX - physOriginX;
                int bitmapY = screenY - physOriginY;

                int hotspotX = 0, hotspotY = 0;
                ICONINFO iconInfo;
                if (GetIconInfo(hCursor, out iconInfo))
                {
                    hotspotX = iconInfo.xHotspot;
                    hotspotY = iconInfo.yHotspot;
                    if (iconInfo.hbmMask != IntPtr.Zero) DeleteObject(iconInfo.hbmMask);
                    if (iconInfo.hbmColor != IntPtr.Zero) DeleteObject(iconInfo.hbmColor);
                }

                IntPtr hdc = graphics.GetHdc();
                try
                {
                    DrawIconEx(hdc, bitmapX - hotspotX, bitmapY - hotspotY, hCursor, 0, 0, 0, IntPtr.Zero, DI_NORMAL);
                }
                finally
                {
                    graphics.ReleaseHdc(hdc);
                }
            }
            catch { }
            finally
            {
                if (attached)
                {
                    AttachThreadInput(currentThreadId, foregroundThreadId, false);
                }
            }
        }
    }
}