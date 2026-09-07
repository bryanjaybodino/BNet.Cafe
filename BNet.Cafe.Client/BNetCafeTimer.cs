using System;
using System.Drawing;
using System.Windows.Forms;

namespace BNet.Cafe.Client
{
    public partial class BNetCafeTimer : Form
    {
        private int remainingSeconds = 0;

        public BNetCafeTimer()
        {
            InitializeComponent();
            if (cbPcSelection.Items.Count > 0)
            {
                cbPcSelection.SelectedIndex = 0;
            }
        }

        private void TimerCard_Paint(object sender, PaintEventArgs e)
        {
            Color borderLight = ColorTranslator.FromHtml("#e2e8f0");
            ControlPaint.DrawBorder(e.Graphics, timerCard.ClientRectangle, borderLight, ButtonBorderStyle.Solid);
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            remainingSeconds = (int)numMinutes.Value * 60;
            UpdateDisplay();
            sessionTimer.Start();
            btnStart.Enabled = false;
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            sessionTimer.Stop();
            remainingSeconds = 0;
            UpdateDisplay();
            btnStart.Enabled = true;
        }

        private void SessionTimer_Tick(object sender, EventArgs e)
        {
            if (remainingSeconds > 0)
            {
                remainingSeconds--;
                UpdateDisplay();
            }
            else
            {
                sessionTimer.Stop();
                btnStart.Enabled = true;
                MessageBox.Show($"Time is up for {cbPcSelection.SelectedItem}!", "Session Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void UpdateDisplay()
        {
            TimeSpan time = TimeSpan.FromSeconds(remainingSeconds);
            lblTimerDisplay.Text = time.ToString(@"hh\:mm\:ss");
        }
    }
}