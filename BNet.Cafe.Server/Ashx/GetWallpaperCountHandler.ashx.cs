using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Web;

namespace BNet.Cafe.Server.Ashx
{
    /// <summary>
    /// Summary description for GetWallpaperCountHandler
    /// </summary>
    public class GetWallpaperCountHandler : IHttpHandler
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            try
            {
                // Map the relative path to the physical path on the server
                string relativePath = @"~\Uploads\Wallpapers";
                string physicalPath = context.Server.MapPath(relativePath);

                if (!Directory.Exists(physicalPath))
                {
                    SendJsonResponse(context, false, "Wallpapers directory does not exist.");
                    return;
                }

                // Get all image files matching allowed extensions
                var imageFiles = Directory.GetFiles(physicalPath)
                    .Where(file => AllowedExtensions.Contains(Path.GetExtension(file).ToLower()))
                    .Select(Path.GetFileName)
                    .ToList();

                var responseData = new GetWallpaperCountData
                {
                    TotalCount = imageFiles.Count,
                    FileNames = imageFiles
                };

                SendJsonResponse(context, true, "Wallpaper count retrieved successfully.", responseData);
            }
            catch (Exception ex)
            {
                SendJsonResponse(context, false, $"Server error: {ex.Message}");
            }
        }

        private void SendJsonResponse(HttpContext context, bool success, string message, GetWallpaperCountData data = null)
        {
            var responseObj = new
            {
                success = success,
                message = message,
                data = data
            };

            string jsonResponse = JsonConvert.SerializeObject(responseObj);
            context.Response.Write(jsonResponse);
        }

        public bool IsReusable => false;

        public class GetWallpaperCountData
        {
            [JsonProperty("totalCount")]
            public int TotalCount { get; set; }

            [JsonProperty("fileNames")]
            public System.Collections.Generic.List<string> FileNames { get; set; }
        }
    }
}