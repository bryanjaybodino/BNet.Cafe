using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace BNet.Cafe.Client.Services
{
    public class WallpaperService
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(uint uAction, uint uParam, string lpvParam, uint fuWinIni);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(uint uAction, uint uParam, StringBuilder lpvParam, uint fuWinIni);

        private const uint SPI_GETDESKWALLPAPER = 0x0073;
        private const uint SPI_SETDESKWALLPAPER = 0x0014;
        private const uint SPIF_UPDATEINIFILE = 0x01;
        private const uint SPIF_SENDCHANGE = 0x02;

        private readonly Timer _slideshowTimer;
        private readonly string _wallpaperFolderPath;
        private string _originalWallpaperPath;
        private string[] _imageFiles = new string[0];
        private int _currentImageIndex = -1;

        public WallpaperService(int intervalMs = 10000)
        {
            _wallpaperFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Wallpaper");

            // Ensure directory exists
            if (!Directory.Exists(_wallpaperFolderPath))
            {
                Directory.CreateDirectory(_wallpaperFolderPath);
            }

            _slideshowTimer = new Timer
            {
                Interval = intervalMs
            };
            _slideshowTimer.Tick += SlideshowTimer_Tick;
        }

        public void Start()
        {
            SaveOriginalWallpaper();
            RefreshImagesAndApply();
        }

        public void Stop()
        {
            _slideshowTimer.Stop();
            RestoreOriginalWallpaper();
        }

        public void RefreshImagesAndApply()
        {
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp" };

            if (Directory.Exists(_wallpaperFolderPath))
            {
                _imageFiles = Directory.GetFiles(_wallpaperFolderPath)
                    .Where(f => validExtensions.Contains(Path.GetExtension(f).ToLower()))
                    .ToArray();
            }
            else
            {
                _imageFiles = new string[0];
            }

            if (_imageFiles.Length == 0)
            {
                // No images found -> Restore default/original desktop wallpaper
                _slideshowTimer.Stop();
                RestoreOriginalWallpaper();
            }
            else if (_imageFiles.Length == 1)
            {
                // Single image -> Apply wallpaper once, no slideshow needed
                _slideshowTimer.Stop();
                SetWallpaper(_imageFiles[0]);
            }
            else
            {
                // Multiple images -> Start auto-slideshow
                _currentImageIndex = 0;
                SetWallpaper(_imageFiles[_currentImageIndex]);
                if (!_slideshowTimer.Enabled)
                {
                    _slideshowTimer.Start();
                }
            }
        }

        private void SlideshowTimer_Tick(object sender, EventArgs e)
        {
            if (_imageFiles.Length <= 1)
            {
                RefreshImagesAndApply();
                return;
            }

            _currentImageIndex = (_currentImageIndex + 1) % _imageFiles.Length;
            SetWallpaper(_imageFiles[_currentImageIndex]);
        }

        private void SaveOriginalWallpaper()
        {
            StringBuilder sb = new StringBuilder(260);
            SystemParametersInfo(SPI_GETDESKWALLPAPER, (uint)sb.Capacity, sb, 0);
            _originalWallpaperPath = sb.ToString();
        }

        private void RestoreOriginalWallpaper()
        {
            if (!string.IsNullOrEmpty(_originalWallpaperPath) && File.Exists(_originalWallpaperPath))
            {
                SetWallpaper(_originalWallpaperPath);
            }
        }

        private void SetWallpaper(string path)
        {
            if (File.Exists(path))
            {
                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
            }
        }
    }
}