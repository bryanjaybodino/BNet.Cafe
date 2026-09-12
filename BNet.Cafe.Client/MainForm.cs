using BNet.Cafe.Client.Ashx;
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
        }
        public void UnlockScreen()
        {
            KeyboardHook.Stop();
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            string clientName = ConfigurationManager.AppSettings["ClientName"];
            lblBigPcName.Text = clientName;
            lblStatusBadge.Text = $"● Station {clientName} Online";
            await Task.Delay(1000);
            _deviceInfo = await DeviceInfoCollector.GatherDeviceInfoAsync();

            var session = SessionManager.ReadSession();
            if (session != null)
            {
                _BNetCafeTimer.isAdministrator = session.IsAdministrator;
                this.Hide();
                _BNetCafeTimer.Show();
                ClearTitleCache();
            }

            _screenStreamer.RebuildScreenKeyCache(ScreenCaptured.GetScreenCount(), _deviceInfo.ClientName);

            UserActivity.ActiveWindowMonitor.OnPolled += async info =>
            {

                // 1. Check disk on background thread
                bool fileExists = SessionManager.Exists();

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
                        }

                        if (!_BNetCafeTimer.Visible)
                        {
                            _BNetCafeTimer.Show();
                            // Stops hook during active session as well
                            UnlockScreen();
                        }
                    }
                }));


                if (!_BNetCafeTimer.isAdministrator)
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
        private async Task ContinuousPendingLogoutSyncLoopAsync()
        {
            while (true)
            {
                try
                {
                    // First check: notepad / pending file exist?
                    if (PendingLogoutManager.HasPendingLogout())
                    {
                        // Second check: check server & third: save to DB & fourth: remove file
                        await PendingLogoutManager.ProcessPendingLogoutAsync();
                    }
                }
                catch { }

                // Poll every 10 seconds for offline logouts to re-sync
                await Task.Delay(10000);
            }
        }
        private async Task RunAgentLoop()
        {
            string wsUrl = ConfigurationManager.AppSettings["WebSocketUrl"];
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

                if (!SessionManager.Exists())
                {
                    _BNetCafeTimer.isAdministrator = false;
                    _BNetCafeTimer.userId = userId;
                    await _BNetCafeTimer.CreateTimerDataAsync(dateTime, duration, amount);
                }
                else
                {
                    await _BNetCafeTimer.UpdateTimerDataAsync(duration, amount);
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
                }
                ClearTitleCache();
                TriggerImmediateActivityReport();
            }
            else if (textMessage == "RESUME")
            {
                if (_BNetCafeTimer != null && !_BNetCafeTimer.IsDisposed)
                {
                    _BNetCafeTimer.ResumeTimer();
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

                await Task.Delay(200).ContinueWith(_ =>
                {
                    var currentWindow = UserActivity.ActiveWindowMonitor.GetCurrent();
                    var sess = _currentSession;
                    if (sess != null && sess.IsOpen)
                    {
                        _ = _activityReporter.SendActivityInfoAsync(sess, currentWindow, _deviceInfo);
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
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
        private void TriggerImmediateActivityReport()
        {
            var currentWindow = UserActivity.ActiveWindowMonitor.GetCurrent();
            var sess = _currentSession;
            if (sess != null && sess.IsOpen)
            {
                _ = _activityReporter.SendActivityInfoAsync(sess, currentWindow, _deviceInfo);
            }
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
                LoginHandler loginHandler = new LoginHandler();
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
                string clientName = ConfigurationManager.AppSettings["ClientName"];

                CreateRentalHandler createRentalHandler = new CreateRentalHandler();
                await createRentalHandler.CreateRentalAsync(clientName, userId, durationMinutes, "0");

                CreateBalanceHandler createBalanceHandler = new CreateBalanceHandler();
                await createBalanceHandler.CreateBalanceAsync(userId, "-" + durationMinutes, "0", "LOGGING-IN");

                _BNetCafeTimer.isAdministrator = false;
                _BNetCafeTimer.userId = userId;
                await _BNetCafeTimer.CreateTimerDataAsync(TimeService.Get().ToString(), durationMinutes, "0");

                TextBox_Username.Text = string.Empty;
                TextBox_Password.Text = string.Empty;
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
                _BNetCafeTimer.isAdministrator = true;
                _BNetCafeTimer.userId = "Administrator";
                await _BNetCafeTimer.CreateTimerDataAsync(TimeService.Get().ToString(), "6000", "0");
                TextBox_Username.Text = string.Empty;
                TextBox_Password.Text = string.Empty;
                return;
            }
        }

        private void Button_Register_Click(object sender, EventArgs e)
        {
            using (OAuthLoginForm regForm = new OAuthLoginForm())
            {
                regForm.ShowDialog(this);
            }
        }
    }
}