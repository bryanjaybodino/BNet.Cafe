using BNet.Cafe.Client.Models;
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
    /// Translates RemoteInput events (received from the browser) into real Win32 SendInput calls.
    ///
    /// FIX: Mouse coordinates from the browser are always relative to the top-left of the
    /// captured screen (0,0 = that screen's origin).  MOUSEEVENTF_ABSOLUTE works in the
    /// virtual-desktop coordinate space (all monitors combined), so we must first offset
    /// the per-screen coords by the screen's physical origin on the virtual desktop before
    /// converting to the 0-65535 absolute range.
    /// </summary>
    public static class RemoteController
    {
        // ── Win32 structs ──────────────────────────────────────────────────────

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct INPUT_UNION
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
            [FieldOffset(0)] public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type; // 0 = mouse, 1 = keyboard
            public INPUT_UNION u;
        }

        // Mouse dwFlags
        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        private const uint MOUSEEVENTF_VIRTUALDESK = 0x4000; // FIX: required for multi-monitor absolute positioning

        // Keyboard dwFlags
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;

        // Virtual-screen metrics indices
        private const int SM_XVIRTUALSCREEN = 76;
        private const int SM_YVIRTUALSCREEN = 77;
        private const int SM_CXVIRTUALSCREEN = 78;
        private const int SM_CYVIRTUALSCREEN = 79;

        private const int ABSOLUTE_MAX = 65535;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Dispatches the appropriate Win32 input event(s) for a given RemoteInput.
        /// </summary>
        public static void Dispatch(RemoteInput input)
        {
            if (input == null) return;

            switch (input.Type?.ToLower())
            {
                case "mousemove": MouseMove(input.X, input.Y, input.ScreenIndex, input.FrameW, input.FrameH); break;
                case "mousedown": MouseButton(input.X, input.Y, input.Button, down: true, input.ScreenIndex, input.FrameW, input.FrameH); break;
                case "mouseup": MouseButton(input.X, input.Y, input.Button, down: false, input.ScreenIndex, input.FrameW, input.FrameH); break;
                case "wheel": MouseWheel(input.Delta); break;
                case "keydown": Key(input, down: true); break;
                case "keyup": Key(input, down: false); break;
            }
        }

        // ── Virtual-desktop helpers ────────────────────────────────────────────

        /// <summary>
        /// Returns the physical pixel bounds of a screen on the virtual desktop.
        /// Applies the DPI scale so that coordinates match what BitBlt captures.
        /// </summary>
        private static Rectangle GetPhysicalScreenBounds(int screenIndex)
        {
            Screen[] screens = Screen.AllScreens;
            if (screenIndex < 0 || screenIndex >= screens.Length)
                screenIndex = 0;

            Screen screen = screens[screenIndex];
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

        /// <summary>
        /// Converts physical pixel coordinates that are relative to a specific screen's
        /// top-left corner into MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_VIRTUALDESK values (0-65535).
        ///
        /// MOUSEEVENTF_VIRTUALDESK maps 0 → left edge of virtual desktop,
        /// 65535 → right edge of virtual desktop (across ALL monitors).
        /// We therefore need the virtual desktop origin and dimensions.
        /// </summary>
        /// 
        private static readonly int VdLeft = GetSystemMetrics(SM_XVIRTUALSCREEN);
        private static readonly int VdTop = GetSystemMetrics(SM_YVIRTUALSCREEN);
        private static readonly int VdW = Math.Max(1, GetSystemMetrics(SM_CXVIRTUALSCREEN));
        private static readonly int VdH = Math.Max(1, GetSystemMetrics(SM_CYVIRTUALSCREEN));
        private static (int absX, int absY) ToVirtualAbsolute(int screenRelX, int screenRelY, int screenIndex, int frameW = 0, int frameH = 0)
        {
            Rectangle screenBounds = GetPhysicalScreenBounds(screenIndex);

            int physX = screenRelX;
            int physY = screenRelY;
            if (frameW > 0 && frameH > 0 && screenBounds.Width > 0 && screenBounds.Height > 0)
            {
                physX = (int)Math.Round((double)screenRelX * screenBounds.Width / frameW);
                physY = (int)Math.Round((double)screenRelY * screenBounds.Height / frameH);
            }

            int vdX = screenBounds.X + physX;
            int vdY = screenBounds.Y + physY;

            // Use cached metrics
            int absX = (int)(((double)(vdX - VdLeft) / VdW) * (ABSOLUTE_MAX + 1));
            int absY = (int)(((double)(vdY - VdTop) / VdH) * (ABSOLUTE_MAX + 1));

            return (Math.Max(0, Math.Min(ABSOLUTE_MAX, absX)), Math.Max(0, Math.Min(ABSOLUTE_MAX, absY)));
        }

        // ── Mouse helpers ──────────────────────────────────────────────────────

        private static (int lastX, int lastY) _lastPos = (-1, -1);

        private static void MouseMove(int x, int y, int screenIndex, int frameW = 0, int frameH = 0)
        {
            var (absX, absY) = ToVirtualAbsolute(x, y, screenIndex, frameW, frameH);

            // Skip redundant mouse moves if coordinates haven't changed
            if (_lastPos.lastX == absX && _lastPos.lastY == absY) return;
            _lastPos = (absX, absY);

            var inp = new INPUT
            {
                type = 0,
                u = new INPUT_UNION
                {
                    mi = new MOUSEINPUT
                    {
                        dx = absX,
                        dy = absY,
                        dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_VIRTUALDESK
                    }
                }
            };
            SendInput(1, new[] { inp }, Marshal.SizeOf(typeof(INPUT)));
        }

        private static void MouseButton(int x, int y, int button, bool down, int screenIndex, int frameW = 0, int frameH = 0)
        {
            // Move first so the click lands on the right spot
            MouseMove(x, y, screenIndex, frameW, frameH);

            var (absX, absY) = ToVirtualAbsolute(x, y, screenIndex, frameW, frameH);

            uint flags;
            switch (button)
            {
                case 2: flags = down ? MOUSEEVENTF_RIGHTDOWN : MOUSEEVENTF_RIGHTUP; break;
                case 1: flags = down ? MOUSEEVENTF_MIDDLEDOWN : MOUSEEVENTF_MIDDLEUP; break;
                default: flags = down ? MOUSEEVENTF_LEFTDOWN : MOUSEEVENTF_LEFTUP; break;
            }

            var inp = new INPUT
            {
                type = 0,
                u = new INPUT_UNION
                {
                    mi = new MOUSEINPUT
                    {
                        dx = absX,
                        dy = absY,
                        dwFlags = flags | MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_VIRTUALDESK
                    }
                }
            };
            SendInput(1, new[] { inp }, Marshal.SizeOf(typeof(INPUT)));
        }

        private static void MouseWheel(int delta)
        {
            // Browser deltaY: positive = scroll down. Win32 WHEEL: positive = scroll up → invert.
            int winDelta = -(int)(Math.Sign(delta) * 120 * Math.Max(1, Math.Abs(delta) / 100));

            var inp = new INPUT
            {
                type = 0,
                u = new INPUT_UNION
                {
                    mi = new MOUSEINPUT
                    {
                        mouseData = (uint)winDelta,
                        dwFlags = MOUSEEVENTF_WHEEL
                    }
                }
            };
            SendInput(1, new[] { inp }, Marshal.SizeOf(typeof(INPUT)));
        }

        // ── Keyboard helpers ───────────────────────────────────────────────────

        private static void Key(RemoteInput input, bool down)
        {
            ushort vk = MapKey(input.Key, input.KeyCode);
            if (vk == 0) return;

            bool extended = IsExtended(vk);
            var inputs = new List<INPUT>();

            // Send modifier down before the main key
            if (down)
            {
                if (input.Ctrl) inputs.Add(MakeKey(0x11, down: true));
                if (input.Alt) inputs.Add(MakeKey(0x12, down: true));
                if (input.Shift) inputs.Add(MakeKey(0x10, down: true));
            }

            inputs.Add(MakeKey(vk, down, extended));

            // Release modifiers after key up
            if (!down)
            {
                if (input.Shift) inputs.Add(MakeKey(0x10, down: false));
                if (input.Alt) inputs.Add(MakeKey(0x12, down: false));
                if (input.Ctrl) inputs.Add(MakeKey(0x11, down: false));
            }

            var arr = inputs.ToArray();
            SendInput((uint)arr.Length, arr, Marshal.SizeOf(typeof(INPUT)));
        }

        private static INPUT MakeKey(ushort vk, bool down, bool extended = false)
        {
            uint flags = 0;
            if (!down) flags |= KEYEVENTF_KEYUP;
            if (extended) flags |= KEYEVENTF_EXTENDEDKEY;

            return new INPUT
            {
                type = 1,
                u = new INPUT_UNION
                {
                    ki = new KEYBDINPUT { wVk = vk, dwFlags = flags }
                }
            };
        }

        private static bool IsExtended(ushort vk)
        {
            return vk == 0x21 || vk == 0x22 || vk == 0x23 || vk == 0x24  // PgUp PgDn End Home
                || vk == 0x25 || vk == 0x26 || vk == 0x27 || vk == 0x28  // Left Up Right Down
                || vk == 0x2D || vk == 0x2E                               // Insert Delete
                || vk == 0x11 || vk == 0x12;                              // Ctrl Alt (right-side)
        }

        private static ushort MapKey(string key, int keyCode)
        {
            if (string.IsNullOrEmpty(key)) return (ushort)keyCode;

            switch (key)
            {
                case "Backspace": return 0x08;
                case "Tab": return 0x09;
                case "Enter": return 0x0D;
                case "Shift": return 0x10;
                case "Control": return 0x11;
                case "Alt": return 0x12;
                case "Pause": return 0x13;
                case "CapsLock": return 0x14;
                case "Escape": return 0x1B;
                case " ": return 0x20;
                case "PageUp": return 0x21;
                case "PageDown": return 0x22;
                case "End": return 0x23;
                case "Home": return 0x24;
                case "ArrowLeft": return 0x25;
                case "ArrowUp": return 0x26;
                case "ArrowRight": return 0x27;
                case "ArrowDown": return 0x28;
                case "Insert": return 0x2D;
                case "Delete": return 0x2E;
                case "Meta": return 0x5B;
                case "ContextMenu": return 0x5D;
                case "PrintScreen": return 0x2C;
                case "ScrollLock": return 0x91;
                case "NumLock": return 0x90;
                case "F1": return 0x70;
                case "F2": return 0x71;
                case "F3": return 0x72;
                case "F4": return 0x73;
                case "F5": return 0x74;
                case "F6": return 0x75;
                case "F7": return 0x76;
                case "F8": return 0x77;
                case "F9": return 0x78;
                case "F10": return 0x79;
                case "F11": return 0x7A;
                case "F12": return 0x7B;
                default:
                    if (key.Length == 1)
                    {
                        char c = char.ToUpper(key[0]);
                        if (c >= 'A' && c <= 'Z') return (ushort)c;
                        if (c >= '0' && c <= '9') return (ushort)c;
                        short scan = VkKeyScan(key[0]);
                        if (scan != -1) return (ushort)(scan & 0xFF);
                    }
                    return 0;
            }
        }

        [DllImport("user32.dll")]
        private static extern short VkKeyScan(char ch);
    }
}