using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNet.Cafe.Client.Models
{
    /// <summary>
    /// Represents a single input event sent from the web browser remote control.
    /// JSON shape mirrors the JavaScript sendInput() payload in index.html.
    /// </summary>
    public class RemoteInput
    {
        /// <summary>
        /// Event type: mousemove | mousedown | mouseup | wheel | keydown | keyup
        /// </summary>
        public string Type { get; set; }

        // ── Mouse ──────────────────────────────────────────────────────────────
        /// <summary>X coordinate in the remote screen's pixel space.</summary>
        public int X { get; set; }

        /// <summary>Y coordinate in the remote screen's pixel space.</summary>
        public int Y { get; set; }

        /// <summary>
        /// Mouse button index: 0 = left, 1 = middle, 2 = right.
        /// </summary>
        public int Button { get; set; }

        /// <summary>
        /// Wheel delta in pixels (positive = scroll down, negative = scroll up).
        /// </summary>
        public int Delta { get; set; }

        // ── Keyboard ───────────────────────────────────────────────────────────
        /// <summary>Browser KeyboardEvent.key value, e.g. "a", "Enter", "ArrowLeft".</summary>
        public string Key { get; set; }

        /// <summary>Browser KeyboardEvent.code value, e.g. "KeyA", "Enter".</summary>
        public string Code { get; set; }

        /// <summary>Browser KeyboardEvent.keyCode (legacy, used as fallback).</summary>
        public int KeyCode { get; set; }

        public bool Ctrl { get; set; }
        public bool Alt { get; set; }
        public bool Shift { get; set; }
        public bool Meta { get; set; }
        public int ScreenIndex { get; set; }

        /// <summary>
        /// Width of the JPEG frame the browser received (may differ from physical
        /// screen width due to server-side image compression scaling).
        /// Used by RemoteController to map browser coords → physical screen pixels.
        /// 0 means unknown — RemoteController falls back to treating coords as physical.
        /// </summary>
        public int FrameW { get; set; }

        /// <summary>Height of the JPEG frame (see FrameW).</summary>
        public int FrameH { get; set; }
    }
}