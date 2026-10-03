using BNet.Cafe.Client.Services;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BNet.Cafe.Client
{
    public partial class SlideshowOverlayForm : Form
    {
        private PictureBox _pictureBox;
        private Timer _slideTimer;
        private Timer _countdownTimer;
        private string[] _imageFiles = new string[0];
        private int _currentIndex = -1;
        private readonly string _wallpaperFolder;
        private Point _lastMousePosition = Point.Empty;

        private int IdleTimeoutSeconds
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

        public SlideshowOverlayForm()
        {
            InitializeComponent();
            InitializeCustomOverlayControls();

            _wallpaperFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Wallpaper");
            LoadImages();

            _slideTimer = new Timer { Interval = 10000 };
            _slideTimer.Tick += SlideTimer_Tick;

            InitializeCountdownTimer();
        }

        private void InitializeCustomOverlayControls()
        {
            this.TopMost = true;

            _pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage, // Fullscreen stretch fit
                BackColor = Color.FromArgb(15, 23, 42)
            };

            _pictureBox.Click += (s, e) => CloseOverlay();
            _pictureBox.MouseMove += PictureBox_MouseMove;

            this.Controls.Add(_pictureBox);
            _pictureBox.SendToBack();

            // Re-parent text labels directly to PictureBox for true transparency over images
            AttachControlToPicture(lblBigPcName);
            AttachControlToPicture(lblSubtitle);
            AttachControlToPicture(badgeCountdown);
            AttachControlToPicture(lblDismissHint);

            this.KeyPreview = true;
            this.KeyDown += (s, e) => CloseOverlay();
        }

        private void AttachControlToPicture(Control control)
        {
            Point originalPos = control.Location;
            control.Parent = _pictureBox;
            control.Location = originalPos;

            // Preserve the control's design background if it already has an alpha/background value
            if (control.BackColor == Color.Transparent)
            {
                // Default dark semi-transparent backdrop for plain labels (e.g., PC Name, Subtitle)
                control.BackColor = Color.FromArgb(160, 15, 23, 42);
            }
        }

        private void InitializeCountdownTimer()
        {
            _remainingIdleSeconds = IdleTimeoutSeconds;
            _countdownTimer = new Timer { Interval = 1000 };
            _countdownTimer.Tick += CountdownTimer_Tick;
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
                // Use ARGB with alpha (e.g., 200) instead of completely opaque Color.FromArgb(239, 68, 68)
                badgeCountdown.BackColor = Color.FromArgb(200, 239, 68, 68);
                badgeCountdown.ForeColor = Color.White;
            }
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (_lastMousePosition.IsEmpty)
            {
                _lastMousePosition = e.Location;
                return;
            }

            if (Math.Abs(e.X - _lastMousePosition.X) > 5 || Math.Abs(e.Y - _lastMousePosition.Y) > 5)
            {
                CloseOverlay();
            }
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

            _remainingIdleSeconds = IdleTimeoutSeconds;
            _countdownTimer.Start();
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

        public void CloseOverlay()
        {
            _slideTimer.Stop();
            _countdownTimer.Stop();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _slideTimer.Stop();
            _countdownTimer.Stop();
            _pictureBox.Image?.Dispose();
            base.OnFormClosing(e);
        }

        private void SlideshowOverlayForm_Load(object sender, EventArgs e)
        {
            string clientName = ConfigHelper.GetClientNameFromIP();
            lblBigPcName.Text = string.IsNullOrEmpty(clientName) ? "PC-01" : clientName;
        }
    }
}