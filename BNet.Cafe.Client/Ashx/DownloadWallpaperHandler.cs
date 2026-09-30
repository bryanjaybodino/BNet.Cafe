using BNet.Cafe.Client.Services;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace BNet.Cafe.Client.Ashx
{
    internal class DownloadWallpaperHandler
    {
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Downloads a wallpaper image file stream by file name.
        /// </summary>
        public async Task<byte[]> DownloadWallpaperAsync(string fileName)
        {
            string baseUrl = ConfigHelper.AppUrl?.TrimEnd('/');
            string handlerUrl = $"{baseUrl}/Ashx/DownloadWallpaperHandler.ashx?fileName={Uri.EscapeDataString(fileName)}";

            HttpResponseMessage response = await client.GetAsync(handlerUrl);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }

        /// <summary>
        /// Downloads wallpaper directly to a specified local file destination path.
        /// </summary>
        public async Task DownloadWallpaperToFileAsync(string fileName, string saveFilePath)
        {
            byte[] fileBytes = await DownloadWallpaperAsync(fileName);
            File.WriteAllBytes(saveFilePath, fileBytes);
        }
    }
}