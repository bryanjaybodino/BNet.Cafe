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
        private DateTime endTime;
        private readonly string sessionFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "session.txt");


        // Constructor to RESUME session on application restart
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

            // Set end time relative to current time
            endTime = DateTime.Now.AddMinutes(parsedDurationMinutes);

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

            // Extract current customer name from existing label if available
            string currentCustomerName = Label_CustomerName.Text.Replace("User : ", "");

            ApplyTimerData(currentCustomerName, parsedAmount);
        }

        /// <summary>
        /// Helper method to recalculate remaining time, update UI elements, save session state, and ensure countdown timer runs.
        /// </summary>
        private void ApplyTimerData(string customerName, double amount)
        {
            // Calculate remaining seconds based on computed end time
            remainingSeconds = (endTime - DateTime.Now).TotalSeconds;

            // Save state to text file
            SaveSessionToFile(customerName, amount);

            // Update UI Labels
            Label_ClientName.Text = ConfigurationManager.AppSettings["ClientName"]?.ToUpper().Replace(" ", "") ?? "CLIENT";
            Label_CustomerName.Text = $"User : {(string.IsNullOrWhiteSpace(customerName) ? "GUEST" : customerName.ToUpper())}";
            Label_TotalHours.Text = $"Purchased : {FormatPurchasedTime(remainingSeconds)}";
            label_TotalAmount.Text = $"Amount : ₱{amount:N2}";

            // Displays exact timeout time (e.g., "05:30 PM")
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
            // Compute live remaining seconds relative to actual system time
            remainingSeconds = (endTime - DateTime.Now).TotalSeconds;

            if (remainingSeconds > 0)
            {
                UpdateDisplay();
            }
            else
            {
                Timer_Countdown.Stop();
                remainingSeconds = 0;
                UpdateDisplay();
                Button_Logout.Enabled = false;

                ClearSessionFile(); // Session ended, delete text file
                MessageBox.Show("Your time has expired!", "Session Ended", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                // File format: TargetEndTimeTicks|CustomerName|Amount
                string content = $"{endTime.Ticks}|{customerName}|{amount}";
                File.WriteAllText(sessionFilePath, content);
            }
            catch (Exception ex)
            {
                // Handle file writing errors safely
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

                    if (parts.Length >= 3 && long.TryParse(parts[0], out long ticks))
                    {
                        endTime = new DateTime(ticks);
                        string customerName = parts[1];
                        double.TryParse(parts[2], out double amount);

                        remainingSeconds = (endTime - DateTime.Now).TotalSeconds;

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
                Timer_Countdown.Stop();
                ClearSessionFile();
                remainingSeconds = 0;
                this.Hide();
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