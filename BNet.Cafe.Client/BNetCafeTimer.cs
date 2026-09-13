using BNet.Cafe.Client.Ashx;
using BNet.Cafe.Client.Repositories;
using BNet.Cafe.Client.Services;
using System;
using System.Configuration;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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

        public bool isAdministrator = false;
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
                && userId != "Administrator"
                && !isAdministrator
                && Label_CustomerName.Text != "Guest / Walk-in";
        }

        private bool IsWalkInUser()
        {
            return (!isAdministrator && userId != "Administrator") &&
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
            isAdministrator = (userId == "Administrator" && parsedDurationMinutes == 6000);
            isPaused = false;
            endTime = isOpenTime ? DateTime.MaxValue : createdTime.AddMinutes(parsedDurationMinutes);

            await ApplyTimerDataAsync(parsedAmount);
        }

        public async Task UpdateTimerDataAsync(string newDuration, string newAmount)
        {
            double.TryParse(newDuration, out double parsedDurationMinutes);
            double.TryParse(newAmount, out double parsedAmount);

            isOpenTime = (parsedDurationMinutes == 0);
            endTime = isOpenTime ? DateTime.MaxValue : createdTime.AddMinutes(parsedDurationMinutes);

            await ApplyTimerDataAsync(parsedAmount);
        }

        public void Logout()
        {
            if (!Label_CustomerName.Text.Contains("Guest / Walk-in") && !string.IsNullOrEmpty(userId))
            {
                TimeSpan time = TimeSpan.FromSeconds(remainingSeconds);
                string totalMinutes = ((int)time.TotalMinutes).ToString();

                PendingLogoutManager.SavePendingLogout(userId, totalMinutes, "0", "LOGGING-OUT");

                _ = Task.Run(async () =>
                {
                    await PendingLogoutManager.ProcessPendingLogoutAsync();
                });
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

            SessionManager.SaveSession(createdTime, endTime, userId, amount, isOpenTime, isPaused);

            Label_ClientName.Text =  ConfigHelper.GetClientNameFromIP();
            Label_CustomerName.Text = await ResolveDisplayNameAsync(userId);
            Label_StartTime.Text = $"Started At: {createdTime:hh:mm tt}";

            UpdateAccountActionButton();

            if (isOpenTime)
            {
                Label_SessionType.Text = "OPEN TIME SESSION";
                Label_TotalHours.Text = "Purchased: Pay-as-you-go";
                Label_TimeoutDisplay.Text = "Timeout: Continuous";
            }
            else if (isAdministrator)
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
            if (string.IsNullOrWhiteSpace(id)) return "Guest / Walk-in";
            if (id == "Administrator") return id;

            if (int.TryParse(id, out _))
            {
                try
                {
                    var userHandler = new UserHandler();
                    var data = await userHandler.UserAsync(id);
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
            }
        }

        private void UpdateDisplay()
        {
            if (isPaused)
            {
                Label_TimerDisplay.Text = "PAUSED";
                Label_TimerDisplay.ForeColor = Color.FromArgb(217, 119, 6);
                Label_TimeoutDisplay.Text = "Timeout: PAUSED";
                Label_SessionType.Text = "SESSION PAUSED";
                return;
            }

            Label_TimerDisplay.ForeColor = Color.FromArgb(67, 56, 202);
            TimeSpan time = TimeSpan.FromSeconds(Math.Max(0, remainingSeconds));
            Label_TimerDisplay.Text = isAdministrator ? "Unlimited" : time.ToString(@"hh\:mm\:ss");

            if (isOpenTime)
            {
                double amount = CalculateRentalPrice.CalculatePrice((int)time.TotalMinutes);
                label_TotalAmount.Text = $"Total Amount: ₱ {amount:N2}";
                Button_Logout.Enabled = false;
            }
            else
            {
                Label_TimeoutDisplay.Text = isAdministrator ? "Timeout: --:--" : DisplayFormatter.FormatTimeoutDisplay(TimeService.Get().AddSeconds(remainingSeconds));
                Button_Logout.Enabled = true;
            }
        }

        public void PauseTimer()
        {
            if (isPaused) return;

            isPaused = true;
            Timer_Countdown.Stop();

            SessionManager.SaveSession(createdTime, endTime, userId, 0, isOpenTime, isPaused: true, remainingSeconds: remainingSeconds);
            UpdateDisplay();
        }

        public void ResumeTimer()
        {
            if (!isPaused) return;

            isPaused = false;

            DateTime now = TimeService.Get();
            endTime = now.AddSeconds(remainingSeconds);

            SessionManager.SaveSession(createdTime, endTime, userId, 0, isOpenTime, isPaused: false, remainingSeconds: remainingSeconds);

            UpdateDisplay();
            Timer_Countdown.Start();
        }

        private async Task ResumeExistingSessionAsync()
        {
            var session = SessionManager.ReadSession();
            if (session == null)
            {
                ClearSessionFile();
                return;
            }

            createdTime = session.CreatedTime;
            userId = session.UserId;
            isOpenTime = session.IsOpenTime;
            isAdministrator = session.IsAdministrator;
            isPaused = session.IsPaused;

            DateTime now = TimeService.Get();

            if (isPaused)
            {
                remainingSeconds = session.RemainingSeconds;
                endTime = session.EndTime;
            }
            else
            {
                endTime = session.EndTime;
                remainingSeconds = isOpenTime ? (now - createdTime).TotalSeconds : (endTime - now).TotalSeconds;
            }

            if (isAdministrator || isOpenTime || remainingSeconds > 0)
            {
                Label_ClientName.Text = ConfigHelper.GetClientNameFromIP();
                Label_CustomerName.Text = await ResolveDisplayNameAsync(userId);
                Label_StartTime.Text = $"Started At: {createdTime:hh:mm tt}";

                UpdateAccountActionButton();

                if (isAdministrator)
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
            if (SessionManager.Exists())
            {
                userId = string.Empty;
                isRunning = false;
                Timer_Countdown.Stop();
                remainingSeconds = 0;
                this.Hide();
                SessionManager.ClearSession();
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
                var resetForm = new ResetPasswordForm();
                resetForm.Show();
                resetForm.ResetPassword(userId);
            }
            else if (IsWalkInUser())
            {
                var oauthForm = new OAuthLoginForm();
                oauthForm.Show();
            }
        }
    }
}