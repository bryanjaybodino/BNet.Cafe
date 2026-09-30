using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
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
            public bool isExisting { get; set; }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadExistingWallpapers();
            }
        }

        private void LoadExistingWallpapers()
        {
            string uploadFolder = Server.MapPath("~/Uploads/Wallpapers/");
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
                return;
            }

            var files = Directory.GetFiles(uploadFolder, "*.png")
                .Select(f => new FileInfo(f))
                .OrderBy(f =>
                {
                    int number;
                    return int.TryParse(Path.GetFileNameWithoutExtension(f.Name), out number) ? number : int.MaxValue;
                })
                .ToList();

            List<WallpaperItem> existingList = new List<WallpaperItem>();

            foreach (var file in files)
            {
                byte[] bytes = File.ReadAllBytes(file.FullName);
                string base64 = "data:image/png;base64," + Convert.ToBase64String(bytes);

                existingList.Add(new WallpaperItem
                {
                    id = file.Name,
                    name = file.Name,
                    base64 = base64,
                    isExisting = true
                });
            }

            // Set MaxJsonLength to int.MaxValue to support large base64 image strings
            JavaScriptSerializer serializer = new JavaScriptSerializer
            {
                MaxJsonLength = int.MaxValue
            };

            HiddenField_WallpaperData.Value = serializer.Serialize(existingList);
        }
        protected void LinkButton_Delete_Click(object sender, EventArgs e)
        {
            try
            {
                string fileName = HiddenField_DeleteTarget.Value;
                if (!string.IsNullOrEmpty(fileName))
                {
                    DeleteFileFromServer(fileName);
                }
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert(UpdatePanel1, "Delete failed: " + ex.Message, "error");
            }
        }
        private void DeleteFileFromServer(string fileName)
        {
            try
            {
                string path = Server.MapPath("~/Uploads/Wallpapers/" + fileName);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                // Refresh hidden field data so client updates UI automatically
                LoadExistingWallpapers();
                AlertService.ShowAlert(UpdatePanel1, $"{fileName} deleted successfully.", "success");
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert(UpdatePanel1, "Error deleting file: " + ex.Message, "error");
            }
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
                // Set MaxJsonLength when deserializing large JSON data back from client
                JavaScriptSerializer serializer = new JavaScriptSerializer
                {
                    MaxJsonLength = int.MaxValue
                };

                List<WallpaperItem> wallpapers = serializer.Deserialize<List<WallpaperItem>>(rawJson);

                if (wallpapers == null || wallpapers.Count == 0)
                {
                    AlertService.ShowAlert(UpdatePanel1, "No wallpapers found to upload.", "error");
                    return;
                }

                string uploadFolder = Server.MapPath("~/Uploads/Wallpapers/");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                int nextIndex = GetNextFileIndex(uploadFolder);
                int savedCount = 0;

                foreach (var wallpaper in wallpapers)
                {
                    if (wallpaper.isExisting) continue;

                    string base64Data = wallpaper.base64.Substring(wallpaper.base64.IndexOf(",") + 1);
                    byte[] imageBytes = Convert.FromBase64String(base64Data);

                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    using (Image img = Image.FromStream(ms))
                    {
                        string targetPath = Path.Combine(uploadFolder, $"{nextIndex}.png");
                        img.Save(targetPath, ImageFormat.Png);
                        nextIndex++;
                        savedCount++;
                    }
                }

                AlertService.ShowAlert(UpdatePanel1, $"{savedCount} new wallpaper(s) successfully saved.", "success");
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert(UpdatePanel1, "Error saving wallpapers: " + ex.Message, "error");
            }
        }

        private int GetNextFileIndex(string folderPath)
        {
            var files = Directory.GetFiles(folderPath, "*.png");
            int max = 0;

            foreach (var file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                if (int.TryParse(fileName, out int num))
                {
                    if (num > max) max = num;
                }
            }

            return max + 1;
        }
    }
}