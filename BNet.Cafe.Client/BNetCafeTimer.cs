using System;
using System.Configuration;
using System.Drawing;
using System.Windows.Forms;

namespace BNet.Cafe.Client
{
    public partial class BNetCafeTimer : Form
    {
        private double remainingSeconds = 0;

        public BNetCafeTimer(string clientName, string duration, string amount)
        {
            InitializeComponent();

            // Set Form Style Properties
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.TopMost = true; // Keep window always on top
            this.ShowInTaskbar = false;

            // Initialize values
            UpdateTimerData(clientName, duration, amount);
        }

        /// <summary>
        /// Updates or top-ups existing timer data without opening a new form instance.
        /// </summary>
        public void UpdateTimerData(string customerName, string duration, string amount)
        {
            double.TryParse(duration, out double parsedDurationMinutes);
            double.TryParse(amount, out double parsedAmount);

            // Convert minutes into total remaining seconds
            double addedSeconds = parsedDurationMinutes * 60;
            remainingSeconds += addedSeconds;

            // Update UI Labels
            Label_ClientName.Text = ConfigurationManager.AppSettings["ClientName"].ToUpper().Replace(" ", "");
            Label_CustomerName.Text = $"User : {(customerName == "" ? "GUEST" : customerName.ToUpper())}";
            Label_TotalHours.Text = $"Purchased : {FormatPurchasedTime(remainingSeconds)}";
            label_TotalAmount.Text = $"Amount : ₱{parsedAmount:N2}";

            UpdateDisplay();
            Button_Logout.Enabled = true;

            if (!countdownTimer.Enabled)
            {
                countdownTimer.Start();
            }
        }

        private void BNetCafeTimer_Load(object sender, EventArgs e)
        {
            // Position window at the Top-Right corner of the screen
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(workingArea.Right - this.Width - 15, workingArea.Top + 15);

            countdownTimer.Interval = 1000; // Tick every 1 second
            countdownTimer.Start();
        }

        private void BNetCafeTimer_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Prevent users from manually closing the window
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            if (remainingSeconds > 0)
            {
                remainingSeconds--;
                UpdateDisplay();
            }
            else
            {
                countdownTimer.Stop();
                Label_TimerDisplay.Text = "00:00:00";
                Button_Logout.Enabled = false;
                MessageBox.Show("Your time has expired!", "Session Ended", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateDisplay()
        {
            TimeSpan time = TimeSpan.FromSeconds(remainingSeconds);
            Label_TimerDisplay.Text = time.ToString(@"hh\:mm\:ss");
        }

        private void Button_Logout_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to log out?", "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                countdownTimer.Stop();
                this.FormClosing -= BNetCafeTimer_FormClosing; // Remove close prevention lock
                this.Close();
            }
        }

        private string FormatPurchasedTime(double totalSeconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(totalSeconds);

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