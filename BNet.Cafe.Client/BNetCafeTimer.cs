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

            // 2. Capture creation time when creating a new session
            createdTime = TimeService.Get();
            endTime = createdTime.AddMinutes(parsedDurationMinutes);

            ApplyTimerData(customerName, parsedAmount);
        }

        /// <summary>
        /// Extends/top-ups an existing timer session with additional duration and amount.
        /// </summary>
        public void UpdateTimerData(string additionalDuration, string additionalAmount)
        {
            double.TryParse(additionalDuration, out double parsedDurationMinutes);
            double.TryParse(additionalAmount, out double parsedAmount);

            endTime = endTime.AddMinutes(parsedDurationMinutes);

            string currentCustomerName = Label_CustomerName.Text.Replace("User : ", "");

            ApplyTimerData(currentCustomerName, parsedAmount);
        }

        private void ApplyTimerData(string customerName, double amount)
        {
            remainingSeconds = (endTime - TimeService.Get()).TotalSeconds;

            SaveSessionToFile(customerName, amount);

            Label_ClientName.Text = ConfigurationManager.AppSettings["ClientName"]?.ToUpper().Replace(" ", "") ?? "CLIENT";
            Label_CustomerName.Text = $"User : {(string.IsNullOrWhiteSpace(customerName) ? "GUEST" : customerName.ToUpper())}";
            Label_TotalHours.Text = $"Purchased : {FormatPurchasedTime(remainingSeconds)}";
            label_TotalAmount.Text = $"Amount : ₱{amount:N2}";

            Label_TimeoutDisplay.Text = $"Timeout : {endTime:hh:mm tt}";

            UpdateDisplay();
            Button_Logout.Enabled = true;

            if (!Timer_Countdown.Enabled)
            {
                Timer_Countdown.Start();
            }
        }

        private void Timer_Countdown_Tick(object sender, EventArgs e)
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
                // 3. Saved CreatedTime Ticks into file format: CreatedTimeTicks|TargetEndTimeTicks|CustomerName|Amount
                string content = $"{createdTime.Ticks}|{endTime.Ticks}|{customerName}|{amount}";
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

                    // 4. Added support to read 4 pipe-separated values
                    if (parts.Length >= 4 &&
                        long.TryParse(parts[0], out long createdTicks) &&
                        long.TryParse(parts[1], out long endTicks))
                    {
                        createdTime = new DateTime(createdTicks);
                        endTime = new DateTime(endTicks);
                        string customerName = parts[2];
                        double.TryParse(parts[3], out double amount);

                        remainingSeconds = (endTime - TimeService.Get()).TotalSeconds;

                        if (remainingSeconds > 0)
                        {
                            Label_ClientName.Text = ConfigurationManager.AppSettings["ClientName"]?.ToUpper().Replace(" ", "") ?? "CLIENT";
                            Label_CustomerName.Text = $"User : {(string.IsNullOrWhiteSpace(customerName) ? "GUEST" : customerName.ToUpper())}";
                            Label_TotalHours.Text = $"Purchased : {FormatPurchasedTime(remainingSeconds)}";
                            label_TotalAmount.Text = $"Amount : ₱{amount:N2}";
                            Label_TimeoutDisplay.Text = $"Timeout : {endTime:hh:mm tt}";

                            UpdateDisplay();
                            Timer_Countdown.Start();
                            return;
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
            if (File.Exists(sessionFilePath))
            {
                MainForm._timeStart = "";
                MainForm._timeEnd = "";
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

            if (time.TotalHours < 1)
            {
                return $"{time.Minutes} mins";
            }

            return time.Minutes > 0
                ? $"{time.Hours} hr {time.Minutes} mins"
                : $"{time.Hours} hrs";
        }
    }
}