using BNet.Cafe.Client.Design;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            _pictureBox = new BufferedPictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.FromArgb(15, 23, 42)
            };

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
            else
            {
                _imageFiles = new string[0];
            }

            if (_imageFiles.Length > 0)
            {
                // Reset index to ensure clean cycling when new files arrive
                _currentIndex = -1;
                LoadNextImage();

                // Start slideshow timer if more than 1 image is present
                if (_imageFiles.Length > 1)
                {
                    _slideTimer.Stop();
                    _slideTimer.Start();
                }
                else
                {
                    _slideTimer.Stop();
                }
            }
            else
            {
                _slideTimer.Stop();
            }
        }

        private bool _loading;

        private async void LoadNextImage()
        {
            if (_imageFiles.Length == 0 || _loading) return;
            _loading = true;
            try
            {
                _currentIndex = (_currentIndex + 1) % _imageFiles.Length;
                string path = _imageFiles[_currentIndex];

                // Use PictureBox size or parent form size instead of screen bounds
                Size target = _pictureBox.ClientSize.Width > 0 && _pictureBox.ClientSize.Height > 0
                    ? _pictureBox.ClientSize
                    : _parentForm.ClientSize;

                Bitmap scaled = await Task.Run(() => LoadScaled(path, target));

                if (_pictureBox.IsDisposed) { scaled?.Dispose(); return; }
                var old = _pictureBox.Image;
                _pictureBox.Image = scaled;
                old?.Dispose();
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
            finally { _loading = false; }
        }

        private static Bitmap LoadScaled(string path, Size target)
        {
            using (var ms = new MemoryStream(File.ReadAllBytes(path)))
            using (var src = Image.FromStream(ms))
            {
                var bmp = new Bitmap(target.Width, target.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(src, 0, 0, target.Width, target.Height);
                }
                return bmp;
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
        public void ResetCountdown(int idleTimeoutSeconds)
        {
            _remainingIdleSeconds = idleTimeoutSeconds;
            UpdateCountdownDisplay();

            // Restart the countdown timer
            _countdownTimer.Stop();
            _countdownTimer.Start();
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