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

        // Constructor for NEW transactions
        public BNetCafeTimer(string clientName, string duration, string amount)
        {
            InitializeComponent();
            ConfigureFormStyle();
            UpdateTimerData(clientName, duration, amount);
        }

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
            this.TopMost = true;
            this.ShowInTaskbar = false;
        }

        /// <summary>
        /// Updates or top-ups existing timer data and persists the new end time to disk.
        /// </summary>
        public void UpdateTimerData(string customerName, string duration, string amount)
        {
            double.TryParse(duration, out double parsedDurationMinutes);
            double.TryParse(amount, out double parsedAmount);

            // Add duration to target end time
            if (endTime < DateTime.Now)
            {
                endTime = DateTime.Now.AddMinutes(parsedDurationMinutes);
            }
            else
            {
                endTime = endTime.AddMinutes(parsedDurationMinutes);
            }

            // Calculate remaining seconds based on the computed end time
            remainingSeconds = (endTime - DateTime.Now).TotalSeconds;

            // Save state to text file
            SaveSessionToFile(customerName, parsedAmount);

            // Update UI Labels
            Label_ClientName.Text = ConfigurationManager.AppSettings["ClientName"]?.ToUpper().Replace(" ", "") ?? "CLIENT";
            Label_CustomerName.Text = $"User : {(string.IsNullOrWhiteSpace(customerName) ? "GUEST" : customerName.ToUpper())}";
            Label_TotalHours.Text = $"Purchased : {FormatPurchasedTime(remainingSeconds)}";
            label_TotalAmount.Text = $"Amount : ₱{parsedAmount:N2}";

            // Displays exact timeout time (e.g., "05:30 PM")
            Label_TimeoutDisplay.Text = $"Timeout : {endTime:hh:mm tt}";

            UpdateDisplay();
            Button_Logout.Enabled = true;

            if (!countdownTimer.Enabled)
            {
                countdownTimer.Start();
            }
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            // Compute live remaining seconds relative to actual system time
            remainingSeconds = (endTime - DateTime.Now).TotalSeconds;

            if (remainingSeconds > 0)
            {
                UpdateDisplay();
            }
            else
            {
                countdownTimer.Stop();
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
                            countdownTimer.Start();
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
            countdownTimer.Interval = 1000;
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
                countdownTimer.Stop();
                ClearSessionFile();
                this.FormClosing -= BNetCafeTimer_FormClosing;
                this.Close();
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