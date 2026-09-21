using BNet.Cafe.Client.Ashx;
using BNet.Cafe.Client.Design;
using BNet.Cafe.Client.Repositories;
using BNet.Cafe.Client.Services;
using System;
using System.Configuration;
using System.Drawing;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web.SessionState;
using System.Windows.Forms;

namespace BNet.Cafe.Client
{
    public partial class BNetCafeTimer : Form
    {
        // Native Win32 window drag constants
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private double remainingSeconds = 0;
        private DateTime createdTime;
        private DateTime endTime;
        private bool isOpenTime = false;
        private bool hasWarned5Min = false;
        private bool hasWarned1Min = false;

        public bool isAdmin = false;
        public bool isRunning = false;
        public string userId = string.Empty;
        public bool isPaused = false;

        public BNetCafeTimer()
        {
            InitializeComponent();
            ConfigureFormStyle();
            _ = ResumeExistingSessionAsync();
        }

        private void ConfigureFormStyle()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = false;
            this.ShowInTaskbar = false;
        }

        private void Panel_Header_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private bool IsMemberUser()
        {
            return !string.IsNullOrWhiteSpace(userId)
                && !isAdmin
                && Label_CustomerName.Text != "Guest / Walk-in";
        }

        private bool IsWalkInUser()
        {
            return (!isAdmin) &&
                   (string.IsNullOrWhiteSpace(userId) || Label_CustomerName.Text == "Guest / Walk-in");
        }

        private void UpdateAccountActionButton()
        {
            if (IsMemberUser())
            {
                Button_AccountAction.Text = "Change Password";
                Button_AccountAction.BackColor = Color.FromArgb(99, 102, 241);
                Button_AccountAction.Visible = true;
            }
            else if (IsWalkInUser())
            {
                Button_AccountAction.Text = "Create Account";
                Button_AccountAction.BackColor = Color.FromArgb(16, 185, 129);
                Button_AccountAction.Visible = true;
            }
            else
            {
                Button_AccountAction.Visible = false;
            }
        }

        public async Task CreateTimerDataAsync(string serverTime, string duration, string amount)
        {
            double.TryParse(duration, out double parsedDurationMinutes);
            double.TryParse(amount, out double parsedAmount);

            createdTime = Convert.ToDateTime(serverTime);
            isOpenTime = (parsedDurationMinutes == 0);
            isPaused = false;
            hasWarned5Min = false;
            hasWarned1Min = false;
            endTime = isOpenTime ? DateTime.MaxValue : createdTime.AddMinutes(parsedDurationMinutes);

            await ApplyTimerDataAsync(parsedAmount);
        }

        public async Task UpdateTimerDataAsync(string newDuration, string newAmount)
        {
            double.TryParse(newDuration, out double parsedDurationMinutes);
            double.TryParse(newAmount, out double parsedAmount);
            isOpenTime = (parsedDurationMinutes == 0);
            hasWarned5Min = false;
            hasWarned1Min = false;
            endTime = isOpenTime ? DateTime.MaxValue : createdTime.AddMinutes(parsedDurationMinutes);

            await ApplyTimerDataAsync(parsedAmount);
        }

        public void Logout()
        {
            if (!Label_CustomerName.Text.Contains("Guest / Walk-in") && !string.IsNullOrEmpty(userId))
            {
                TimeSpan time = TimeSpan.FromSeconds(remainingSeconds);
                string totalMinutes = ((int)time.TotalMinutes).ToString();
                SessionLogout.SavePendingLogout(userId, totalMinutes, "0", "LOGGING-OUT");
            }
            isPaused = false;
            isOpenTime = false;
            endTime = createdTime.AddMinutes(0);
            _ = ApplyTimerDataAsync(0);
            ClearSessionFile();
        }

