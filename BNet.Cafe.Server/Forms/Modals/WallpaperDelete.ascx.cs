using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.UI;

namespace BNet.Cafe.Server.Forms.Modals
{
    public partial class WallpaperDelete : System.Web.UI.UserControl
    {
        protected void LinkButton_ConfirmDelete_Click(object sender, EventArgs e)
        {
            // Verify that the postback target is actually this button
            string eventTarget = Request.Form["__EVENTTARGET"] ?? string.Empty;
            if (!eventTarget.Contains(LinkButton_ConfirmDelete.ID))
            {
                return;
            }

            try
            {
                string fileName = HiddenField_DeleteTarget.Value;
                if (!string.IsNullOrEmpty(fileName))
                {
                    DeleteFileAndReindexServer(fileName);
                }
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert(this, "Delete failed: " + ex.Message, "error");
            }
        }

        private void DeleteFileAndReindexServer(string fileName)
        {
            try
            {
                string uploadFolder = Server.MapPath("~/Uploads/Wallpapers/");
                string path = Path.Combine(uploadFolder, fileName);

                // 1. Delete target file if it exists
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                // 2. Re-index remaining PNG files sequentially (1.png, 2.png, 3.png, ...)
                if (Directory.Exists(uploadFolder))
                {
                    ReindexRemainingWallpapers(uploadFolder);
                }

                // 3. Refresh parent control's data so HiddenField_WallpaperData gets updated
                RefreshParentGallery();


                ClientData clientData = new ClientData();
                var targetList = clientData.FetchData();
                foreach (var target in targetList)
                {
                    RemoteMessagingService.RefreshPC(this, target.ClientName);
                }
                AlertService.ShowAlert(this, $"{fileName} deleted and wallpapers re-indexed successfully.", "success");
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert(this, "Error deleting file: " + ex.Message, "error");
            }
        }

        private void ReindexRemainingWallpapers(string uploadFolder)
        {
            // Get all remaining PNG files and sort them numerically by their current index
            var files = Directory.GetFiles(uploadFolder, "*.png")
                .Select(f => new FileInfo(f))
                .OrderBy(f =>
                {
                    int number;
                    return int.TryParse(Path.GetFileNameWithoutExtension(f.Name), out number) ? number : int.MaxValue;
                })
                .ToList();

            // Temporary directory to avoid file lock / name collision issues during bulk rename
            string tempFolder = Path.Combine(uploadFolder, "TempReindex_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempFolder);

            try
            {
                // Step A: Move remaining files to temporary directory with new 1..N sequence names
                for (int i = 0; i < files.Count; i++)
                {
                    int newIndex = i + 1;
                    string tempPath = Path.Combine(tempFolder, $"{newIndex}.png");
                    File.Move(files[i].FullName, tempPath);
                }

                // Step B: Move indexed files back to main upload folder
                var tempFiles = Directory.GetFiles(tempFolder, "*.png");
                foreach (var tempFile in tempFiles)
                {
                    string targetPath = Path.Combine(uploadFolder, Path.GetFileName(tempFile));
                    File.Move(tempFile, targetPath);
                }
            }
            finally
            {
                // Step C: Clean up temporary folder
                if (Directory.Exists(tempFolder))
                {
                    Directory.Delete(tempFolder, true);
                }
            }
        }

        private void RefreshParentGallery()
        {
            // Traverse up to find the parent Wallpaper control and refresh existing items
            Control parent = Parent;
            while (parent != null && !(parent is Forms.Wallpaper))
            {
                parent = parent.Parent;
            }

            if (parent is Forms.Wallpaper wallpaperControl)
            {
                MethodInfo loadMethod = typeof(Forms.Wallpaper).GetMethod("LoadExistingWallpapers", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (loadMethod != null)
                {
                    loadMethod.Invoke(wallpaperControl, null);
                }
            }
        }
    }
}