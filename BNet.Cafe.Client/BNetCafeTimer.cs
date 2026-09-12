using BNet.Cafe.Client.Ashx;
using BNet.Cafe.Client.Repositories;
using BNet.Cafe.Client.Services;
using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BNet.Cafe.Client
{
    public partial class BNetCafeTimer : Form
    {
        private double remainingSeconds = 0;
        private DateTime createdTime; // 1. Added createdTime field
        private DateTime endTime;
        private bool isOpenTime = false; // Track open time mode
        private readonly string sessionFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "session.txt");
        public bool isAdministrator = false;
        public bool isRunning = false;
        public string userId = string.Empty;

        public BNetCafeTimer()
        {
            InitializeComponent();
            ConfigureFormStyle();
            ResumeExistingSession();
        }

        private void ConfigureFormStyle()
        {
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.TopMost = false;
            this.ShowInTaskbar = false;
        }

        /// <summary>
        /// Initializes a brand-new timer session for a customer.
        /// </summary>
        public void CreateTimerData(string serverTime, string duration, string amount)
        {
            double.TryParse(duration, out double parsedDurationMinutes);
            double.TryParse(amount, out double parsedAmount);

            createdTime = Convert.ToDateTime(serverTime);

            // Check if open time (duration is 0)
            isOpenTime = (parsedDurationMinutes == 0);
            isAdministrator = (userId == "Administrator" && parsedDurationMinutes == 6000);

            if (isOpenTime)
            {
                endTime = DateTime.MaxValue; // Set no end time limit
            }
            else
            {
                endTime = createdTime.AddMinutes(parsedDurationMinutes);
            }

            ApplyTimerData(parsedAmount);
        }

        /// <summary>
        /// Extends/top-ups an existing timer session with additional duration and amount.
        /// </summary>
        public void UpdateTimerData(string newDuration, string newAmount)
        {
            double.TryParse(newDuration, out double parsedDurationMinutes);
            double.TryParse(newAmount, out double parsedAmount);

            isOpenTime = (parsedDurationMinutes == 0);

            if (isOpenTime)
            {
                endTime = DateTime.MaxValue;
            }
            else
            {
                endTime = createdTime.AddMinutes(parsedDurationMinutes);
            }

            ApplyTimerData(parsedAmount);
        }

        public async void Logout()
        {
            if (!Label_CustomerName.Text.Contains("Guest / Walk-in"))
            {
                CreateBalanceHandler createBalanceHandler = new CreateBalanceHandler();
                TimeSpan time = TimeSpan.FromSeconds(remainingSeconds);
                int minutes = time.Minutes;       // Returns 46
                int totalMinutes = (int)time.TotalMinutes; // Returns 166
                await createBalanceHandler.CreateBalanceAsync(userId, totalMinutes.ToString(), "0", "LOGGING-OUT");
            }
            isOpenTime = false;
            endTime = createdTime.AddMinutes(0);
            ApplyTimerData(0);
            ClearSessionFile();
        }


        private void ApplyTimerData(double amount)
        {
            if (isOpenTime)
            {
                // Count elapsed time from creation
                remainingSeconds = (TimeService.Get() - createdTime).TotalSeconds;
            }
            else
            {
                // Count remaining time to end time
                remainingSeconds = (endTime - TimeService.Get()).TotalSeconds;
            }

            SaveSessionToFile(amount);

            Label_ClientName.Text = ConfigurationManager.AppSettings["ClientName"]?.ToUpper().Replace(" ", "") ?? "CLIENT";
            Label_CustomerName.Text = $"{(string.IsNullOrWhiteSpace(userId) ? "Guest / Walk-in" : userId)}";

            if (isOpenTime)
            {
                Label_TotalHours.Text = "Purchased : ∞";
                Label_TimeoutDisplay.Text = "Timeout : ∞";
            }
            else if (isAdministrator)
            {
                Label_TotalHours.Text = $"Purchased : --";
                Label_TimeoutDisplay.Text = $"Timeout : --";
                label_TotalAmount.Text = $"Amount : --";
            }
            else
            {
                Label_TotalHours.Text = $"Purchased : {FormatPurchasedTime((endTime - createdTime).TotalSeconds)}";
                Label_TimeoutDisplay.Text = $"Timeout : {endTime:hh:mm tt}";
                label_TotalAmount.Text = $"Amount : ₱{amount:N2}";
            }

            UpdateDisplay();
            Button_Logout.Enabled = true;

            if (!Timer_Countdown.Enabled)
            {
                Timer_Countdown.Start();
            }
        }

        private void Timer_Countdown_Tick(object sender, EventArgs e)
        {
            if (isOpenTime)
            {
                // Continuously count up elapsed time
                remainingSeconds = (TimeService.Get() - createdTime).TotalSeconds;
                UpdateDisplay();
            }
            else
            {
                remainingSeconds = (endTime - TimeService.Get()).TotalSeconds;

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
            Label_TimerDisplay.Text = time.ToString(@"hh\:mm\:ss");

            if (isOpenTime)
            {
                double amount = CalculateRentalPrice.CalculatePrice((int)time.TotalMinutes);
                label_TotalAmount.Text = $"Amount : ₱ {amount:N2}";

                if (Button_Logout.Enabled)
                {
                    Button_Logout.Enabled = false;
                }
            }
            else if (isAdministrator)
            {
                Label_TimerDisplay.Text = "Unlimited";
            }
            else
            {
                if (!Button_Logout.Enabled)
                {
                    Button_Logout.Enabled = true;
                }
            }
        }

        #region Session Persistence (Text File)

        private void SaveSessionToFile(double amount)
        {
            try
            {
                // Updated file format: CreatedTimeTicks|TargetEndTimeTicks|UserId|Amount|IsOpenTime
                string content = $"{createdTime.Ticks}|{endTime.Ticks}|{userId}|{amount}|{isOpenTime}";
                File.WriteAllText(sessionFilePath, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving session: {ex.Message}");
            }
        }
        private async void ResumeExistingSession()
        {
            if (!File.Exists(sessionFilePath)) return;

            try
            {
                string[] parts = File.ReadAllText(sessionFilePath).Split('|');

                if (parts.Length >= 4 &&
                    long.TryParse(parts[0], out long createdTicks) &&
                    long.TryParse(parts[1], out long endTicks))
                {
                    createdTime = new DateTime(createdTicks);
                    endTime = new DateTime(endTicks);

                    userId = parts[2];
                    double.TryParse(parts[3], out double amount);

                    isOpenTime = parts.Length >= 5 && bool.TryParse(parts[4], out bool parsed) && parsed;
                    isAdministrator = userId == "Administrator" && amount == 0;

                    string displayUser = string.IsNullOrWhiteSpace(userId) ? "Guest / Walk-in" : userId;
                    bool isNumber = int.TryParse(userId, out _);

                    if (isNumber)
                    {
                        UserHandler userHandler = new UserHandler();
                        var data = await userHandler.UserAsync(userId);
                        if (data.Success)
                        {
                            displayUser = data.Data.Name;
                        }
                    }

                    DateTime now = TimeService.Get();
                    remainingSeconds = isOpenTime ? (now - createdTime).TotalSeconds : (endTime - now).TotalSeconds;

                    // Only proceed if session is active (Admin, OpenTime, or remaining time > 0)
                    if (isAdministrator || isOpenTime || remainingSeconds > 0)
                    {

                        string clientName = ConfigurationManager.AppSettings["ClientName"]?.ToUpper().Replace(" ", "") ?? "CLIENT";

                        Label_ClientName.Text = clientName;
                        Label_CustomerName.Text = $"{displayUser}";

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
                            double totalPurchasedSeconds = (endTime - createdTime).TotalSeconds;
                            Label_TotalHours.Text = $"Purchased : {FormatPurchasedTime(totalPurchasedSeconds)}";
                            Label_TimeoutDisplay.Text = FormatTimeoutDisplay(endTime);
                            label_TotalAmount.Text = $"Amount : ₱ {amount:N2}";
                        }

                        UpdateDisplay();
                        Timer_Countdown.Start();
                        KeyboardHook.Stop();
                        return;
                    }
                }
            }
            catch
            {
                // Ignore read/parse errors and fall through to clear
            }

            ClearSessionFile();
        }
        private void ClearSessionFile()
        {
            isOpenTime = false;
            if (File.Exists(sessionFilePath))
            {
                userId = string.Empty;
                isRunning = false;
                Timer_Countdown.Stop();
                remainingSeconds = 0;
                this.Hide();
                try { File.Delete(sessionFilePath); } catch { }
            }
        }

        #endregion

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
            var result = MessageBox.Show("Are you sure you want to log out?", "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Logout();
            }
        }

        private string FormatPurchasedTime(double totalSeconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(Math.Max(0, totalSeconds));

            int totalHours = (int)time.TotalHours;
            int minutes = time.Minutes;

            string hourText = totalHours == 1 ? "1hr" : $"{totalHours}hrs";
            string minText = minutes == 1 ? "1min" : $"{minutes}mins";

            if (totalHours < 1)
            {
                return minText;
            }

            return minutes > 0
                ? $"{hourText} | {minText}"
                : hourText;
        }

        private string FormatTimeoutDisplay(DateTime targetEndTime)
        {
            // If the timeout is on a different day, append the date (e.g., "03:30 PM (Sep 12)")
            if (targetEndTime.Date > TimeService.Get().Date)
            {
                return $"Timeout : {targetEndTime:hh:mm tt (MMM dd)}";
            }

            return $"Timeout : {targetEndTime:hh:mm tt}";
        }
    }
}