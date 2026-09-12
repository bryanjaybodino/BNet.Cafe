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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

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

            // 3. Configure window for full-screen lock mode
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.ControlBox = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // 4. Show and force focus
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
            bool isAdministrator = false;
            lblBigPcName.Text = clientName;
            lblStatusBadge.Text = $"● Station {clientName} Online";
            await Task.Delay(1000);
            _deviceInfo = await DeviceInfoCollector.GatherDeviceInfoAsync();
            string sessionFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "session.txt");
            if (File.Exists(sessionFilePath))
            {

                string[] parts = File.ReadAllText(sessionFilePath).Split('|');

                if (parts.Length >= 4 &&
                    long.TryParse(parts[0], out long createdTicks) &&
                    long.TryParse(parts[1], out long endTicks))
                {
                    string userId = parts[2];
                    double.TryParse(parts[3], out double amount);
                    isAdministrator = userId == "Administrator" && amount == 0;
                }

                this.Hide();
                _BNetCafeTimer.Show();
                ClearTitleCache();

            }

            _screenStreamer.RebuildScreenKeyCache(ScreenCaptured.GetScreenCount(), _deviceInfo.ClientName);

            UserActivity.ActiveWindowMonitor.OnPolled += async info =>
            {

                // 1. Check disk on background thread
                bool fileExists = File.Exists(sessionFilePath);

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


                if (!isAdministrator)
                {
                    string processName = info?.ProcessName ?? string.Empty;
                    string windowTitle = info?.WindowTitle ?? string.Empty;

                    bool isTaskManager = processName.Equals("Taskmgr", StringComparison.OrdinalIgnoreCase);
                    bool isControlPanel = windowTitle.IndexOf("Programs and Features", StringComparison.OrdinalIgnoreCase) >= 0;

                    if (isTaskManager || isControlPanel)
                    {
                        try
                        {
                            foreach (var proc in Process.GetProcessesByName("Taskmgr")) proc.Kill();
                            foreach (var proc in Process.GetProcessesByName("explorer"))
                            {
                                if (proc.MainWindowTitle.Contains("Programs and Features")) proc.Kill();
                            }
                        }
                        catch { }
                        return;
                    }
                }
                var sess = _currentSession;
                if (sess == null || !sess.IsOpen) return;
                await _activityReporter.SendActivityInfoAsync(sess, info, _deviceInfo);

            };

            UserActivity.ActiveWindowMonitor.StartPolling(ActivityIntervalMs);
            _ = Task.Run(RunAgentLoop);
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

        private void HandleIncomingTextMessage(string textMessage)
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

                string sessionFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "session.txt");

                // 1. Instantiate timer if null or disposed
                if (_BNetCafeTimer == null || _BNetCafeTimer.IsDisposed)
                {
                    _BNetCafeTimer = new BNetCafeTimer();
                }

                // 2. Update or Create Timer Data
                if (!File.Exists(sessionFilePath))
                {
                    _BNetCafeTimer.CreateTimerData(dateTime, userId, duration, amount);
                }
                else
                {
                    _BNetCafeTimer.UpdateTimerData(duration, amount);
                }

                // 3. Immediately reflect UI changes and unlock PC
                UnlockScreen();
                this.Hide();
                ClearTitleCache();
            }
            else if (textMessage == "LOGOUT")
            {
                _BNetCafeTimer?.Logout();

                // 1. Clear title cache first so the next window poll is guaranteed to send
                ClearTitleCache();

                // 2. Lock screen and force window focus
                LockScreen();

                // 3. Manually trigger activity update AFTER window focus has settled
                Task.Delay(200).ContinueWith(_ =>
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

            LoginHandler loginHandler = new LoginHandler();
            var loginResponse = await loginHandler.LoginAsync(username, password);

            // 1. Guard clause: Handle failed authentication immediately
            if (!loginResponse.Success && username != "BNet")
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

            // 2. Admin Check
            bool isAdmin = (username == "BNet" && password == "@12345") || string.Equals(loginResponse.Data.Role, "ADMIN", StringComparison.OrdinalIgnoreCase);

            if (isAdmin)
            {
                _BNetCafeTimer.CreateTimerData(TimeService.Get().ToString(), "Administrator", "6000", "0");
                TextBox_Username.Text = string.Empty;
                TextBox_Password.Text = string.Empty;
                return;
            }

            // 3. Insufficient Balance Check
            if (loginResponse.Data.TotalDuration <= 0)
            {
                MessageBox.Show(
                    "Your account balance is 0. Please top up at the counter to continue.",
                    "Insufficient Balance",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            // 4. Standard User Rental Process
            string userId = loginResponse.Data.Id.ToString();
            string durationMinutes = loginResponse.Data.TotalDuration.ToString();
            string clientName = ConfigurationManager.AppSettings["ClientName"];

            CreateRentalHandler createRentalHandler = new CreateRentalHandler();
            await createRentalHandler.CreateRentalAsync(clientName, userId, durationMinutes, "0");

            CreateBalanceHandler createBalanceHandler = new CreateBalanceHandler();
            await createBalanceHandler.CreateBalanceAsync(userId, "-" + durationMinutes, "0", "LOGGING-IN");

            _BNetCafeTimer.CreateTimerData(TimeService.Get().ToString(), userId, durationMinutes, "0");

            TextBox_Username.Text = string.Empty;
            TextBox_Password.Text = string.Empty;
        }
        private void Button_Register_Click(object sender, EventArgs e)
        {
            using (OAuthLoginForm regForm = new OAuthLoginForm())
            {
                regForm.ShowDialog(this);
            }
        }
        public static void OAuth_Login(string userId, string durationMinutes)
        {
            MainForm activeForm = Application.OpenForms.OfType<MainForm>().FirstOrDefault();
            if (activeForm == null) return;

            // Handle cross-thread UI updates safely
            if (activeForm.InvokeRequired)
            {
                activeForm.BeginInvoke(new Action(() => OAuth_Login(userId, durationMinutes)));
                return;
            }

            // Timer creation logic
            _BNetCafeTimer.CreateTimerData(
                TimeService.Get().ToString(),
                userId,
                durationMinutes,
                "0"
            );

        }
    }
}