using BNet.Cafe.Client.Ashx;
using BNet.Cafe.Client.Controls;
using BNet.Cafe.Client.Design;
using BNet.Cafe.Client.Models;
using BNet.Cafe.Client.Repositories;
using BNet.Cafe.Client.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BNet.Cafe.Client
{
    public partial class MainForm : Form
    {
        private const int ReconnectDelayMs = 500;
        private const int ActivityIntervalMs = 1000;
        private const int SendTimeoutMs = 600;
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        private static uint GetSystemIdleTimeMs()
        {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);

            if (GetLastInputInfo(ref lastInputInfo))
            {
                uint currentTick = (uint)Environment.TickCount;
                if (currentTick >= lastInputInfo.dwTime)
                    return currentTick - lastInputInfo.dwTime;
            }

            return 0;
        }

        private System.Windows.Forms.Timer _idleCheckTimer;
        private const int InputIdleThresholdSeconds = 10;

        private bool _userInteracted = false;
        private DateTime _monitoringStartTime;
        private Point _lastMousePosition = Point.Empty;

        private volatile bool _paused = true;
        private readonly SemaphoreSlim _resumeSignal = new SemaphoreSlim(0, 1);
        private readonly ConcurrentDictionary<int, byte[]> _pendingFrames = new ConcurrentDictionary<int, byte[]>();
        private readonly SemaphoreSlim _framePending = new SemaphoreSlim(0, 1);

        private volatile bool _isBacklogged = false;
        private volatile int[] _requestedScreenIndices = new int[0];

        private TextContent _deviceInfo;
        private volatile AgentSession _currentSession;
        private static BNetCafeTimer _BNetCafeTimer = new BNetCafeTimer();

        private readonly ScreenStreamer _screenStreamer = new ScreenStreamer();
        private readonly ActivityReporter _activityReporter = new ActivityReporter();
        private WallpaperService _wallpaperService = new WallpaperService();

        private SlideshowManager _slideshowManager;

        private LoginControl _loginControl;
        private RegisterControl _registerControl;

        private int IdleTimeoutSeconds
        {
            get
            {
                if (int.TryParse(ConfigHelper.AutoShutDownInterval?.Trim(), out int timeout))
                {
                    return timeout;
                }
                return 300;
            }
        }

        public MainForm()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            InitializeComponent();

            _slideshowManager = new SlideshowManager(
                parentForm: this,
                onUserInteraction: RegisterUserInteraction,
                onAutoShutdownTriggered: () => CommandPromptService.ShutdownSystem()
            );

            _slideshowManager.AttachOverlayControl(lblBigPcName);
            _slideshowManager.AttachOverlayControl(lblSubtitle);
            _slideshowManager.AttachOverlayControl(badgeCountdown);
            _slideshowManager.AttachOverlayControl(lblDismissHint);
            _slideshowManager.AttachOverlayControl(Panel_LoginCard);

            InitializeAuthControls();

            this.MouseMove += MainForm_MouseMove;
            this.MouseDown += (s, e) => RegisterUserInteraction();
            RegisterMouseEventsRecursively(this);

            if (!EnvironmentHelper.IsDevelopment)
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                this.TopMost = true;
                this.ControlBox = false;
                this.MaximizeBox = false;
                this.MinimizeBox = false;
            }

            InitializeIdleCheckTimer();
        }

        private void InitializeAuthControls()
        {
            _loginControl = new LoginControl();
            _registerControl = new RegisterControl();

            _loginControl.Dock = DockStyle.Fill;
            _registerControl.Dock = DockStyle.Fill;

            _loginControl.UserActivityDetected += (s, e) => RegisterUserInteraction();
            _registerControl.UserActivityDetected += (s, e) => RegisterUserInteraction();

            _loginControl.SwitchToRegisterRequested += (s, e) =>
            {
                if (!ConfigHelper.IsAccountCreationAllowed)
                {
                    MessageBox.Show("Account creation is disabled.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                ShowView(_registerControl);
            };

            _registerControl.SwitchToLoginRequested += (s, e) => ShowView(_loginControl);
            _registerControl.RegistrationCompleted += (s, e) => ShowView(_loginControl);

            _loginControl.LoginSuccessful += async (s, args) =>
            {
                _BNetCafeTimer.isAdmin = args.IsAdmin;
                _BNetCafeTimer.userId = args.UserId;

                string clientName = ConfigHelper.GetClientNameFromIP();
                if (!args.IsAdmin)
                {
                    CreateRentalHandler rentalHandler = new CreateRentalHandler();
                    await rentalHandler.CreateRentalAsync(clientName, args.UserId, args.DurationMinutes, "0");

                    CreateBalanceHandler balanceHandler = new CreateBalanceHandler();
                    await balanceHandler.CreateBalanceAsync(args.UserId, "-" + args.DurationMinutes, "0", "LOGGING-IN");
                }

                await _BNetCafeTimer.CreateTimerDataAsync(TimeService.Get().ToString(), args.DurationMinutes, "0");

                _loginControl.ClearFields();
                StopIdleMonitor();
                TriggerImmediateActivityReport();
            };

            Panel_LoginCard.Controls.Add(_loginControl);
            Panel_LoginCard.Controls.Add(_registerControl);

            ShowView(_loginControl);
        }

        private void ShowView(Control targetView)
        {
            this.SuspendLayout();
            Panel_LoginCard.SuspendLayout();

            _loginControl.Visible = (targetView == _loginControl);
            _registerControl.Visible = (targetView == _registerControl);

            if (targetView == _registerControl)
            {
                Panel_LoginCard.Size = new Size(400, 520);
                _registerControl.ClearFields();
            }
            else
            {
                Panel_LoginCard.Size = new Size(400, 360);
                _loginControl.ClearFields();
            }

            Panel_LoginCard.Location = new Point(
                (this.ClientSize.Width - Panel_LoginCard.Width) / 2,
                (this.ClientSize.Height - Panel_LoginCard.Height) / 2
            );

            Panel_LoginCard.ResumeLayout(true);
            this.ResumeLayout(true);

            _slideshowManager?.PictureBox?.Invalidate();
        }

        private void RegisterMouseEventsRecursively(Control control)
        {
            if (control == null) return;

            control.MouseMove += (s, e) => CheckMouseMovement(Cursor.Position);
            control.MouseDown += (s, e) => RegisterUserInteraction();

            foreach (Control child in control.Controls)
            {
                RegisterMouseEventsRecursively(child);
            }
        }

        private void InitializeIdleCheckTimer()
        {
            _idleCheckTimer = new System.Windows.Forms.Timer();
            _idleCheckTimer.Interval = 500;
            _idleCheckTimer.Tick += IdleCheckTimer_Tick;
        }

        private void StartIdleMonitor()
        {
            _userInteracted = false;
            _lastUserInteractionTime = DateTime.Now;
            _monitoringStartTime = DateTime.Now;
            _lastMousePosition = Cursor.Position;

            ShowLoginForm();
            _idleCheckTimer.Start();

            _slideshowManager.Start(IdleTimeoutSeconds, badgeCountdown);
        }

        private void StopIdleMonitor()
        {
            _idleCheckTimer.Stop();
            _slideshowManager.Stop();
            HideLoginForm();
        }

        private void ShowLoginForm()
        {
            if (!Panel_LoginCard.Visible)
            {
                Panel_LoginCard.Visible = true;
                Panel_LoginCard.BringToFront();
                _slideshowManager.PictureBox.Invalidate();
            }
        }

        private void HideLoginForm()
        {
            if (Panel_LoginCard.Visible)
            {
                Panel_LoginCard.Visible = false;
                _slideshowManager.PictureBox.Invalidate();
            }
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            CheckMouseMovement(Cursor.Position);
        }

        private void CheckMouseMovement(Point currentScreenPos)
        {
            if (_lastMousePosition.IsEmpty)
            {
                _lastMousePosition = currentScreenPos;
                return;
            }

            if (Math.Abs(currentScreenPos.X - _lastMousePosition.X) > 5 ||
                Math.Abs(currentScreenPos.Y - _lastMousePosition.Y) > 5)
            {
                _lastMousePosition = currentScreenPos;
                RegisterUserInteraction();
            }
        }

        private DateTime _lastUserInteractionTime = DateTime.Now;

        private void RegisterUserInteraction()
        {
            if (!_idleCheckTimer.Enabled) return;
            if (GetSystemIdleTimeMs() > 1000) return;

            _lastUserInteractionTime = DateTime.Now;
            _userInteracted = true;
            ShowLoginForm();

            if (ConfigHelper.ResetShutdownCountdown)
            {
                _slideshowManager.ResetCountdown(IdleTimeoutSeconds);
            }
        }

        private void IdleCheckTimer_Tick(object sender, EventArgs e)
        {
            if (SessionLogin.Exists())
            {
                StopIdleMonitor();
                return;
            }

            uint systemIdleMs = GetSystemIdleTimeMs();
            double systemIdleSeconds = systemIdleMs / 1000.0;
            double appIdleSeconds = (DateTime.Now - _lastUserInteractionTime).TotalSeconds;

            if (systemIdleSeconds >= InputIdleThresholdSeconds && appIdleSeconds >= InputIdleThresholdSeconds)
            {
                if (Panel_LoginCard.Visible)
                {
                    HideLoginForm();
                }
            }
        }

        public void LockScreen()
        {
            _wallpaperService?.Stop();

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(LockScreen));
                return;
            }

            KeyboardHook.Start();

            this.Show();
            this.BringToFront();
            this.Activate();
            this.Focus();

            StartIdleMonitor();
        }

        public void UnlockScreen()
        {
            HideLoginForm();
            _wallpaperService?.StartAsync();

            KeyboardHook.Stop();
            StopIdleMonitor();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _slideshowManager?.Dispose();
            HideLoginForm();
            _wallpaperService?.Stop();
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            await _wallpaperService?.SyncWallpapersFromServerAsync();

            string clientName = ConfigHelper.GetClientNameFromIP();
            lblBigPcName.Text = string.IsNullOrEmpty(clientName) ? "PC-01" : clientName;
            await Task.Delay(1000);
            _deviceInfo = await DeviceInfoCollector.GatherDeviceInfoAsync();
            await SessionPricingRate.InitializeRatesAsync();
            var session = SessionLogin.ReadSession();
            if (session != null)
            {
                TriggerImmediateActivityReport();
                _BNetCafeTimer.isAdmin = session.isAdmin;
                this.Hide();
                _BNetCafeTimer.Show();
                ClearTitleCache();

                StopIdleMonitor();
            }
            else
            {
                StartIdleMonitor();
            }

            _screenStreamer.RebuildScreenKeyCache(ScreenCaptured.GetScreenCount(), _deviceInfo.ClientName);

            UserActivity.ActiveWindowMonitor.OnPolled += async info =>
            {
                bool fileExists = SessionLogin.Exists();

                this.BeginInvoke((Action)(() =>
                {
                    if (!fileExists)
                    {
                        if (!_BNetCafeTimer.Visible && !this.Visible)
                        {
                            LockScreen();
                        }

                        if (_BNetCafeTimer.Visible)
                        {
                            _BNetCafeTimer.Hide();
                        }
                    }
                    else
                    {
                        if (this.Visible)
                        {
                            this.Hide();
                            StopIdleMonitor();
                        }

                        if (!_BNetCafeTimer.Visible)
                        {
                            _BNetCafeTimer.Show();
                            UnlockScreen();
                        }
                    }
                }));

                if (!_BNetCafeTimer.isAdmin)
                {
                    SecurityAccessManager.EnforceRestrictions(info);
                }
                var sess = _currentSession;
                if (sess == null || !sess.IsOpen) return;
                await _activityReporter.SendActivityInfoAsync(sess, info, _deviceInfo);
            };

            UserActivity.ActiveWindowMonitor.StartPolling(ActivityIntervalMs);
            _ = Task.Run(ContinuousPendingLogoutSyncLoopAsync);
            _ = Task.Run(RunAgentLoop);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_LBUTTONDOWN = 0x0201;
            const int WM_RBUTTONDOWN = 0x0204;
            const int WM_KEYDOWN = 0x0100;

            if (m.Msg == WM_LBUTTONDOWN || m.Msg == WM_RBUTTONDOWN || m.Msg == WM_KEYDOWN)
            {
                RegisterUserInteraction();
            }

            base.WndProc(ref m);
        }

        private async Task ContinuousPendingLogoutSyncLoopAsync()
        {
            while (true)
            {
                try
                {
                    if (SessionLogout.HasPendingLogout())
                    {
                        await SessionLogout.ProcessPendingLogoutAsync();
                    }
                }
                catch { }

                await Task.Delay(10000);
            }
        }

        private async Task RunAgentLoop()
        {
            string wsUrl = ConfigHelper.WebSocketUrl;
            Uri agentWsUri = new Uri($"{wsUrl}/ws/agent");

            while (true)
            {
                try
                {
                    _paused = true;
                    _requestedScreenIndices = new int[0];
                    _pendingFrames.Clear();
                    _screenStreamer.ClearCache();
                    _isBacklogged = false;

                    while (_resumeSignal.CurrentCount > 0) _resumeSignal.Wait(0);
                    while (_framePending.CurrentCount > 0) _framePending.Wait(0);

                    using (var ws = new ClientWebSocket())
                    {
                        ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
                        ws.Options.SetBuffer(64 * 1024, 64 * 1024);

                        await ws.ConnectAsync(agentWsUri, CancellationToken.None);

                        var session = new AgentSession(ws, SendTimeoutMs);
                        _currentSession = session;

                        var captureTask = _screenStreamer.CaptureLoopAsync(
                            session,
                            () => _paused,
                            () => _isBacklogged,
                            () => _requestedScreenIndices,
                            _pendingFrames,
                            _resumeSignal,
                            _framePending,
                            _deviceInfo.ClientName
                        );

                        var sendTask = SendLoop(session);
                        var recvTask = ReceiveLoop(session);

                        await Task.WhenAny(captureTask, sendTask, recvTask);
                        _currentSession = null;

                        _framePending.Release();
                    }
                }
                catch { }

                await Task.Delay(ReconnectDelayMs);
            }
        }

        private async Task SendLoop(AgentSession session)
        {
            while (session.IsOpen)
            {
                bool got = await _framePending.WaitAsync(500);
                if (!got) continue;

                foreach (var kv in _pendingFrames.ToArray())
                {
                    if (!session.IsOpen) return;

                    if (!_pendingFrames.TryRemove(kv.Key, out byte[] payload)) continue;

                    bool ok = await session.SendAsync(payload, WebSocketMessageType.Binary);
                    if (!ok)
                    {
                        _isBacklogged = true;
                        return;
                    }
                    _isBacklogged = false;
                }
            }
        }

        private async Task ReceiveLoop(AgentSession session)
        {
            var buf = new byte[64 * 1024];

            while (session.IsOpen)
            {
                try
                {
                    byte[] msg = await ReceiveFullMessage(session.WebSocket, buf);
                    if (msg == null) return;
                    if (msg.Length == 0) continue;

                    byte msgType = msg[0];

                    if (msgType == 0x02)
                    {
                        try
                        {
                            string textMessage = Utf8.GetString(msg, 1, msg.Length - 1);
                            HandleIncomingTextMessage(textMessage);
                        }
                        catch { }
                    }
                    else if (msgType == 0x40)
                    {
                        _paused = true;
                        _pendingFrames.Clear();
                        _screenStreamer.ClearCache();
                    }
                    else if (msgType == 0x41)
                    {
                        _paused = false;
                        _isBacklogged = false;
                        if (_resumeSignal.CurrentCount == 0)
                            _resumeSignal.Release();
                    }
                    else if (msgType == 0x43)
                    {
                        try
                        {
                            string json = Utf8.GetString(msg, 1, msg.Length - 1);
                            int[] indices = JsonConvert.DeserializeObject<int[]>(json) ?? new int[0];
                            _requestedScreenIndices = indices;

                            var indexSet = new HashSet<int>(indices);
                            foreach (var key in _pendingFrames.Keys.ToArray())
                                if (!indexSet.Contains(key)) _pendingFrames.TryRemove(key, out _);

                            _screenStreamer.RemoveKeysNotIn(indexSet);
                        }
                        catch { }
                    }
                    else if (msgType == 0x30)
                    {
                        string json = Utf8.GetString(msg, 1, msg.Length - 1);
                        var events = JsonConvert.DeserializeObject<List<RemoteInput>>(json);
                        if (events != null)
                            foreach (var evt in events)
                                try { RemoteController.Dispatch(evt); } catch { }
                    }
                }
                catch { return; }
            }
        }

        private void ClearTitleCache()
        {
            _activityReporter.ClearTitleCache();
        }

        private async void HandleIncomingTextMessage(string textMessage)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => HandleIncomingTextMessage(textMessage)));
                return;
            }

            if (Repositories.JsonValidation.IsValidJson(textMessage))
            {
                var jsonObject = JObject.Parse(textMessage);
                string userId = jsonObject["userId"]?.ToString() ?? "Guest / Walk-in";
                string duration = jsonObject["duration"]?.ToString() ?? "0";
                string amount = jsonObject["amount"]?.ToString() ?? "0";
                string dateTime = jsonObject["dateTime"]?.ToString() ?? TimeService.Get().ToString();

                if (_BNetCafeTimer == null || _BNetCafeTimer.IsDisposed)
                {
                    _BNetCafeTimer = new BNetCafeTimer();
                }

                if (!SessionLogin.Exists())
                {
                    _BNetCafeTimer.isAdmin = false;
                    _BNetCafeTimer.userId = userId;
                    await _BNetCafeTimer.CreateTimerDataAsync(dateTime, duration, amount);
                }
                else
                {
                    await _BNetCafeTimer.UpdateTimerDataAsync(duration, amount);
                    NotificationForm.Show("Time Updated", "Your session dynamic time was updated successfully.", NotificationType.Success);
                }

                UnlockScreen();
                this.Hide();
                ClearTitleCache();
                TriggerImmediateActivityReport();
            }
            else if (textMessage == "PAUSE")
            {
                if (_BNetCafeTimer != null && !_BNetCafeTimer.IsDisposed)
                {
                    _BNetCafeTimer.PauseTimer();
                    NotificationForm.Show("Session Paused", "Your timer has been temporarily paused.", NotificationType.Info);
                }
                ClearTitleCache();
                TriggerImmediateActivityReport();
            }
            else if (textMessage == "RESUME")
            {
                if (_BNetCafeTimer != null && !_BNetCafeTimer.IsDisposed)
                {
                    _BNetCafeTimer.ResumeTimer();
                    NotificationForm.Show("Session Resumed", "Your timer is now active.", NotificationType.Success);
                }
                ClearTitleCache();
                TriggerImmediateActivityReport();
            }
            else if (textMessage == "LOGOUT")
            {
                if (_BNetCafeTimer != null && !_BNetCafeTimer.IsDisposed)
                {
                    _BNetCafeTimer.Logout();
                }

                ClearTitleCache();
                LockScreen();
                TriggerImmediateActivityReport();
            }
            else if (textMessage == "SHUTDOWN")
            {
                CommandPromptService.ShutdownSystem();
            }
            else if (textMessage == "RESTART")
            {
                CommandPromptService.RestartSystem();
            }
            else
            {
                MessageBox.Show(
                    textMessage,
                    "Server Message",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.ServiceNotification
                );
            }
        }

        private async void TriggerImmediateActivityReport()
        {
            await Task.Delay(1000).ContinueWith(_ =>
            {
                var currentWindow = UserActivity.ActiveWindowMonitor.GetCurrent();
                var sess = _currentSession;
                _ = _activityReporter.HeartBeat(sess);

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private static async Task<byte[]> ReceiveFullMessage(WebSocket ws, byte[] buffer)
        {
            WebSocketReceiveResult result;
            try
            {
                result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            catch { return null; }

            if (result.MessageType == WebSocketMessageType.Close) return null;

            if (result.EndOfMessage)
            {
                var single = new byte[result.Count];
                Buffer.BlockCopy(buffer, 0, single, 0, result.Count);
                return single;
            }

            using (var ms = new MemoryStream())
            {
                ms.Write(buffer, 0, result.Count);
                do
                {
                    try
                    {
                        result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    }
                    catch { return null; }
                    if (result.MessageType == WebSocketMessageType.Close) return null;
                    ms.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);
                return ms.ToArray();
            }
        }
    }
}