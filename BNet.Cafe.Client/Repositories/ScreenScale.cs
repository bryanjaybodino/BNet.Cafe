using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BNet.Cafe.Client.Repositories
{
    public static class ScreenScale
    {
        [DllImport("Shcore.dll")]
        private static extern int SetProcessDpiAwareness(int awareness);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(
            string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DEVMODE
        {
            private const int CCHDEVICENAME = 0x20;
            private const int CCHFORMNAME = 0x20;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public ScreenOrientation dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        /// <summary>
        /// Call this ONCE from Program.Main() before Application.Run().
        /// Marks the process as Per-Monitor DPI aware so that GetSystemMetrics
        /// and all GDI calls return physical pixel values.
        /// </summary>
        public static void InitDpiAwareness()
        {
            SetProcessDpiAwareness(2); // PROCESS_PER_MONITOR_DPI_AWARE
        }

        /// <summary>
        /// Returns the DPI scale factor for <paramref name="screen"/> as a percentage.
        /// e.g. 150 means 150% (1.5× physical-to-logical).
        /// Returns 100 (no scaling) if the query fails.
        /// </summary>
        public static decimal scale(Screen screen)
        {
            try
            {
                var dm = new DEVMODE();
                dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
                if (!EnumDisplaySettings(screen.DeviceName, -1, ref dm))
                    return 100m;

                if (screen.Bounds.Width == 0) return 100m;

                decimal factor = Math.Round(
                    decimal.Divide(dm.dmPelsWidth, screen.Bounds.Width), 2);

                return factor * 100m;
            }
            catch { return 100m; }
        }
    }
}