        private async Task ApplyTimerDataAsync(double amount)
        {
            DateTime now = TimeService.Get();
            remainingSeconds = isOpenTime ? (now - createdTime).TotalSeconds : (endTime - now).TotalSeconds;

            SessionLogin.SaveSession(createdTime, endTime, userId, amount, isAdmin, isOpenTime, isPaused);

            Label_ClientName.Text = ConfigHelper.GetClientNameFromIP();
            Label_CustomerName.Text = await ResolveDisplayNameAsync(userId);
            Label_StartTime.Text = $"Started At: {createdTime:hh:mm tt}";

            UpdateAccountActionButton();

            if (isOpenTime)
            {
                Label_SessionType.Text = "OPEN TIME SESSION";
                Label_TotalHours.Text = "Purchased: Pay-as-you-go";
                Label_TimeoutDisplay.Text = "Timeout: Continuous";
            }
            else if (isAdmin)
            {
                Label_SessionType.Text = "ADMINISTRATOR MODE";
                Label_TotalHours.Text = "Purchased: Unlimited";
                Label_TimeoutDisplay.Text = "Timeout: --:--";
                label_TotalAmount.Text = "Total Amount: --";
            }
            else
            {
                Label_SessionType.Text = "PREPAID SESSION";
                Label_TotalHours.Text = $"Purchased: {DisplayFormatter.FormatPurchasedTime((endTime - createdTime).TotalSeconds)}";
                Label_TimeoutDisplay.Text = DisplayFormatter.FormatTimeoutDisplay(endTime);
                label_TotalAmount.Text = $"Total Amount: ₱ {amount:N2}";
            }

            UpdateDisplay();
            Button_Logout.Enabled = true;

            if (!isPaused && !Timer_Countdown.Enabled)
            {
                Timer_Countdown.Start();
            }
        }

        private async Task<string> ResolveDisplayNameAsync(string id)
        {
            if (isAdmin) return "Administrator";
            if (string.IsNullOrWhiteSpace(id)) return "Guest / Walk-in";

            if (int.TryParse(id, out _))
            {
                try
                {
                    var userHandler = new GetUserHandler();
                    var data = await userHandler.GetByIdAsync(id);
                    if (data != null && data.Success && !string.IsNullOrWhiteSpace(data.Data?.Name))
                    {
                        return data.Data.Name;
                    }
                }
                catch { }
            }

            return id;
        }
        private void Timer_Countdown_Tick(object sender, EventArgs e)
        {
            if (isPaused) return;

            DateTime now = TimeService.Get();
            if (isOpenTime)
            {
                remainingSeconds = (now - createdTime).TotalSeconds;
                UpdateDisplay();
            }
            else
            {
                remainingSeconds--;

                if (remainingSeconds > 0)
                {
                    UpdateDisplay();
                }
                else
                {
                    ClearSessionFile();
                }
                // Inside Timer_Countdown_Tick:
                if (!isOpenTime && !isAdmin)
                {
                    int currentSecondsLeft = (int)Math.Max(0, remainingSeconds);

                    // 5-Minute Warning (Trigger between 300 and 299 seconds)
                    if (currentSecondsLeft <= 300 && currentSecondsLeft > 240 && !hasWarned5Min)
                    {
                        hasWarned5Min = true;
                        NotificationForm.Show("Time Warning", "You have 5 minutes remaining in your session.", NotificationType.Warning, 5000);
                    }

                    // 1-Minute Warning (Trigger between 60 and 59 seconds)
                    if (currentSecondsLeft <= 60 && currentSecondsLeft > 0 && !hasWarned1Min)
                    {
                        hasWarned1Min = true;
                        NotificationForm.Show("Session Expiring Soon", "You have 1 minute remaining! Please add time to continue.", NotificationType.Error, 5000);
                    }
                }
            }
        }

        private void UpdateDisplay()
        {
            if (isPaused)
            {
                Label_TimerDisplay.Text = "PAUSED";
                Label_TimerDisplay.ForeColor = Color.FromArgb(217, 119, 6);
                return;
            }

            Label_TimerDisplay.ForeColor = Color.FromArgb(67, 56, 202);
            TimeSpan time = TimeSpan.FromSeconds(Math.Max(0, remainingSeconds));
            Label_TimerDisplay.Text = isAdmin ? "Unlimited" : time.ToString(@"hh\:mm\:ss");

            if (isOpenTime)
            {
                double amount = CalculateRentalPrice.CalculatePrice((int)time.TotalMinutes);
                label_TotalAmount.Text = $"Total Amount: ₱ {amount:N2}";
                Button_Logout.Enabled = false;
            }
            else
            {
                Label_TimeoutDisplay.Text = isAdmin ? "Timeout: --:--" : DisplayFormatter.FormatTimeoutDisplay(TimeService.Get().AddSeconds(remainingSeconds));
                Button_Logout.Enabled = true;
            }
        }

