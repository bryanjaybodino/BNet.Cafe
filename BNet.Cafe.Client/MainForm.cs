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
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices; //Required for Win32 API
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

        //Win32 API to monitor system-wide Mouse & Keyboard inactivity
        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        //Track manual keyboard timestamps when KeyboardHook intercepts input
        private static uint _lastManualInputTick = 0;

        private static uint GetIdleTimeMs()
        {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            uint apiTick = 0;

            if (GetLastInputInfo(ref lastInputInfo))
            {
                apiTick = lastInputInfo.dwTime;
            }

            // Take the most recent user activity between Win32 GetLastInputInfo and Hook triggers
            uint mostRecentTick = Math.Max(apiTick, _lastManualInputTick);
            uint currentTick = (uint)Environment.TickCount;

            if (currentTick >= mostRecentTick)
                return currentTick - mostRecentTick;

            return 0;
        }

        //Auto-shutdown variables
        private System.Windows.Forms.Timer _idleShutdownTimer;
        private const int InputIdleThresholdSeconds = 10; // Trigger countdown if no mouse/keyboard input for 10s
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

        private int _remainingIdleSeconds = 300; // 5 minutes total countdown (300 seconds)
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

        public MainForm()
        {
            InitializeComponent();

            if (!EnvironmentHelper.IsDevelopment)
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                this.TopMost = true;
                this.ControlBox = false;
                this.MaximizeBox = false;
                this.MinimizeBox = false;
            }

            //Initialize idle timer
            InitializeIdleShutdownTimer();
        }

        //Auto-Shutdown Helper Methods
        private void InitializeIdleShutdownTimer()
        {
            _remainingIdleSeconds = IdleTimeoutSeconds;
            _idleShutdownTimer = new System.Windows.Forms.Timer();
            _idleShutdownTimer.Interval = 1000; // 1 second interval
            _idleShutdownTimer.Tick += IdleShutdownTimer_Tick;
        }

        private void StartIdleCountdown()
        {
            _lastManualInputTick = (uint)Environment.TickCount;
            _remainingIdleSeconds = IdleTimeoutSeconds;
            _idleShutdownTimer.Start();
        }

        private void StopIdleCountdown()
        {
            _idleShutdownTimer.Stop();
            _remainingIdleSeconds = IdleTimeoutSeconds;
            string clientName = ConfigHelper.GetClientNameFromIP();
            lblStatusBadge.Text = $"● Station {clientName} Online";
            lblStatusBadge.BackColor = System.Drawing.Color.FromArgb(220, 252, 231);
            lblStatusBadge.ForeColor = System.Drawing.Color.FromArgb(22, 101, 52);
        }

        private void ResetIdleCountdown()
        {
            _remainingIdleSeconds = IdleTimeoutSeconds;
            string clientName = ConfigHelper.GetClientNameFromIP();
            lblStatusBadge.Text = $"● Station {clientName} Online";
            lblStatusBadge.BackColor = System.Drawing.Color.FromArgb(220, 252, 231);
            lblStatusBadge.ForeColor = System.Drawing.Color.FromArgb(22, 101, 52);
        }

        private void IdleShutdownTimer_Tick(object sender, EventArgs e)
        {
            //If Zero dont no auto shutdown
            if (_remainingIdleSeconds == 0)
            {
                _idleShutdownTimer.Stop();
                return;
            }

            // If an active session exists, cancel countdown immediately
            if (SessionLogin.Exists())
            {
                StopIdleCountdown();
                return;
            }

            // Calculate current physical idle time in seconds
            uint idleMs = GetIdleTimeMs();
            uint idleSeconds = idleMs / 1000;

            //If user touches mouse or keyboard, reset the countdown completely
            if (idleSeconds < InputIdleThresholdSeconds)
            {
                ResetIdleCountdown();
                return;
            }

            // User has been physically idle for at least 10 seconds -> tick down the 5-minute timer
            _remainingIdleSeconds--;

            if (_remainingIdleSeconds <= 0)
            {
                _idleShutdownTimer.Stop();
                ShutdownSystem();
                return;
            }

            UpdateShutdownBadgeUI();
        }

        private void UpdateShutdownBadgeUI()
        {
            TimeSpan ts = TimeSpan.FromSeconds(_remainingIdleSeconds);
            lblStatusBadge.Text = $"⚠️ Auto-Shutdown in: {ts.Minutes:D2}:{ts.Seconds:D2}";
            lblStatusBadge.BackColor = System.Drawing.Color.FromArgb(254, 226, 226);
            lblStatusBadge.ForeColor = System.Drawing.Color.FromArgb(185, 28, 28);
        }

        private void ShutdownSystem()
        {
            try
            {
                Process.Start("shutdown", "/s /t 10 /f /c \"No physical activity detected for 10 seconds. PC shutting down.\"");
                Application.Exit();
            }
            catch { }
        }

        public void LockScreen()
        {
            // 1. Always check InvokeRequired FIRST before touching any UI properties
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(LockScreen));
                return;
            }

            // 2. Start global low-level hooks
            KeyboardHook.Start();

            ////// 4. Show and force focus
            this.Show();
            this.BringToFront();
            this.Activate();
            this.Focus();

            //Start monitoring idle input when screen locks
            StartIdleCountdown();
        }

        public void UnlockScreen()
        {
            KeyboardHook.Stop();

            //Stop idle countdown when screen unlocks
            StopIdleCountdown();
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            string clientName = ConfigHelper.GetClientNameFromIP();
            lblBigPcName.Text = clientName;
            lblStatusBadge.Text = $"● Station {clientName} Online";
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

                //Active session running on startup -> stop idle countdown
                StopIdleCountdown();
            }
            else
            {
                //No session on startup -> start idle monitor
                StartIdleCountdown();
            }

            _screenStreamer.RebuildScreenKeyCache(ScreenCaptured.GetScreenCount(), _deviceInfo.ClientName);

            UserActivity.ActiveWindowMonitor.OnPolled += async info =>
            {

                // 1. Check disk on background thread
                bool fileExists = SessionLogin.Exists();

                // 2. Safe UI update on the UI thread
                this.BeginInvoke((Action)(() =>
                {
                    if (!fileExists)
                    {
                        // NO ACTIVE SESSION: Show MainForm (login screen), Hide Timer
                        if (!_BNetCafeTimer.Visible && !this.Visible)
                        {
                            // Unlocks keyboard so user can type in username/password
                            LockScreen();
                        }

                        if (_BNetCafeTimer.Visible)
                        {
                            _BNetCafeTimer.Hide();
                        }
                    }
                    else
                    {
                        // ACTIVE SESSION RUNNING: Hide MainForm, Show Timer
                        if (this.Visible)
                        {
                            this.Hide();

                            //Stop idle countdown when session starts
                            StopIdleCountdown();
                        }

                        if (!_BNetCafeTimer.Visible)
                        {
                            _BNetCafeTimer.Show();
                            // Stops hook during active session as well
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

        //Update input timestamp on user typing
        private void TextBox_Username_TextChanged(object sender, EventArgs e)
        {
            _lastManualInputTick = (uint)Environment.TickCount;
            ResetIdleCountdown();
        }

        private void TextBox_Password_TextChanged(object sender, EventArgs e)
        {
            _lastManualInputTick = (uint)Environment.TickCount;
            ResetIdleCountdown();
        }

        protected override void WndProc(ref Message m)
        {
            //Intercept Windows mouse/key messages on the Form to keep track of user activity
            const int WM_MOUSEMOVE = 0x0200;
            const int WM_LBUTTONDOWN = 0x0201;
            const int WM_RBUTTONDOWN = 0x0204;
            const int WM_KEYDOWN = 0x0100;

            if (m.Msg == WM_MOUSEMOVE || m.Msg == WM_LBUTTONDOWN || m.Msg == WM_RBUTTONDOWN || m.Msg == WM_KEYDOWN)
            {
                _lastManualInputTick = (uint)Environment.TickCount;
            }

            base.WndProc(ref m);
        }

        private async Task ContinuousPendingLogoutSyncLoopAsync()
        {
            while (true)
            {
                try
                {
                    // First check: notepad / pending file exist?
                    if (SessionLogout.HasPendingLogout())
                    {
                        // Second check: check server & third: save to DB & fourth: remove file
                        await SessionLogout.ProcessPendingLogoutAsync();
                    }
                }
                catch { }

                // Poll every 10 seconds for offline logouts to re-sync
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

            // 1. Check offline admin bypass first before network calls
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

                // 2. Handle failed server response or null payload safely
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

                // 3. Admin Check (Server role OR Local Admin credentials)
                bool isAdmin = isLocalAdmin || string.Equals(loginResponse?.Data?.Role, "ADMIN", StringComparison.OrdinalIgnoreCase);

                if (isAdmin)
                {
                    AdminLogin();
                    return;
                }

                // 4. Insufficient Balance Check
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

                // 5. Standard User Rental Process
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

                //Stop idle countdown when successfully logged in
                StopIdleCountdown();

                TriggerImmediateActivityReport();
            }
            catch (Exception ex)
            {
                // Prevent system termination on network failure or server disconnect
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

                //Stop idle countdown on admin login
                StopIdleCountdown();

                TriggerImmediateActivityReport();
                return;
            }
        }

        private void Button_Register_Click(object sender, EventArgs e)
        {
            if (!ConfigHelper.IsAccountCreationAllowed)
            {
                MessageBox.Show("Account creation is disabled.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // 1. Pause active window polling so LockScreen() isn't continuously called in the background
            UserActivity.ActiveWindowMonitor.StopPolling();

            using (RegisterForm registerForm = new RegisterForm())
            {
                // 3. Keep TopMost on RegisterForm so it stays above MainForm
                registerForm.TopMost = true;
                registerForm.StartPosition = FormStartPosition.CenterScreen;
                // 2. Stop the global keyboard hook to allow typing in the registration form
                UnlockScreen();
                // 4. Pass 'this' as owner so RegisterForm displays directly OVER MainForm without hiding MainForm
                registerForm.ShowDialog(this);
            }

            // 5. Re-enable security hooks and window monitoring when RegisterForm closes
            if (!SessionLogin.Exists())
            {
                LockScreen();
            }
            UserActivity.ActiveWindowMonitor.StartPolling(ActivityIntervalMs);
        }
    }
}