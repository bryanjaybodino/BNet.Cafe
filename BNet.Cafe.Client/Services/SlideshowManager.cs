using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BNet.Cafe.Client.Services
{
    public class SlideshowManager : IDisposable
    {
        private readonly Form _parentForm;
        private readonly Action _onUserInteraction;
        private readonly Action _onAutoShutdownTriggered;
        private readonly string _wallpaperFolder;

        private PictureBox _pictureBox;
        private Timer _slideTimer;
        private Timer _countdownTimer;
        private Label _badgeCountdownLabel;

        private string[] _imageFiles = new string[0];
        private int _currentIndex = -1;
        private int _remainingIdleSeconds;

        public PictureBox PictureBox => _pictureBox;

        public SlideshowManager(Form parentForm, Action onUserInteraction, Action onAutoShutdownTriggered)
        {
            _parentForm = parentForm ?? throw new ArgumentNullException(nameof(parentForm));
            _onUserInteraction = onUserInteraction;
            _onAutoShutdownTriggered = onAutoShutdownTriggered;

            _wallpaperFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Wallpaper");

            InitializePictureBox();
            InitializeTimers();
            LoadImages();
        }

        private void InitializePictureBox()
        {
            _pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.FromArgb(15, 23, 42)
            };

            // User interaction on the wallpaper backdrop resets idle timer
            _pictureBox.MouseDown += (s, e) => _onUserInteraction?.Invoke();

            _parentForm.Controls.Add(_pictureBox);
            _pictureBox.SendToBack();
        }

        private void InitializeTimers()
        {
            _slideTimer = new Timer { Interval = 10000 };
            _slideTimer.Tick += SlideTimer_Tick;

            _countdownTimer = new Timer { Interval = 1000 };
            _countdownTimer.Tick += CountdownTimer_Tick;
        }

        public void AttachOverlayControl(Control control)
        {
            if (control == null) return;

            Point originalPos = control.Location;
            control.Parent = _pictureBox;
            control.Location = originalPos;

            // Retain transparent appearance relative to PictureBox backdrop
            if (control.BackColor == Color.Transparent || control.BackColor.A < 255)
            {
                control.BackColor = Color.FromArgb(160, 15, 23, 42);
            }
        }

        public void LoadImages()
        {
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp" };
            if (Directory.Exists(_wallpaperFolder))
            {
                _imageFiles = Directory.GetFiles(_wallpaperFolder)
                    .Where(f => validExtensions.Contains(Path.GetExtension(f).ToLower()))
                    .ToArray();
            }

            if (_imageFiles.Length > 0)
            {
                LoadNextImage();
            }
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
                System.Diagnostics.Debug.WriteLine($"Error displaying image: {ex.Message}");
            }
        }

        public void Start(int idleTimeoutSeconds, Label badgeCountdown)
        {
            _badgeCountdownLabel = badgeCountdown;
            _remainingIdleSeconds = idleTimeoutSeconds;

            UpdateCountdownDisplay();

            if (_imageFiles.Length > 1)
            {
                _slideTimer.Stop();
                _slideTimer.Start();
            }

            _countdownTimer.Stop();
            _countdownTimer.Start();
        }

        public void Stop()
        {
            _slideTimer.Stop();
            _countdownTimer.Stop();
        }

        private void SlideTimer_Tick(object sender, EventArgs e)
        {
            LoadNextImage();
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            if (_remainingIdleSeconds <= 0)
            {
                _countdownTimer.Stop();
                _parentForm.BeginInvoke((Action)(() => _onAutoShutdownTriggered?.Invoke()));
                return;
            }

            _remainingIdleSeconds--;
            UpdateCountdownDisplay();
        }

        private void UpdateCountdownDisplay()
        {
            if (_badgeCountdownLabel == null || _badgeCountdownLabel.IsDisposed) return;

            TimeSpan ts = TimeSpan.FromSeconds(_remainingIdleSeconds);
            _badgeCountdownLabel.Text = $"⚠️ Auto-Shutdown in: {ts.Minutes:D2}:{ts.Seconds:D2}";

            if (_remainingIdleSeconds <= 60)
            {
                _badgeCountdownLabel.BackColor = Color.FromArgb(200, 239, 68, 68);
                _badgeCountdownLabel.ForeColor = Color.White;
            }
            else
            {
                _badgeCountdownLabel.BackColor = Color.FromArgb(160, 15, 23, 42);
                _badgeCountdownLabel.ForeColor = Color.White;
            }
        }

        public void Dispose()
        {
            _slideTimer?.Stop();
            _slideTimer?.Dispose();

            _countdownTimer?.Stop();
            _countdownTimer?.Dispose();

            _pictureBox?.Image?.Dispose();
            _pictureBox?.Dispose();
        }
    }
}