        public void PauseTimer()
        {
            double amount = 0;
            if (isPaused) return;

            isPaused = true;
            Timer_Countdown.Stop();

            // Store the exact elapsed seconds at the moment of pausing
            DateTime now = TimeService.Get();
            if (isOpenTime)
            {
                remainingSeconds = (now - createdTime).TotalSeconds;
                TimeSpan time = TimeSpan.FromSeconds(Math.Max(0, remainingSeconds));
                amount = CalculateRentalPrice.CalculatePrice((int)time.TotalMinutes);
            }
            else
            {
                var session = SessionLogin.ReadSession();
                amount = session.Amount;
            }

            SessionLogin.SaveSession(
                createdTime,
                endTime,
                userId,
                amount,
                isAdmin,
                isOpenTime,
                isPaused: true,
                remainingSeconds: remainingSeconds
            );

            UpdateDisplay();
        }

        public void ResumeTimer()
        {
            double amount = 0;
            if (!isPaused) return;

            isPaused = false;
            DateTime now = TimeService.Get();

            if (isOpenTime)
            {
                // Re-anchor createdTime so (now - createdTime) equals remainingSeconds
                createdTime = now.AddSeconds(-remainingSeconds);
                TimeSpan time = TimeSpan.FromSeconds(Math.Max(0, remainingSeconds));
                amount = CalculateRentalPrice.CalculatePrice((int)time.TotalMinutes);
            }
            else
            {
                // For Prepaid sessions: end time shifts relative to right now
                endTime = now.AddSeconds(remainingSeconds);
                var session = SessionLogin.ReadSession();
                amount = session.Amount;
            }

            SessionLogin.SaveSession(
                createdTime,
                endTime,
                userId,
                amount,
                isAdmin,
                isOpenTime,
                isPaused: false,
                remainingSeconds: remainingSeconds
            );

            UpdateDisplay();
            Timer_Countdown.Start();
        }

        private async Task ResumeExistingSessionAsync()
        {
            var session = SessionLogin.ReadSession();
            if (session == null)
            {
                ClearSessionFile();
                return;
            }

            createdTime = session.CreatedTime;
            userId = session.UserId;
            isOpenTime = session.IsOpenTime;
            isAdmin = session.isAdmin;
            isPaused = session.IsPaused;

            DateTime now = TimeService.Get();

            if (isPaused)
            {
                // Load stored remaining elapsed seconds from disk
                remainingSeconds = session.RemainingSeconds;
                endTime = session.EndTime;
            }
            else
            {
                endTime = session.EndTime;
                if (isOpenTime)
                {
                    // For OpenTime, calculate elapsed seconds based on anchor createdTime
                    remainingSeconds = Math.Max(0, (now - createdTime).TotalSeconds);
                }
                else
                {
                    // For Prepaid, calculate remaining seconds left until endTime
                    remainingSeconds = Math.Max(0, (endTime - now).TotalSeconds);
                }
            }

            if (isAdmin || isOpenTime || remainingSeconds > 0)
            {
                Label_ClientName.Text = ConfigHelper.GetClientNameFromIP();
                Label_CustomerName.Text = await ResolveDisplayNameAsync(userId);
                Label_StartTime.Text = $"Started At: {createdTime:hh:mm tt}";

                UpdateAccountActionButton();

                if (isAdmin)
                {
                    Label_SessionType.Text = "ADMINISTRATOR MODE";
                    Label_TotalHours.Text = "Purchased: Unlimited";
                    Label_TimeoutDisplay.Text = "Timeout: --:--";
                    label_TotalAmount.Text = "Total Amount: --";
                }
                else if (isOpenTime)
                {
                    Label_SessionType.Text = "OPEN TIME SESSION";
                    Label_TotalHours.Text = "Purchased: Pay-as-you-go";
                    Label_TimeoutDisplay.Text = "Timeout: Continuous";
                    label_TotalAmount.Text = $"Total Amount: ₱ {session.Amount:N2}";
                }
                else
                {
                    Label_SessionType.Text = "PREPAID SESSION";
                    double totalPurchasedSeconds = remainingSeconds + (now - createdTime).TotalSeconds;
                    if (isPaused)
                    {
                        totalPurchasedSeconds = (session.EndTime - session.CreatedTime).TotalSeconds;
                    }

                    Label_TotalHours.Text = $"Purchased: {DisplayFormatter.FormatPurchasedTime(totalPurchasedSeconds)}";
                    label_TotalAmount.Text = $"Total Amount: ₱ {session.Amount:N2}";
                }

                if (isPaused)
                {
                    Timer_Countdown.Stop();
                    UpdateDisplay();
                }
                else
                {
                    UpdateDisplay();
                    Timer_Countdown.Start();
                }

                KeyboardHook.Stop();
                return;
            }

            ClearSessionFile();
        }

