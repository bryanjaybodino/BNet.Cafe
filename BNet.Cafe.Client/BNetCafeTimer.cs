using BNet.Cafe.Client.Repositories;
using System;
using System.Configuration;
using System.Drawing;
using System.IO;
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
        public void CreateTimerData(string customerName, string duration, string amount)
        {
            double.TryParse(duration, out double parsedDurationMinutes);
            double.TryParse(amount, out double parsedAmount);

            createdTime = TimeService.Get();

            // Check if open time (duration is 0)
            isOpenTime = (parsedDurationMinutes == 0);

            if (isOpenTime)
            {
                endTime = DateTime.MaxValue; // Set no end time limit
            }
            else
            {
                endTime = createdTime.AddMinutes(parsedDurationMinutes);
            }

            ApplyTimerData(customerName, parsedAmount);
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

            string currentCustomerName = Label_CustomerName.Text.Replace("User : ", "");
            ApplyTimerData(currentCustomerName, parsedAmount);
        }

        public void Logout()
        {
            isOpenTime = false;
            endTime = createdTime.AddMinutes(0);
            ApplyTimerData("", 0);
            ClearSessionFile();
        }


        private void ApplyTimerData(string customerName, double amount)
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

            SaveSessionToFile(customerName, amount);

            Label_ClientName.Text = ConfigurationManager.AppSettings["ClientName"]?.ToUpper().Replace(" ", "") ?? "CLIENT";
            Label_CustomerName.Text = $"User : {(string.IsNullOrWhiteSpace(customerName) ? "GUEST" : customerName.ToUpper())}";

            if (isOpenTime)
            {
                Label_TotalHours.Text = "Purchased : OPEN TIME";
                Label_TimeoutDisplay.Text = "Timeout : OPEN TIME";
            }
            else
            {
                Label_TotalHours.Text = $"Purchased : {FormatPurchasedTime((endTime - createdTime).TotalSeconds)}";
                Label_TimeoutDisplay.Text = $"Timeout : {endTime:hh:mm tt}";
            }

            label_TotalAmount.Text = $"Amount : ₱{amount:N2}";

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
        }

        #region Session Persistence (Text File)

        private void SaveSessionToFile(string customerName, double amount)
        {
            try
            {
                // Updated file format: CreatedTimeTicks|TargetEndTimeTicks|CustomerName|Amount|IsOpenTime
                string content = $"{createdTime.Ticks}|{endTime.Ticks}|{customerName}|{amount}|{isOpenTime}";
                File.WriteAllText(sessionFilePath, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving session: {ex.Message}");
            }
        }

        private void ResumeExistingSession()
        {
            if (File.Exists(sessionFilePath))
            {
                try
                {
                    string content = File.ReadAllText(sessionFilePath);
                    string[] parts = content.Split('|');

                    if (parts.Length >= 4 &&
                        long.TryParse(parts[0], out long createdTicks) &&
                        long.TryParse(parts[1], out long endTicks))
                    {
                        createdTime = new DateTime(createdTicks);
                        endTime = new DateTime(endTicks);
                        string customerName = parts[2];
                        double.TryParse(parts[3], out double amount);

                        // Load IsOpenTime flag if available (backwards compatible)
                        if (parts.Length >= 5 && bool.TryParse(parts[4], out bool parsedIsOpenTime))
                        {
                            isOpenTime = parsedIsOpenTime;
                        }
                        else
                        {
                            isOpenTime = false;
                        }

                        if (isOpenTime)
                        {
                            remainingSeconds = (TimeService.Get() - createdTime).TotalSeconds;
                            Label_ClientName.Text = ConfigurationManager.AppSettings["ClientName"]?.ToUpper().Replace(" ", "") ?? "CLIENT";
                            Label_CustomerName.Text = $"User : {(string.IsNullOrWhiteSpace(customerName) ? "GUEST" : customerName.ToUpper())}";
                            Label_TotalHours.Text = "Purchased : OPEN TIME";
                            label_TotalAmount.Text = $"Amount : ₱ {amount:N2}";
                            Label_TimeoutDisplay.Text = "Timeout : OPEN TIME";

                            UpdateDisplay();
                            Timer_Countdown.Start();
                            KeyboardHook.Stop();
                            return;
                        }
                        else
                        {
                            remainingSeconds = (endTime - TimeService.Get()).TotalSeconds;

                            if (remainingSeconds > 0)
                            {
                                double totalPurchasedSeconds = (endTime - createdTime).TotalSeconds;

                                Label_ClientName.Text = ConfigurationManager.AppSettings["ClientName"]?.ToUpper().Replace(" ", "") ?? "CLIENT";
                                Label_CustomerName.Text = $"User : {(string.IsNullOrWhiteSpace(customerName) ? "GUEST" : customerName.ToUpper())}";
                                Label_TotalHours.Text = $"Purchased : {FormatPurchasedTime(totalPurchasedSeconds)}";
                                label_TotalAmount.Text = $"Amount : ₱ {amount:N2}";
                                Label_TimeoutDisplay.Text = FormatTimeoutDisplay(endTime);

                                UpdateDisplay();
                                Timer_Countdown.Start();
                                KeyboardHook.Stop();
                                return;
                            }
                        }
                    }
                }
                catch
                {
                    // If reading fails, clear corrupted session
                }
            }

            ClearSessionFile();
        }

        private void ClearSessionFile()
        {
            isOpenTime = false;
            if (File.Exists(sessionFilePath))
            {
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
                ClearSessionFile();
            }
        }

        private string FormatPurchasedTime(double totalSeconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(Math.Max(0, totalSeconds));

            int totalHours = (int)time.TotalHours;
            int minutes = time.Minutes;

            string hourText = totalHours == 1 ? "1 hr" : $"{totalHours} hrs";
            string minText = minutes == 1 ? "1 min" : $"{minutes} mins";

            if (totalHours < 1)
            {
                return minText;
            }

            return minutes > 0
                ? $"{hourText} and {minText}"
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