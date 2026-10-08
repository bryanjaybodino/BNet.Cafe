using System;
using System.IO;
using System.Linq;

namespace BNet.Cafe.Client.Services
{
    public static class DirectoryHelper
    {
        private static string _baseDataDirectory;

        /// <summary>
        /// Gets the root persistent storage path across diskless and traditional setups.
        /// Optionally appends a subfolder (e.g. "Wallpaper", "Cache", "Data").
        /// </summary>
        public static string GetAppDataFolderPath(string subFolder = "")
        {
            if (string.IsNullOrEmpty(_baseDataDirectory))
            {
                // 1. Check for available non-system fixed drives (D:\, E:\, etc.)
                var targetDrive = DriveInfo.GetDrives()
                    .FirstOrDefault(d => d.IsReady
                                      && d.DriveType == DriveType.Fixed
                                      && !string.Equals(d.Name, @"C:\", StringComparison.OrdinalIgnoreCase));

                if (targetDrive != null)
                {
                    // Save on secondary drive, e.g., D:\BNetCafe
                    _baseDataDirectory = Path.Combine(targetDrive.Name, "BNetCafe");
                }
                else
                {
                    // Fallback for single-drive setups (C:\...)
                    _baseDataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BNetCafe");
                }
            }

            string targetPath = string.IsNullOrWhiteSpace(subFolder)
                ? _baseDataDirectory
                : Path.Combine(_baseDataDirectory, subFolder);

            if (!Directory.Exists(targetPath))
            {
                try
                {
                    Directory.CreateDirectory(targetPath);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to create directory '{targetPath}': {ex.Message}");
                    targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, subFolder);
                    Directory.CreateDirectory(targetPath);
                }
            }

            return targetPath;
        }
    }
}