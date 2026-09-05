using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;

namespace BNet.Cafe.Client.Repositories
{
    public class UserActivity
    {
        // ══════════════════════════════════════════════════════════════
        //  Data model returned on every poll
        // ══════════════════════════════════════════════════════════════
        public class ActiveWindowInfo
        {
            public string AppName { get; set; } = "Unknown";
            public string ProcessName { get; set; } = "Unknown";
            public string WindowTitle { get; set; } = "";
            public string Url { get; set; }           // null when not a browser
            public bool IsBrowser => Url != null;
            public DateTime CapturedAt { get; set; }

            public override string ToString() =>
                IsBrowser
                    ? $"[{AppName}]  URL: {Url}  ({CapturedAt:HH:mm:ss})"
                    : $"[{AppName}]  {WindowTitle}  ({CapturedAt:HH:mm:ss})";
        }

        // ══════════════════════════════════════════════════════════════
        //  ActiveWindowMonitor  –  drop this class in any project
        //
        //  QUICK START (from any Form):
        //
        //    // 1. One-shot read
        //    var info = ActiveWindowMonitor.GetCurrent();
        //    MessageBox.Show(info.ToString());
        //
        //    // 2. Auto-polling with event
        //    ActiveWindowMonitor.OnChanged += (info) => label1.Text = info.AppName;
        //    ActiveWindowMonitor.StartPolling(intervalMs: 1000);
        //    ...
        //    ActiveWindowMonitor.StopPolling();
        //
        // ══════════════════════════════════════════════════════════════
        public static class ActiveWindowMonitor
        {
            // ── Win32 P/Invoke ─────────────────────────────────────────
            [DllImport("user32.dll")]
            private static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            private static extern int GetWindowText(IntPtr hWnd, StringBuilder sb, int count);

            [DllImport("user32.dll")]
            private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint pid);

            // ── Browser process names ──────────────────────────────────
            private static readonly string[] _browsers =
                { "chrome", "firefox", "msedge", "opera", "brave", "vivaldi", "iexplore" };

            // ── Polling internals ──────────────────────────────────────
            private static Timer _timer;
            private static ActiveWindowInfo _last;

            /// <summary>
            /// Raised every poll interval when the active window changes.
            /// Subscribe from any form: ActiveWindowMonitor.OnChanged += MyHandler;
            /// </summary>
            public static event Action<ActiveWindowInfo> OnChanged;

            /// <summary>
            /// Raised every poll interval regardless of whether the window changed.
            /// </summary>
            public static event Action<ActiveWindowInfo> OnPolled;

            // ── Public API ─────────────────────────────────────────────

            /// <summary>One-shot: returns info about the current foreground window.</summary>
            public static ActiveWindowInfo GetCurrent() => Capture();

            /// <summary>
            /// Starts background polling. Fires OnPolled every tick, OnChanged only when app/URL switches.
            /// Safe to call multiple times – re-entrant calls just update the interval.
            /// </summary>
            public static void StartPolling(int intervalMs = 1000)
            {
                if (_timer == null)
                {
                    _timer = new Timer();
                    _timer.Tick += Poll;
                }
                _timer.Interval = intervalMs;
                _timer.Start();
            }

            /// <summary>Stops polling. Does not clear subscribers.</summary>
            public static void StopPolling()
            {
                _timer?.Stop();
            }

            /// <summary>Stops polling and removes all event subscribers.</summary>
            public static void Dispose()
            {
                StopPolling();
                _timer?.Dispose();
                _timer = null;
                _last = null;
                OnChanged = null;
                OnPolled = null;
            }

            // ── Private helpers ────────────────────────────────────────

            private static void Poll(object sender, EventArgs e)
            {
                var info = Capture();
                OnPolled?.Invoke(info);

                // Fire OnChanged only when process or URL actually changed
                if (_last == null
                    || _last.ProcessName != info.ProcessName
                    || _last.Url != info.Url)
                {
                    _last = info;
                    OnChanged?.Invoke(info);
                }
            }

            private static ActiveWindowInfo Capture()
            {
                var result = new ActiveWindowInfo { CapturedAt = DateTime.Now };

                IntPtr hwnd = GetForegroundWindow();
                if (hwnd == IntPtr.Zero) return result;

                // Window title
                var sb = new StringBuilder(512);
                GetWindowText(hwnd, sb, sb.Capacity);
                result.WindowTitle = sb.ToString();

                // Process info
                GetWindowThreadProcessId(hwnd, out uint pid);
                try
                {
                    var proc = Process.GetProcessById((int)pid);
                    result.ProcessName = proc.ProcessName;
                    result.AppName = GetFriendlyName(proc);

                    if (IsBrowserProcess(proc.ProcessName))
                        result.Url = TryGetBrowserUrl(hwnd);
                }
                catch { /* process exited between calls */ }

                return result;
            }

            private static bool IsBrowserProcess(string name) =>
                Array.Exists(_browsers, b => b.Equals(name, StringComparison.OrdinalIgnoreCase));

            private static string GetFriendlyName(Process proc)
            {
                try
                {
                    var desc = proc.MainModule?.FileVersionInfo?.FileDescription;
                    if (!string.IsNullOrWhiteSpace(desc)) return desc;
                }
                catch { }
                return proc.ProcessName;
            }

            private static string TryGetBrowserUrl(IntPtr hwnd)
            {
                try
                {
                    var root = AutomationElement.FromHandle(hwnd);
                    var condition = new AndCondition(
                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit),
                        new OrCondition(
                            new PropertyCondition(AutomationElement.NameProperty, "Address and search bar"), // Chrome / Edge
                            new PropertyCondition(AutomationElement.NameProperty, "Address bar"),            // Firefox (older)
                            new PropertyCondition(AutomationElement.NameProperty, "Search or enter address") // Firefox
                        )
                    );

                    var bar = root.FindFirst(TreeScope.Descendants, condition);
                    if (bar?.GetCurrentPattern(ValuePattern.Pattern) is ValuePattern vp)
                        return vp.Current.Value;
                }
                catch { }
                return null;
            }
        }
    }
}

