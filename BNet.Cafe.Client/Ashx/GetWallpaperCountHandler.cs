using BNet.Cafe.Client.Services;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BNet.Cafe.Client.Ashx
{
    internal class GetWallpaperCountHandler
    {
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Fetches total count and filenames of wallpapers.
        /// </summary>
        public async Task<GetWallpaperCountHandlerData> GetWallpaperCountAsync()
        {
            string baseUrl = ConfigHelper.AppUrl?.TrimEnd('/');
            string handlerUrl = $"{baseUrl}/Ashx/GetWallpaperCountHandler.ashx";

            HttpResponseMessage response = await client.GetAsync(handlerUrl);
            response.EnsureSuccessStatusCode();

            string jsonString = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<GetWallpaperCountHandlerData>(jsonString);
        }
    }
}