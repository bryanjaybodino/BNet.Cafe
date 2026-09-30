using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using System.Web.UI;
using BNet.Cafe.Server.Services;

namespace BNet.Cafe.Server.Forms
{
    public partial class Wallpaper : System.Web.UI.UserControl
    {
        public class WallpaperItem
        {
            public string id { get; set; }
            public string name { get; set; }
            public string base64 { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            string rawJson = HiddenField_WallpaperData.Value;

            if (string.IsNullOrEmpty(rawJson))
            {
                AlertService.ShowAlert(UpdatePanel1, "No wallpapers selected for upload.", "error");
                return;
            }

            try
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                List<WallpaperItem> wallpapers = serializer.Deserialize<List<WallpaperItem>>(rawJson);

                if (wallpapers == null || wallpapers.Count == 0)
                {
                    AlertService.ShowAlert(UpdatePanel1, "No wallpapers found to upload.", "error");
                    return;
                }

                // Storage location for uploaded wallpapers
                string uploadFolder = Server.MapPath("~/Uploads/Wallpapers/");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                foreach (var wallpaper in wallpapers)
                {
                    // Clean Base64 string prefix (e.g. data:image/png;base64,)
                    string base64Data = wallpaper.base64.Substring(wallpaper.base64.IndexOf(",") + 1);
                    byte[] imageBytes = Convert.FromBase64String(base64Data);

                    string fileExtension = Path.GetExtension(wallpaper.name);
                    string fileName = "wp_" + Guid.NewGuid().ToString("N") + fileExtension;
                    string fullPath = Path.Combine(uploadFolder, fileName);

                    File.WriteAllBytes(fullPath, imageBytes);
                }

                AlertService.ShowAlert(UpdatePanel1, $"{wallpapers.Count} wallpaper(s) successfully saved.", "success", "BNetPage.aspx?Form=Wallpaper");
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert(UpdatePanel1, "Error saving wallpapers: " + ex.Message, "error");
            }
        }
    }
}