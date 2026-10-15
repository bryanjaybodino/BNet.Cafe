using BNet.Cafe.Client.Design;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BNet.Cafe.Client.Services
{
    public class WallpaperLockscreenService : IDisposable
    {
        private readonly Form _parentForm;
        private readonly Action _onUserInteraction;
        private readonly Action _onAutoShutdownTriggered;
        private readonly string _wallpaperFolder;

        private PictureBox _pictureBox;
        private readonly List<SecondaryLockForm> _secondaryForms = new List<SecondaryLockForm>();

        private Timer _slideTimer;
        private Timer _countdownTimer;
        private Label _badgeCountdownLabel;

        private string[] _imageFiles = new string[0];
        private int _currentIndex = -1;
        private int _remainingIdleSeconds;
        private bool _loading;

        public PictureBox PictureBox => _pictureBox;

        public WallpaperLockscreenService(Form parentForm, Action onUserInteraction, Action onAutoShutdownTriggered)
        {
            _parentForm = parentForm ?? throw new ArgumentNullException(nameof(parentForm));
            _onUserInteraction = onUserInteraction;
            _onAutoShutdownTriggered = onAutoShutdownTriggered;
            _wallpaperFolder = DirectoryHelper.GetAppDataFolderPath("Wallpaper");

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

            if (control.BackColor == Color.Transparent || control.BackColor.A < 255)
            {
                control.BackColor = Color.FromArgb(160, 15, 23, 42);
            }
        }

        public void ShowSecondaryScreens()
        {
            CloseSecondaryScreens();

            foreach (var screen in Screen.AllScreens)
            {
                if (!screen.Primary)
                {
                    var secForm = new SecondaryLockForm(screen.Bounds);
                    secForm.WallpaperPictureBox.MouseDown += (s, e) => _onUserInteraction?.Invoke();

                    if (_imageFiles.Length > 0 && _currentIndex >= 0 && _currentIndex < _imageFiles.Length)
                    {
                        Task.Run(async () =>
                        {
                            Bitmap scaled = LoadScaled(_imageFiles[_currentIndex], secForm.WallpaperPictureBox.ClientSize);
                            secForm.BeginInvoke((Action)(() =>
                            {
                                if (!secForm.IsDisposed && secForm.WallpaperPictureBox != null)
                                {
                                    secForm.WallpaperPictureBox.Image = scaled;
                                }
                                else
                                {
                                    scaled?.Dispose();
                                }
                            }));
                        });
                    }

                    secForm.Show();
                    _secondaryForms.Add(secForm);
                }
            }
        }

        public void CloseSecondaryScreens()
        {
            foreach (var form in _secondaryForms)
            {
                if (!form.IsDisposed)
                {
                    form.Close();
                    form.Dispose();
                }
            }
            _secondaryForms.Clear();
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
                _currentIndex = -1;
                LoadNextImage();

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

        private async void LoadNextImage()
        {
            if (_imageFiles.Length == 0 || _loading) return;
            _loading = true;
            try
            {
                _currentIndex = (_currentIndex + 1) % _imageFiles.Length;
                string path = _imageFiles[_currentIndex];

                Size target = _pictureBox.ClientSize.Width > 0 && _pictureBox.ClientSize.Height > 0
                    ? _pictureBox.ClientSize
                    : _parentForm.ClientSize;

                Bitmap scaled = await Task.Run(() => LoadScaled(path, target));

                if (_pictureBox.IsDisposed)
                {
                    scaled?.Dispose();
                }
                else
                {
                    var old = _pictureBox.Image;
                    _pictureBox.Image = scaled;
                    old?.Dispose();
                }

                // Push synchronized updates to all secondary screens
                foreach (var secForm in _secondaryForms)
                {
                    if (secForm.WallpaperPictureBox != null && !secForm.WallpaperPictureBox.IsDisposed)
                    {
                        Size secTarget = secForm.WallpaperPictureBox.ClientSize;
                        if (secTarget.Width <= 0 || secTarget.Height <= 0) secTarget = new Size(800, 600);

                        Bitmap secScaled = await Task.Run(() => LoadScaled(path, secTarget));
                        var oldSec = secForm.WallpaperPictureBox.Image;
                        secForm.WallpaperPictureBox.Image = secScaled;
                        oldSec?.Dispose();
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
            finally { _loading = false; }
        }

        private static Bitmap LoadScaled(string path, Size target)
        {
            if (target.Width <= 0 || target.Height <= 0) target = new Size(800, 600);
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
            ShowSecondaryScreens();

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
            CloseSecondaryScreens();
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

            _countdownTimer.Stop();
            _countdownTimer.Start();
        }

        public void Dispose()
        {
            Stop();
            _slideTimer?.Dispose();
            _countdownTimer?.Dispose();

            _pictureBox?.Image?.Dispose();
            _pictureBox?.Dispose();
        }
    }
}