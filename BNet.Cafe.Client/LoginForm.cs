using BNet.Cafe.Client.Services;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BNet.Cafe.Client
{
    public partial class LoginForm : Form
    {
        private PictureBox _pictureBox;
        private Timer _slideTimer;
        private Timer _countdownTimer;
        private Timer _idleTimer;

        private string[] _imageFiles = new string[0];
        private int _currentIndex = -1;
        private readonly string _wallpaperFolder;
        private Point _lastMousePosition = Point.Empty;

        // Idle Timeout before hiding the login form (e.g., 30 seconds of inactivity)
        private const int IdleHideTimeoutSeconds = 30;
        private int _idleSecondsCounter = 0;

        private int AutoShutdownTimeoutSeconds
        {
            get
            {
                if (int.TryParse(ConfigHelper.AutoShutDownInterval?.Trim(), out int timeout))
                {
                    return timeout;
                }
                return 300;
            }
        }

        private int _remainingIdleSeconds = 300;

        public LoginForm()
        {
            InitializeComponent();
            InitializeCustomOverlayControls();

            _wallpaperFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Wallpaper");
            LoadImages();

            _slideTimer = new Timer { Interval = 10000 };
            _slideTimer.Tick += SlideTimer_Tick;

            InitializeCountdownTimer();
            InitializeIdleTracker();
        }

        private void InitializeCustomOverlayControls()
        {
            this.TopMost = !ConfigHelper.IsOverlayFreeze;

            _pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.FromArgb(15, 23, 42)
            };

            _pictureBox.MouseMove += PictureBox_MouseMove;
            _pictureBox.Click += (s, e) => ResetIdleTimer();

            this.Controls.Add(_pictureBox);
            _pictureBox.SendToBack();

            // Re-parent elements directly to PictureBox for background transparency
            AttachControlToPicture(lblBigPcName);
            AttachControlToPicture(lblSubtitle);
            AttachControlToPicture(badgeCountdown);
            AttachControlToPicture(pnlLoginCard);

            this.KeyPreview = true;
            this.KeyDown += (s, e) => ResetIdleTimer();
        }

        private void AttachControlToPicture(Control control)
        {
            Point originalPos = control.Location;
            control.Parent = _pictureBox;
            control.Location = originalPos;

            if (control.BackColor == Color.Transparent)
            {
                control.BackColor = Color.FromArgb(160, 15, 23, 42);
            }
        }

        private void InitializeCountdownTimer()
        {
            _remainingIdleSeconds = AutoShutdownTimeoutSeconds;
            _countdownTimer = new Timer { Interval = 1000 };
            _countdownTimer.Tick += CountdownTimer_Tick;
        }

        private void InitializeIdleTracker()
        {
            _idleTimer = new Timer { Interval = 1000 };
            _idleTimer.Tick += IdleTimer_Tick;
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            if (_remainingIdleSeconds <= 0)
            {
                _countdownTimer.Stop();
                CommandPromptService.ShutdownSystem();
                return;
            }

            _remainingIdleSeconds--;

            TimeSpan ts = TimeSpan.FromSeconds(_remainingIdleSeconds);
            badgeCountdown.Text = $"⚠️ Auto-Shutdown in: {ts.Minutes:D2}:{ts.Seconds:D2}";

            if (_remainingIdleSeconds <= 60)
            {
                badgeCountdown.BackColor = Color.FromArgb(200, 239, 68, 68);
                badgeCountdown.ForeColor = Color.White;
            }
        }

        private void IdleTimer_Tick(object sender, EventArgs e)
        {
            _idleSecondsCounter++;

            if (_idleSecondsCounter >= IdleHideTimeoutSeconds)
            {
                HideLoginForm();
            }
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (_lastMousePosition.IsEmpty)
            {
                _lastMousePosition = e.Location;
                return;
            }

            if (Math.Abs(e.X - _lastMousePosition.X) > 2 || Math.Abs(e.Y - _lastMousePosition.Y) > 2)
            {
                _lastMousePosition = e.Location;
                ResetIdleTimer();
            }
        }

        public void ResetIdleTimer()
        {
            _idleSecondsCounter = 0;

            if (!pnlLoginCard.Visible)
            {
                ShowLoginForm();
            }
        }

        private void HideLoginForm()
        {
            if (pnlLoginCard.Visible)
            {
                pnlLoginCard.Visible = false;
            }
        }

        private void ShowLoginForm()
        {
            pnlLoginCard.Visible = true;
            txtUsername.Focus();
        }

        private void LoadImages()
        {
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp" };
            if (Directory.Exists(_wallpaperFolder))
            {
                _imageFiles = Directory.GetFiles(_wallpaperFolder)
                    .Where(f => validExtensions.Contains(Path.GetExtension(f).ToLower()))
                    .ToArray();
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            LoadNextImage();

            if (_imageFiles.Length > 1)
            {
                _slideTimer.Start();
            }

            _remainingIdleSeconds = AutoShutdownTimeoutSeconds;
            _countdownTimer.Start();
            _idleTimer.Start();
        }

        private void SlideTimer_Tick(object sender, EventArgs e)
        {
            LoadNextImage();
        }

        private void LoadNextImage()
        {
            if (_imageFiles.Length == 0) return;

            try
            {
                _currentIndex = (_currentIndex + 1) % _imageFiles.Length;
                string imagePath = _imageFiles[_currentIndex];

                using (var stream = new MemoryStream(File.ReadAllBytes(imagePath)))
                {
                    var oldImage = _pictureBox.Image;
                    _pictureBox.Image = Image.FromStream(stream);
                    oldImage?.Dispose();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error displaying image: {ex.Message}");
            }
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            string clientName = ConfigHelper.GetClientNameFromIP();
            lblBigPcName.Text = string.IsNullOrEmpty(clientName) ? "PC-01" : clientName;

            CenterLoginCard();
        }

        private void LoginForm_Resize(object sender, EventArgs e)
        {
            CenterLoginCard();
        }

        private void CenterLoginCard()
        {
            if (pnlLoginCard != null)
            {
                pnlLoginCard.Location = new Point(
                    (this.ClientSize.Width - pnlLoginCard.Width) / 2,
                    (this.ClientSize.Height - pnlLoginCard.Height) / 2
                );
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            ResetIdleTimer();

            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblErrorMessage.Text = "Please enter both username and password.";
                lblErrorMessage.Visible = true;
                return;
            }

            // Add your login authentication logic here
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Control_Interaction(object sender, EventArgs e)
        {
            ResetIdleTimer();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _slideTimer.Stop();
            _countdownTimer.Stop();
            _idleTimer.Stop();
            _pictureBox.Image?.Dispose();
            base.OnFormClosing(e);
        }
    }
}