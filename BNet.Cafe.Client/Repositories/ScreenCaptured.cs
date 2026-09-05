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
    /// <summary>
    /// Captures the physical desktop using BitBlt on the INPUT desktop.
    ///
    /// CHANGES:
    ///  • TakeScreenshot now accepts a screenIndex parameter (0 = primary, 1+ = secondary).
    ///  • CaptureInputDesktop overload captures only the bounds of the requested screen.
    ///  • Bitmap is always disposed by the caller (HostCaptureLoop) after JPEG encoding —
    ///    never held here longer than needed.
    /// </summary>
    public class ScreenCaptured
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr OpenInputDesktop(
            uint dwFlags, bool fInherit, uint dwDesiredAccess);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr OpenDesktop(
            string lpszDesktop, uint dwFlags, bool fInherit, uint dwDesiredAccess);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetThreadDesktop(IntPtr hDesktop);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool CloseDesktop(IntPtr hDesktop);

        private const uint DESKTOP_ALL =
            0x0001 | 0x0002 | 0x0004 | 0x0008 | 0x0010 |
            0x0020 | 0x0040 | 0x0080 | 0x0100;

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int w, int h);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObj);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObj);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(
            IntPtr hdcDst, int xDst, int yDst, int w, int h,
            IntPtr hdcSrc, int xSrc, int ySrc, uint rop);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int idx);

        private const int SM_CXVIRTUALSCREEN = 78;
        private const int SM_CYVIRTUALSCREEN = 79;
        private const int SM_XVIRTUALSCREEN = 76;
        private const int SM_YVIRTUALSCREEN = 77;
        private const uint SRCCOPY = 0x00CC0020;

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the count of currently attached screens.
        /// Safe to call from any thread.
        /// </summary>
        public static int GetScreenCount() => Screen.AllScreens.Length;

        /// <summary>
        /// Returns a list of screen labels for UI display ("Screen 1 (Primary)", etc.).
        /// </summary>
        public static List<string> GetScreenLabels()
        {
            var labels = new List<string>();
            var screens = Screen.AllScreens;
            for (int i = 0; i < screens.Length; i++)
            {
                string label = $"Screen {i + 1}";
                if (screens[i].Primary) label += " (Primary)";
                labels.Add(label);
            }
            return labels;
        }

        /// <summary>
        /// Captures <paramref name="screenIndex"/> and returns a single-element list.
        /// Returns an empty list on failure.  The caller MUST dispose the bitmap.
        /// </summary>
        public static async Task<List<Bitmap>> TakeScreenshot(bool isMouseVisible, int screenIndex = 0)
        {
            var result = new List<Bitmap>();

            // Clamp to valid range.
            var screens = Screen.AllScreens;
            if (screenIndex < 0 || screenIndex >= screens.Length)
                screenIndex = 0;

            Screen screen = screens[screenIndex];
            Rectangle bounds = GetPhysicalBounds(screen);

            Bitmap bmp = await Task.Run(() => CaptureScreen(bounds)).ConfigureAwait(false);

            if (bmp != null)
            {
                if (isMouseVisible)
                {
                    using (var g = Graphics.FromImage(bmp))
                        new MouseCursor().SetCursor(g, screen.Bounds, screenIndex);
                }
                result.Add(bmp);
            }

            return result;
        }

        // ── Physical bounds (DPI-aware) ────────────────────────────────────────

        /// <summary>
        /// Returns the physical pixel bounds of the screen by comparing
        /// the logical Bounds.Width against the actual DEVMODE resolution.
        /// Falls back to logical bounds if the query fails.
        /// </summary>
        private static Rectangle GetPhysicalBounds(Screen screen)
        {
            try
            {
                decimal sf = ScreenScale.scale(screen);
                if (sf <= 0) sf = 100m;

                int physX = (int)(screen.Bounds.X * sf / 100m);
                int physY = (int)(screen.Bounds.Y * sf / 100m);
                int physW = (int)(screen.Bounds.Width * sf / 100m);
                int physH = (int)(screen.Bounds.Height * sf / 100m);

                return new Rectangle(physX, physY, physW, physH);
            }
            catch
            {
                return screen.Bounds;
            }
        }

        // ── Core BitBlt capture ────────────────────────────────────────────────

        private static Bitmap CaptureScreen(Rectangle bounds)
        {
            IntPtr hDesktop = OpenInputDesktop(0, false, DESKTOP_ALL);
            if (hDesktop == IntPtr.Zero)
            {
                hDesktop = OpenDesktop("Default", 0, false, DESKTOP_ALL);
                if (hDesktop == IntPtr.Zero)
                    return null;
            }

            SetThreadDesktop(hDesktop);

            IntPtr screenDC = IntPtr.Zero;
            IntPtr memDC = IntPtr.Zero;
            IntPtr hBmp = IntPtr.Zero;
            IntPtr hOld = IntPtr.Zero;

            try
            {
                screenDC = GetDC(IntPtr.Zero);
                if (screenDC == IntPtr.Zero) return null;

                int w = bounds.Width;
                int h = bounds.Height;
                if (w <= 0 || h <= 0) return null;

                memDC = CreateCompatibleDC(screenDC);
                hBmp = CreateCompatibleBitmap(screenDC, w, h);
                hOld = SelectObject(memDC, hBmp);

                // BitBlt from the physical origin of this screen.
                if (!BitBlt(memDC, 0, 0, w, h, screenDC, bounds.X, bounds.Y, SRCCOPY))
                    return null;

                return Image.FromHbitmap(hBmp);   // copies GDI bits into a managed Bitmap
            }
            catch { return null; }
            finally
            {
                if (hOld != IntPtr.Zero) SelectObject(memDC, hOld);
                if (hBmp != IntPtr.Zero) DeleteObject(hBmp);
                if (memDC != IntPtr.Zero) DeleteDC(memDC);
                if (screenDC != IntPtr.Zero) ReleaseDC(IntPtr.Zero, screenDC);
                CloseDesktop(hDesktop);
            }
        }
    }
}