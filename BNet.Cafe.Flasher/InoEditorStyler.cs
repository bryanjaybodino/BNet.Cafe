using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BNet.Cafe.Flasher
{
    /// <summary>
    /// Arduino (.ino) code editor built only on the standard RichTextBox:
    /// syntax colouring, line numbers, auto-indent and its own undo/redo.
    /// No third-party libraries.
    /// </summary>
    [DesignerCategory("Code")]
    public class InoEditor : UserControl
    {
        // ------------------------------------------------------------ Native helpers

        private const int WM_SETREDRAW = 0x000B;
        private const int EM_GETEVENTMASK = 0x043B;
        private const int EM_SETEVENTMASK = 0x0445;
        private const int EM_GETSCROLLPOS = 0x04DD;
        private const int EM_SETSCROLLPOS = 0x04DE;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref Point lParam);

        // ------------------------------------------------------------ Colours / keywords

        private static readonly Color BackgroundColor = Color.White;
        private static readonly Color DefaultColor = Color.FromArgb(30, 30, 30);
        private static readonly Color CommentColor = Color.FromArgb(106, 153, 85);
        private static readonly Color KeywordColor = Color.FromArgb(0, 0, 200);
        private static readonly Color TypeColor = Color.FromArgb(43, 145, 175);
        private static readonly Color ArduinoColor = Color.FromArgb(204, 102, 0);
        private static readonly Color StringColor = Color.FromArgb(163, 21, 21);
        private static readonly Color NumberColor = Color.FromArgb(9, 134, 88);
        private static readonly Color PreprocessorColor = Color.FromArgb(128, 0, 128);

        private static readonly HashSet<string> Keywords = new HashSet<string>(
            ("if else for while do switch case break continue return default goto const static volatile " +
             "extern register inline struct class union enum typedef sizeof new delete public private " +
             "protected template typename namespace using this operator virtual try catch throw " +
             "true false NULL nullptr constexpr").Split(' '));

        private static readonly HashSet<string> Types = new HashSet<string>(
            ("void int long short char float double bool boolean byte word unsigned signed auto String " +
             "uint8_t uint16_t uint32_t uint64_t int8_t int16_t int32_t int64_t size_t").Split(' '));

        private static readonly HashSet<string> ArduinoApi = new HashSet<string>(
            ("setup loop pinMode digitalWrite digitalRead analogRead analogWrite analogReference " +
             "analogReadResolution analogWriteResolution delay delayMicroseconds millis micros tone noTone " +
             "pulseIn pulseInLong shiftOut shiftIn attachInterrupt detachInterrupt digitalPinToInterrupt " +
             "interrupts noInterrupts map constrain min max abs pow sqrt sq sin cos tan random randomSeed " +
             "bitRead bitWrite bitSet bitClear bit lowByte highByte isDigit isAlpha isSpace " +
             "Serial Serial1 Serial2 Serial3 Wire SPI Servo EEPROM " +
             "begin end print println printf available read write flush peek parseInt parseFloat " +
             "HIGH LOW INPUT OUTPUT INPUT_PULLUP LED_BUILTIN PI DEC HEX OCT BIN " +
             "A0 A1 A2 A3 A4 A5 A6 A7 SDA SCL MOSI MISO SCK SS F PROGMEM").Split(' '));

        // ------------------------------------------------------------ Fields

        private struct Span
        {
            public int Start;
            public int Length;
            public Color Color;
            public Span(int start, int length, Color color) { Start = start; Length = length; Color = color; }
        }

        private sealed class Snapshot
        {
            public readonly string Text;
            public readonly int Caret;
            public Snapshot(string text, int caret) { Text = text; Caret = caret; }
        }

        private readonly RichTextBox _rtb = new RichTextBox();
        private readonly Gutter _gutter;
        private readonly Timer _timer = new Timer { Interval = 150 };

        private readonly List<Snapshot> _undo = new List<Snapshot>();
        private readonly List<Snapshot> _redo = new List<Snapshot>();
        private Snapshot _last = new Snapshot("", 0);

        private bool _highlighting;
        private bool _restoring;
        private bool _pendingHighlight;

        // ------------------------------------------------------------ Construction

        public InoEditor()
        {
            BorderStyle = BorderStyle.FixedSingle;

            var font = new Font("Consolas", 11F);

            _rtb.Dock = DockStyle.Fill;
            _rtb.BorderStyle = BorderStyle.None;
            _rtb.Font = font;
            _rtb.BackColor = BackgroundColor;
            _rtb.ForeColor = DefaultColor;
            _rtb.WordWrap = false;
            _rtb.AcceptsTab = true;
            _rtb.DetectUrls = false;
            _rtb.HideSelection = false;
            _rtb.EnableAutoDragDrop = false;
            _rtb.ScrollBars = RichTextBoxScrollBars.ForcedBoth;

            _gutter = new Gutter(_rtb, font) { Dock = DockStyle.Left };

            // The last control added is docked first, so the gutter takes the left edge
            Controls.Add(_rtb);
            Controls.Add(_gutter);

            _rtb.TextChanged += Rtb_TextChanged;
            _rtb.KeyDown += Rtb_KeyDown;
            _rtb.KeyPress += Rtb_KeyPress;
            _rtb.VScroll += delegate { _gutter.Invalidate(); };
            _rtb.SelectionChanged += delegate { if (!_highlighting) _gutter.Invalidate(); };
            _rtb.Resize += delegate { _gutter.Invalidate(); };
            _rtb.HandleCreated += delegate
            {
                if (_pendingHighlight) _rtb.BeginInvoke(new Action(Highlight));
            };

            _timer.Tick += delegate
            {
                _timer.Stop();
                CommitSnapshot();
                Highlight();
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _timer.Dispose();
            base.Dispose(disposing);
        }

        // ------------------------------------------------------------ Public API

        /// <summary>Editor content with Windows (CRLF) line endings.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get { return _rtb.Text.Replace("\r\n", "\n").Replace("\n", "\r\n"); }
            set { LoadText(value); }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ReadOnly
        {
            get { return _rtb.ReadOnly; }
            set
            {
                _rtb.ReadOnly = value;
                _rtb.BackColor = BackgroundColor; // ReadOnly would otherwise turn it grey
            }
        }

        /// <summary>Replaces the content, clears undo history and highlights it.</summary>
        public void LoadText(string text)
        {
            _timer.Stop();
            _restoring = true;
            _rtb.Text = (text ?? "").Replace("\r\n", "\n");
            _rtb.Select(0, 0);
            _restoring = false;

            _undo.Clear();
            _redo.Clear();
            _last = new Snapshot(_rtb.Text, 0);

            Highlight();
            _gutter.UpdateWidth();
            _gutter.Invalidate();
            OnTextChanged(EventArgs.Empty);
        }

        public void UndoEdit()
        {
            if (_rtb.ReadOnly) return;
            CommitSnapshot();
            if (_undo.Count == 0) return;

            _redo.Add(_last);
            _last = _undo[_undo.Count - 1];
            _undo.RemoveAt(_undo.Count - 1);
            Restore(_last);
        }

        public void RedoEdit()
        {
            if (_rtb.ReadOnly) return;
            CommitSnapshot();
            if (_redo.Count == 0) return;

            _undo.Add(_last);
            _last = _redo[_redo.Count - 1];
            _redo.RemoveAt(_redo.Count - 1);
            Restore(_last);
        }

        // ------------------------------------------------------------ Undo / redo

        private void CommitSnapshot()
        {
            var current = _rtb.Text;
            if (current == _last.Text) return;

            _undo.Add(_last);
            if (_undo.Count > 200) _undo.RemoveAt(0);
            _redo.Clear();
            _last = new Snapshot(current, _rtb.SelectionStart);
        }

        private void Restore(Snapshot snap)
        {
            _restoring = true;
            _rtb.Text = snap.Text;
            _rtb.Select(Math.Min(snap.Caret, _rtb.TextLength), 0);
            _restoring = false;

            Highlight();
            _gutter.UpdateWidth();
            _gutter.Invalidate();
            OnTextChanged(EventArgs.Empty);
        }

        // ------------------------------------------------------------ Editing behaviour

        private void Rtb_TextChanged(object sender, EventArgs e)
        {
            if (_highlighting || _restoring) return;

            _timer.Stop();
            _timer.Start();
            _gutter.UpdateWidth();
            _gutter.Invalidate();
            OnTextChanged(EventArgs.Empty);
        }

        private void Rtb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && !e.Alt && e.KeyCode == Keys.Z && !e.Shift)
            {
                UndoEdit();
                e.Handled = e.SuppressKeyPress = true;
            }
            else if (e.Control && !e.Alt && (e.KeyCode == Keys.Y || (e.KeyCode == Keys.Z && e.Shift)))
            {
                RedoEdit();
                e.Handled = e.SuppressKeyPress = true;
            }
            else if ((e.Control && e.KeyCode == Keys.V) || (e.Shift && e.KeyCode == Keys.Insert))
            {
                // Always paste plain text (no foreign RTF formatting)
                if (!_rtb.ReadOnly && Clipboard.ContainsText())
                    _rtb.SelectedText = Clipboard.GetText().Replace("\r\n", "\n");
                e.Handled = e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Tab && !e.Control && !e.Alt)
            {
                if (!_rtb.ReadOnly) _rtb.SelectedText = "  ";
                e.Handled = e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Enter && !e.Control && !e.Alt && !_rtb.ReadOnly)
            {
                // Auto-indent: keep indentation of the current line, add a level after '{'
                int pos = _rtb.SelectionStart;
                int line = _rtb.GetLineFromCharIndex(pos);
                int lineStart = _rtb.GetFirstCharIndexFromLine(line);
                string before = _rtb.Text.Substring(lineStart, pos - lineStart);

                int ws = 0;
                while (ws < before.Length && (before[ws] == ' ' || before[ws] == '\t')) ws++;
                string indent = before.Substring(0, ws);
                if (before.TrimEnd().EndsWith("{")) indent += "  ";

                _rtb.SelectedText = "\n" + indent;
                e.Handled = e.SuppressKeyPress = true;
            }
        }

        private void Rtb_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Outdent a lone '}' by one level
            if (e.KeyChar == '}' && !_rtb.ReadOnly)
            {
                int pos = _rtb.SelectionStart;
                int line = _rtb.GetLineFromCharIndex(pos);
                int lineStart = _rtb.GetFirstCharIndexFromLine(line);
                string before = _rtb.Text.Substring(lineStart, pos - lineStart);

                if (before.Length >= 2 && before.Trim().Length == 0 && before.EndsWith("  "))
                {
                    _rtb.Select(pos - 2, 2);
                    _rtb.SelectedText = "";
                }
            }
        }

        // ------------------------------------------------------------ Syntax highlighting

        private void Highlight()
        {
            if (!_rtb.IsHandleCreated)
            {
                _pendingHighlight = true;
                return;
            }
            _pendingHighlight = false;
            _highlighting = true;

            IntPtr h = _rtb.Handle;
            int selStart = _rtb.SelectionStart;
            int selLen = _rtb.SelectionLength;
            Point scroll = new Point();
            SendMessage(h, EM_GETSCROLLPOS, 0, ref scroll);
            IntPtr mask = SendMessage(h, EM_GETEVENTMASK, IntPtr.Zero, IntPtr.Zero);

            SendMessage(h, EM_SETEVENTMASK, IntPtr.Zero, IntPtr.Zero); // no change notifications
            SendMessage(h, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);   // no repainting
            try
            {
                var spans = Tokenize(_rtb.Text);

                _rtb.SelectAll();
                _rtb.SelectionColor = DefaultColor;
                foreach (var s in spans)
                {
                    _rtb.Select(s.Start, s.Length);
                    _rtb.SelectionColor = s.Color;
                }
            }
            finally
            {
                _rtb.Select(selStart, selLen);
                SendMessage(h, EM_SETSCROLLPOS, 0, ref scroll);
                SendMessage(h, EM_SETEVENTMASK, IntPtr.Zero, mask);
                SendMessage(h, WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
                _rtb.Invalidate();
                _highlighting = false;
                _gutter.Invalidate();
            }
        }

        private static List<Span> Tokenize(string t)
        {
            var spans = new List<Span>();
            int n = t.Length;
            int i = 0;

            while (i < n)
            {
                char c = t[i];
                char next = i + 1 < n ? t[i + 1] : '\0';

                if (c == '/' && next == '/')
                {
                    int end = t.IndexOf('\n', i);
                    if (end < 0) end = n;
                    spans.Add(new Span(i, end - i, CommentColor));
                    i = end;
                }
                else if (c == '/' && next == '*')
                {
                    int end = t.IndexOf("*/", i + 2, StringComparison.Ordinal);
                    end = end < 0 ? n : end + 2;
                    spans.Add(new Span(i, end - i, CommentColor));
                    i = end;
                }
                else if (c == '#' && IsLineStart(t, i))
                {
                    int eol = t.IndexOf('\n', i);
                    if (eol < 0) eol = n;
                    int cm = t.IndexOf("//", i, eol - i, StringComparison.Ordinal);
                    int end = cm >= 0 ? cm : eol;
                    spans.Add(new Span(i, end - i, PreprocessorColor));
                    i = end;
                }
                else if (c == '"' || c == '\'')
                {
                    int j = i + 1;
                    while (j < n && t[j] != c && t[j] != '\n')
                    {
                        if (t[j] == '\\') j++;
                        j++;
                    }
                    if (j < n && t[j] == c) j++;
                    if (j > n) j = n;
                    spans.Add(new Span(i, j - i, StringColor));
                    i = j;
                }
                else if (char.IsLetter(c) || c == '_')
                {
                    int j = i + 1;
                    while (j < n && (char.IsLetterOrDigit(t[j]) || t[j] == '_')) j++;

                    string word = t.Substring(i, j - i);
                    if (Keywords.Contains(word)) spans.Add(new Span(i, j - i, KeywordColor));
                    else if (Types.Contains(word)) spans.Add(new Span(i, j - i, TypeColor));
                    else if (ArduinoApi.Contains(word)) spans.Add(new Span(i, j - i, ArduinoColor));
                    i = j;
                }
                else if (char.IsDigit(c))
                {
                    int j = i + 1;
                    while (j < n && (char.IsLetterOrDigit(t[j]) || t[j] == '.')) j++;
                    spans.Add(new Span(i, j - i, NumberColor));
                    i = j;
                }
                else
                {
                    i++;
                }
            }
            return spans;
        }

        private static bool IsLineStart(string t, int index)
        {
            for (int k = index - 1; k >= 0; k--)
            {
                if (t[k] == '\n') return true;
                if (t[k] != ' ' && t[k] != '\t') return false;
            }
            return true;
        }

        // ------------------------------------------------------------ Line-number gutter

        private sealed class Gutter : Control
        {
            private readonly RichTextBox _rtb;
            private readonly Font _font;

            public Gutter(RichTextBox rtb, Font font)
            {
                _rtb = rtb;
                _font = font;
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                Width = 48;
            }

            public void UpdateWidth()
            {
                int lines = _rtb.GetLineFromCharIndex(_rtb.TextLength) + 1;
                int digits = Math.Max(3, lines.ToString().Length);
                int w = TextRenderer.MeasureText(new string('9', digits), _font,
                            new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding).Width + 14;
                if (Width != w) Width = w;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.Clear(Color.FromArgb(240, 240, 240));

                int total = _rtb.GetLineFromCharIndex(_rtb.TextLength) + 1;
                int first = _rtb.GetLineFromCharIndex(_rtb.GetCharIndexFromPosition(new Point(0, 0)));
                int current = _rtb.GetLineFromCharIndex(_rtb.SelectionStart);
                int rowHeight = _font.Height + 2;

                for (int line = first; line < total; line++)
                {
                    int idx = _rtb.GetFirstCharIndexFromLine(line);
                    if (idx < 0) break;

                    Point p = _rtb.GetPositionFromCharIndex(idx);
                    if (p.Y > Height) break;

                    TextRenderer.DrawText(g, (line + 1).ToString(), _font,
                        new Rectangle(0, p.Y, Width - 6, rowHeight),
                        line == current ? Color.Black : Color.FromArgb(120, 120, 120),
                        TextFormatFlags.Right | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                }

                using (var pen = new Pen(Color.FromArgb(210, 210, 210)))
                    g.DrawLine(pen, Width - 1, 0, Width - 1, Height);
            }
        }
    }
}