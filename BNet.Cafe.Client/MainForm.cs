using BNet.Cafe.Client.Ashx;
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

        // Win32 API to monitor system-wide Mouse & Keyboard inactivity
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
        private const int InputIdleThresholdSeconds = 10; // 10s timeout to hide login card

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

        // Slideshow & Countdown Controls
        private PictureBox _pictureBox;
        private System.Windows.Forms.Timer _slideTimer;
        private System.Windows.Forms.Timer _countdownTimer;
        private string[] _imageFiles = new string[0];
        private int _currentIndex = -1;
        private readonly string _wallpaperFolder;

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

        private int _remainingIdleSeconds = 300;

        public MainForm()
        {
            // Set double buffering styles to avoid flicker during panel redraws over PictureBox
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

            InitializeComponent();
            InitializeSlideshowControls();

            _wallpaperFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Wallpaper");
            LoadImages();

            _slideTimer = new System.Windows.Forms.Timer { Interval = 10000 };
            _slideTimer.Tick += SlideTimer_Tick;

            InitializeCountdownTimer();

            this.AcceptButton = Button_Login;
            TextBox_Username.KeyDown += TextBox_Username_KeyDown;
            TextBox_Password.KeyDown += TextBox_Password_KeyDown;

            // Attach mouse events using coordinate threshold comparison
            this.MouseMove += MainForm_MouseMove;
            this.MouseDown += (s, e) => RegisterUserInteraction();
            Panel_LoginCard.MouseDown += (s, e) => RegisterUserInteraction();

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

        #region Slideshow Implementation

        private void InitializeSlideshowControls()
        {
            _pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.FromArgb(15, 23, 42)
            };

            _pictureBox.Click += (s, e) => RegisterUserInteraction();
            _pictureBox.MouseMove += PictureBox_MouseMove;

            this.Controls.Add(_pictureBox);
            _pictureBox.SendToBack();

            // Re-parent labels to PictureBox for transparent overlay on images
            AttachControlToPicture(lblBigPcName);
            AttachControlToPicture(lblSubtitle);
            AttachControlToPicture(badgeCountdown);
            AttachControlToPicture(lblDismissHint);
            AttachControlToPicture(Panel_LoginCard);
        }

        private void AttachControlToPicture(Control control)
        {
            Point originalPos = control.Location;
            control.Parent = _pictureBox;
            control.Location = originalPos;

            if (control.BackColor == Color.Transparent)
            {
                control.BackColor = Color.FromArgb(160, 15, 23, 42);
            }
        }

        private void LoadImages()
        {
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp" };
            if (Directory.Exists(_wallpaperFolder))
            {
                _imageFiles = Directory.GetFiles(_wallpaperFolder)
                    .Where(f => validExtensions.Contains(Path.GetExtension(f).ToLower()))
                    .ToArray();
            }
        }

        private void InitializeCountdownTimer()
        {
            _remainingIdleSeconds = IdleTimeoutSeconds;
            _countdownTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _countdownTimer.Tick += CountdownTimer_Tick;
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            if (_remainingIdleSeconds <= 0)
            {
                _countdownTimer.Stop();
                CommandPromptService.ShutdownSystem();
                return;
            }

            _remainingIdleSeconds--;

            TimeSpan ts = TimeSpan.FromSeconds(_remainingIdleSeconds);
            badgeCountdown.Text = $"⚠️ Auto-Shutdown in: {ts.Minutes:D2}:{ts.Seconds:D2}";

            if (_remainingIdleSeconds <= 60)
            {
                badgeCountdown.BackColor = Color.FromArgb(200, 239, 68, 68);
                badgeCountdown.ForeColor = Color.White;
            }
        }

        private void SlideTimer_Tick(object sender, EventArgs e)
        {
            LoadNextImage();
        }

        private void LoadNextImage()
        {
            if (_imageFiles.Length == 0) return;

            try
            {
                _currentIndex = (_currentIndex + 1) % _imageFiles.Length;
                string imagePath = _imageFiles[_currentIndex];

                using (var stream = new MemoryStream(File.ReadAllBytes(imagePath)))
                {
                    var oldImage = _pictureBox.Image;
                    _pictureBox.Image = Image.FromStream(stream);
                    oldImage?.Dispose();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error displaying image: {ex.Message}");
            }
        }

        private void StartSlideshow()
        {
            LoadNextImage();
            if (_imageFiles.Length > 1)
            {
                _slideTimer.Start();
            }

            _remainingIdleSeconds = IdleTimeoutSeconds;
            _countdownTimer.Start();
        }

        private void StopSlideshow()
        {
            _slideTimer.Stop();
            _countdownTimer.Stop();
        }

        #endregion

        private void InitializeIdleCheckTimer()
        {
            _idleCheckTimer = new System.Windows.Forms.Timer();
            _idleCheckTimer.Interval = 500;
            _idleCheckTimer.Tick += IdleCheckTimer_Tick;
        }

        private void StartIdleMonitor()
        {
            _userInteracted = false;
            _monitoringStartTime = DateTime.Now;
            _lastMousePosition = Point.Empty;

            ShowLoginForm();
            _idleCheckTimer.Start();
            StartSlideshow();
        }

        private void StopIdleMonitor()
        {
            _idleCheckTimer.Stop();
            StopSlideshow();
            HideLoginForm();
        }

        private void ShowLoginForm()
        {
            if (!Panel_LoginCard.Visible)
                Panel_LoginCard.Visible = true;
        }

        private void HideLoginForm()
        {
            if (Panel_LoginCard.Visible)
                Panel_LoginCard.Visible = false;
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            CheckMouseMovement(e.Location);
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            CheckMouseMovement(e.Location);
        }

        private void CheckMouseMovement(Point currentPos)
        {
            if (_lastMousePosition.IsEmpty)
            {
                _lastMousePosition = currentPos;
                return;
            }

            // Require > 5 pixels displacement (matching SlideshowOverlayForm) before registering movement
            if (Math.Abs(currentPos.X - _lastMousePosition.X) > 5 || Math.Abs(currentPos.Y - _lastMousePosition.Y) > 5)
            {
                _lastMousePosition = currentPos;
                RegisterUserInteraction();
            }
        }

        private void RegisterUserInteraction()
        {
            if ((DateTime.Now - _monitoringStartTime).TotalSeconds < 1)
                return;

            _userInteracted = true;
            ShowLoginForm();
        }

        private void TextBox_Username_KeyDown(object sender, KeyEventArgs e)
        {
            RegisterUserInteraction();
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                Button_Login_Click(sender, e);
            }
        }

        private void TextBox_Password_KeyDown(object sender, KeyEventArgs e)
        {
            RegisterUserInteraction();
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                Button_Login_Click(sender, e);
            }
        }

        private void IdleCheckTimer_Tick(object sender, EventArgs e)
        {
            if (SessionLogin.Exists())
            {
                StopIdleMonitor();
                return;
            }

            uint idleMs = GetSystemIdleTimeMs();
            double idleSeconds = idleMs / 1000.0;
            double elapsedSecondsSinceStart = (DateTime.Now - _monitoringStartTime).TotalSeconds;

            // Hide if OS idle time reached threshold OR initial monitoring timeout reached without interaction
            if (idleSeconds >= InputIdleThresholdSeconds || (!_userInteracted && elapsedSecondsSinceStart >= InputIdleThresholdSeconds))
            {
                HideLoginForm();
            }
            else
            {
                ShowLoginForm();
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
            StopSlideshow();
            _pictureBox.Image?.Dispose();
            HideLoginForm();
            _wallpaperService?.Stop();
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            await _wallpaperService?.SyncWallpapersFromServerAsync();
            StartSlideshow();
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

        private void TextBox_Username_TextChanged(object sender, EventArgs e)
        {
            RegisterUserInteraction();
        }

        private void TextBox_Password_TextChanged(object sender, EventArgs e)
        {
            RegisterUserInteraction();
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_LBUTTONDOWN = 0x0201;
            const int WM_RBUTTONDOWN = 0x0204;
            const int WM_KEYDOWN = 0x0100;

            // Only register interaction on mouse clicks or keydown events to avoid mouse-move event flooding
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

        private async void Button_Login_Click(object sender, EventArgs e)
        {
            string username = TextBox_Username.Text.Trim();
            string password = TextBox_Password.Text;

            bool isLocalAdmin = (username == "BNetAdmin" && password == "@12345");

            if (isLocalAdmin)
            {
                AdminLogin();
                return;
            }
            try
            {
                GetLoginHandler loginHandler = new GetLoginHandler();
                var loginResponse = await loginHandler.LoginAsync(username, password);

                if (loginResponse == null || (!loginResponse.Success && !isLocalAdmin))
                {
                    MessageBox.Show(
                        "Invalid username or password. Please try again.",
                        "Login Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    TextBox_Password.Text = string.Empty;
                    TextBox_Password.Focus();
                    return;
                }

                bool isAdmin = isLocalAdmin || string.Equals(loginResponse?.Data?.Role, "ADMIN", StringComparison.OrdinalIgnoreCase);

                if (isAdmin)
                {
                    AdminLogin();
                    return;
                }

                if (loginResponse.Data == null || loginResponse.Data.TotalDuration <= 0)
                {
                    MessageBox.Show(
                        "Your account balance is 0. Please top up at the counter to continue.",
                        "Insufficient Balance",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    return;
                }

                string userId = loginResponse.Data.Id.ToString();
                string durationMinutes = loginResponse.Data.TotalDuration.ToString();
                string clientName = ConfigHelper.GetClientNameFromIP();

                CreateRentalHandler createRentalHandler = new CreateRentalHandler();
                await createRentalHandler.CreateRentalAsync(clientName, userId, durationMinutes, "0");

                CreateBalanceHandler createBalanceHandler = new CreateBalanceHandler();
                await createBalanceHandler.CreateBalanceAsync(userId, "-" + durationMinutes, "0", "LOGGING-IN");

                _BNetCafeTimer.isAdmin = false;
                _BNetCafeTimer.userId = userId;
                await _BNetCafeTimer.CreateTimerDataAsync(TimeService.Get().ToString(), durationMinutes, "0");

                TextBox_Username.Text = string.Empty;
                TextBox_Password.Text = string.Empty;

                StopIdleMonitor();
                TriggerImmediateActivityReport();
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "Unable to connect to the server. Please check your network connection or try again later.",
                    "Server Unavailable",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }

            async void AdminLogin()
            {
                _BNetCafeTimer.isAdmin = true;
                await _BNetCafeTimer.CreateTimerDataAsync(TimeService.Get().ToString(), "6000", "0");
                TextBox_Username.Text = string.Empty;
                TextBox_Password.Text = string.Empty;

                StopIdleMonitor();
                TriggerImmediateActivityReport();
            }
        }

        private void Button_Register_Click(object sender, EventArgs e)
        {
            if (!ConfigHelper.IsAccountCreationAllowed)
            {
                MessageBox.Show("Account creation is disabled.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            UserActivity.ActiveWindowMonitor.StopPolling();

            using (RegisterForm registerForm = new RegisterForm())
            {
                registerForm.TopMost = true;
                registerForm.StartPosition = FormStartPosition.CenterScreen;
                UnlockScreen();
                registerForm.ShowDialog(this);
            }

            if (!SessionLogin.Exists())
            {
                LockScreen();
            }
            UserActivity.ActiveWindowMonitor.StartPolling(ActivityIntervalMs);
        }
    }
}