        private void ClearSessionFile()
        {
            isOpenTime = false;
            Button_AccountAction.Visible = false;
            if (SessionLogin.Exists())
            {
                userId = string.Empty;
                isRunning = false;
                Timer_Countdown.Stop();
                remainingSeconds = 0;
                this.Hide();
                SessionLogin.ClearSession();
            }
        }

        private void BNetCafeTimer_Load(object sender, EventArgs e)
        {
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(workingArea.Right - this.Width - 15, workingArea.Top + 15);
            Timer_Countdown.Interval = 1000;
        }

        private void BNetCafeTimer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
        }

        private void Button_Logout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to log out?", "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Logout();
            }
        }

        private void Button_AccountAction_Click(object sender, EventArgs e)
        {
            if (IsMemberUser())
            {
                string baseUrl = ConfigHelper.AppUrl?.TrimEnd('/');
                string handlerUrl = $"{baseUrl}/ResetPassword.aspx?UserId={SecuredDataService.Encrypted(userId)}";

                if (!string.IsNullOrEmpty(handlerUrl))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = handlerUrl,
                        UseShellExecute = true
                    });
                }
            }
            else if (IsWalkInUser())
            {
                this.Hide();

                using (RegisterForm registerForm = new RegisterForm())
                {
                    registerForm.ShowDialog();
                }

                // Restore and force layout recalculation
                this.Show();
                this.PerformLayout();
                this.Refresh();
            }
        }
        private void Button_Print_Click(object sender, EventArgs e)
        {
            try
            {
                string ftpPath = ConfigHelper.FtpServerPath; // Returns e.g. "ftp://192.168.1.2/"

                if (string.IsNullOrEmpty(ftpPath))
                {
                    MessageBox.Show("FTP Server path is not configured.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string currentPcName = ConfigHelper.GetClientNameFromIP();

                // Ensure the path ends with a slash before appending the client directory name
                if (!ftpPath.EndsWith("/"))
                {
                    ftpPath += "/";
                }

                string fullFtpFolderPath = $"{ftpPath}{currentPcName}";

                // Attempt to create the directory via FTP if it does not already exist
                try
                {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(fullFtpFolderPath);
                    request.Method = WebRequestMethods.Ftp.MakeDirectory;
                    // request.Credentials = new NetworkCredential("username", "password");

                    using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                    {
                        // Directory created successfully
                    }
                }
                catch (WebException ex)
                {
                    FtpWebResponse response = (FtpWebResponse)ex.Response;
                    // Ignore status code 550 (ActionNotTakenFileUnavailable) as it indicates the folder already exists
                    if (response != null && response.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        // Optionally log other specific FTP response status codes here
                    }
                }

                // Inform the user where to place their files
                MessageBox.Show(
                    $"Please place the files for printing into this folder: {currentPcName}",
                    "Notice",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                // Launch Windows File Explorer directly to the FTP directory path
                System.Diagnostics.Process.Start("explorer.exe", fullFtpFolderPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open printing folder: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Button_Shop_Click(object sender, EventArgs e)
        {
            // TODO: Open Cafe Shop / Snack Ordering Form
        }

        private void Button_History_Click(object sender, EventArgs e)
        {
            string baseUrl = ConfigHelper.AppUrl?.TrimEnd('/');
            string handlerUrl = $"{baseUrl}/Portal.aspx?UserId={SecuredDataService.Encrypted(userId)}";

            if (!string.IsNullOrEmpty(handlerUrl))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = handlerUrl,
                    UseShellExecute = true
                });
            }
        }
    }
}