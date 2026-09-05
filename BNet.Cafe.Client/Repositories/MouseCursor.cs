using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BNet.Cafe.Client.Repositories
{
    public class MouseCursor
    {
        // ── Win32 ──────────────────────────────────────────────────────────────

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

        // DrawIconEx: cx=0,cy=0 uses cursor's natural size; DI_NORMAL = mask+image.
        [DllImport("user32.dll")]
        private static extern bool DrawIconEx(
            IntPtr hdc, int x, int y, IntPtr hIcon,
            int cx, int cy, uint istepIfAniCur,
            IntPtr hbrFlickerFreeDraw, uint diFlags);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        private const int CURSOR_SHOWING = 0x00000001;
        private const uint DI_NORMAL = 0x0003;   // DI_MASK | DI_IMAGE

        // ── Public API ─────────────────────────────────────────────────────────

        public void SetCursor(Graphics graphics, Rectangle screenBounds, int screenIndex)
        {
            try
            {
                // ── 1. Cursor handle + screen position ─────────────────────────

                IntPtr hCursor;
                int screenX, screenY;

                CURSORINFO pci;
                pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));

                if (GetCursorInfo(out pci)
                    && pci.flags == CURSOR_SHOWING
                    && pci.hCursor != IntPtr.Zero)
                {
                    // Normal path: console / interactive session.
                    hCursor = pci.hCursor;
                    screenX = pci.ptScreenPos.x;
                    screenY = pci.ptScreenPos.y;
                }
                else
                {

                    // Cursor.Current and Cursor.Position work in all session types.
                    Cursor cur = Cursor.Current;
                    if (cur == null) return;
                    hCursor = cur.Handle;
                    System.Drawing.Point pos = Cursor.Position;
                    screenX = pos.X;
                    screenY = pos.Y;
                }

                // ── 2. Screen physical origin ──────────────────────────────────

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
                // If AllScreens empty / index invalid → physOrigin stays (0,0),
                // which is correct for the primary monitor (the only one captured).

                int bitmapX = screenX - physOriginX;
                int bitmapY = screenY - physOriginY;

                int hotspotX = 0, hotspotY = 0;
                ICONINFO iconInfo;
                if (GetIconInfo(hCursor, out iconInfo))
                {
                    hotspotX = iconInfo.xHotspot;
                    hotspotY = iconInfo.yHotspot;
                    // GetIconInfo allocates two GDI bitmaps — free them immediately.
                    if (iconInfo.hbmMask != IntPtr.Zero) DeleteObject(iconInfo.hbmMask);
                    if (iconInfo.hbmColor != IntPtr.Zero) DeleteObject(iconInfo.hbmColor);
                }

                // ── 4. Draw ────────────────────────────────────────────────────

                IntPtr hdc = graphics.GetHdc();
                try
                {
                    DrawIconEx(hdc,
                        bitmapX - hotspotX,
                        bitmapY - hotspotY,
                        hCursor,
                        0, 0,           // cx=0,cy=0 → natural cursor size
                        0,
                        IntPtr.Zero,
                        DI_NORMAL);     // render mask + colour = correct transparency
                }
                finally
                {
                    graphics.ReleaseHdc(hdc);
                }
            }
            catch
            {

            }
        }
    }
}