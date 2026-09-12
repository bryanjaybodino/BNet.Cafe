using BNet.Cafe.Client.Ashx;
using BNet.Cafe.Client.Repositories;
using BNet.Cafe.Client.Services;
using System;
using System.Configuration;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BNet.Cafe.Client
{
    public partial class BNetCafeTimer : Form
    {
        private double remainingSeconds = 0;
        private DateTime createdTime;
        private DateTime endTime;
        private bool isOpenTime = false;

        public bool isAdministrator = false;
        public bool isRunning = false;
        public string userId = string.Empty;

        public BNetCafeTimer()
        {
            InitializeComponent();
            ConfigureFormStyle();
            _ = ResumeExistingSessionAsync();
        }

        private void ConfigureFormStyle()
        {
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.TopMost = false;
            this.ShowInTaskbar = false;
        }

        public async Task CreateTimerDataAsync(string serverTime, string duration, string amount)
        {
            double.TryParse(duration, out double parsedDurationMinutes);
            double.TryParse(amount, out double parsedAmount);

            createdTime = Convert.ToDateTime(serverTime);
            isOpenTime = (parsedDurationMinutes == 0);
            isAdministrator = (userId == "Administrator" && parsedDurationMinutes == 6000);

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

        public async void Logout()
        {
            if (!Label_CustomerName.Text.Contains("Guest / Walk-in"))
            {
                var createBalanceHandler = new CreateBalanceHandler();
                TimeSpan time = TimeSpan.FromSeconds(remainingSeconds);
                await createBalanceHandler.CreateBalanceAsync(userId, ((int)time.TotalMinutes).ToString(), "0", "LOGGING-OUT");
            }

            isOpenTime = false;
            endTime = createdTime.AddMinutes(0);
            await ApplyTimerDataAsync(0);
            ClearSessionFile();
        }

        private async Task ApplyTimerDataAsync(double amount)
        {
            DateTime now = TimeService.Get();
            remainingSeconds = isOpenTime ? (now - createdTime).TotalSeconds : (endTime - now).TotalSeconds;

            SessionManager.SaveSession(createdTime, endTime, userId, amount, isOpenTime);

            Label_ClientName.Text = ConfigurationManager.AppSettings["ClientName"]?.ToUpper().Replace(" ", "") ?? "CLIENT";

            // Resolve user display name (Fixes issue #1)
            Label_CustomerName.Text = await ResolveDisplayNameAsync(userId);

            if (isOpenTime)
            {
                Label_TotalHours.Text = "Purchased : ∞";
                Label_TimeoutDisplay.Text = "Timeout : ∞";
            }
            else if (isAdministrator)
            {
                Label_TotalHours.Text = "Purchased : --";
                Label_TimeoutDisplay.Text = "Timeout : --";
                label_TotalAmount.Text = "Amount : --";
            }
            else
            {
                Label_TotalHours.Text = $"Purchased : {DisplayFormatter.FormatPurchasedTime((endTime - createdTime).TotalSeconds)}";
                Label_TimeoutDisplay.Text = DisplayFormatter.FormatTimeoutDisplay(endTime);
                label_TotalAmount.Text = $"Amount : ₱{amount:N2}";
            }

            UpdateDisplay();
            Button_Logout.Enabled = true;

            if (!Timer_Countdown.Enabled)
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
            DateTime now = TimeService.Get();
            if (isOpenTime)
            {
                remainingSeconds = (now - createdTime).TotalSeconds;
                UpdateDisplay();
            }
            else
            {
                remainingSeconds = (endTime - now).TotalSeconds;
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
            TimeSpan time = TimeSpan.FromSeconds(Math.Max(0, remainingSeconds));
            Label_TimerDisplay.Text = isAdministrator ? "Unlimited" : time.ToString(@"hh\:mm\:ss");

            if (isOpenTime)
            {
                double amount = CalculateRentalPrice.CalculatePrice((int)time.TotalMinutes);
                label_TotalAmount.Text = $"Amount : ₱ {amount:N2}";
                Button_Logout.Enabled = false;
            }
            else
            {
                Button_Logout.Enabled = true;
            }
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
            endTime = session.EndTime;
            userId = session.UserId;
            isOpenTime = session.IsOpenTime;
            isAdministrator = session.IsAdministrator;

            DateTime now = TimeService.Get();
            remainingSeconds = isOpenTime ? (now - createdTime).TotalSeconds : (endTime - now).TotalSeconds;

            if (isAdministrator || isOpenTime || remainingSeconds > 0)
            {
                Label_ClientName.Text = ConfigurationManager.AppSettings["ClientName"]?.ToUpper().Replace(" ", "") ?? "CLIENT";
                Label_CustomerName.Text = await ResolveDisplayNameAsync(userId);

                if (isAdministrator)
                {
                    Label_TotalHours.Text = "Purchased : --";
                    Label_TimeoutDisplay.Text = "Timeout : --";
                    label_TotalAmount.Text = "Amount : --";
                }
                else if (isOpenTime)
                {
                    Label_TotalHours.Text = "Purchased : ∞";
                    Label_TimeoutDisplay.Text = "Timeout : ∞";
                }
                else
                {
                    Label_TotalHours.Text = $"Purchased : {DisplayFormatter.FormatPurchasedTime((endTime - createdTime).TotalSeconds)}";
                    Label_TimeoutDisplay.Text = DisplayFormatter.FormatTimeoutDisplay(endTime);
                    label_TotalAmount.Text = $"Amount : ₱ {session.Amount:N2}";
                }

                UpdateDisplay();
                Timer_Countdown.Start();
                KeyboardHook.Stop();
                return;
            }

            ClearSessionFile();
        }

        private void ClearSessionFile()
        {
            isOpenTime = false;
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
    }
}