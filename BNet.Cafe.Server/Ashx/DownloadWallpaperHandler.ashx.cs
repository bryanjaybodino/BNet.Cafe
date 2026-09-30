using Newtonsoft.Json;
using System;
using System.IO;
using System.Web;

namespace BNet.Cafe.Server.Ashx
{
    /// <summary>
    /// Summary description for DownloadWallpaperHandler
    /// </summary>
    public class DownloadWallpaperHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                string fileName = context.Request["fileName"];

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    SendJsonErrorResponse(context, "Parameter 'fileName' is required.");
                    return;
                }

                // Sanitize file name to prevent path traversal attacks
                string safeFileName = Path.GetFileName(fileName);
                string relativePath = @"~\Uploads\Wallpapers";
                string physicalPath = Path.Combine(context.Server.MapPath(relativePath), safeFileName);

                if (!File.Exists(physicalPath))
                {
                    SendJsonErrorResponse(context, "Requested wallpaper file was not found.");
                    return;
                }

                // Clear response headers and content
                context.Response.Clear();
                context.Response.ContentType = MimeMapping.GetMimeMapping(physicalPath);
                context.Response.AddHeader("Content-Disposition", $"attachment; filename=\"{safeFileName}\"");
                context.Response.AddHeader("Content-Length", new FileInfo(physicalPath).Length.ToString());

                // Write file directly to response stream
                context.Response.WriteFile(physicalPath);
                context.Response.Flush();
            }
            catch (Exception ex)
            {
                SendJsonErrorResponse(context, $"Server error: {ex.Message}");
            }
        }

        private void SendJsonErrorResponse(HttpContext context, string message)
        {
            context.Response.ContentType = "application/json";
            var responseObj = new
            {
                success = false,
                message = message,
                data = (object)null
            };

            string jsonResponse = JsonConvert.SerializeObject(responseObj);
            context.Response.Write(jsonResponse);
        }

        public bool IsReusable => false;
    }
}