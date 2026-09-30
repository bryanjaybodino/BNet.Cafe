using System;
using System.IO;
using System.Reflection;
using System.Web.UI;
using BNet.Cafe.Server.Services;

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
                    DeleteFileFromServer(fileName);
                }
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert(this, "Delete failed: " + ex.Message, "error");
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

                // Refresh parent control's data so HiddenField_WallpaperData is updated
                RefreshParentGallery();

                AlertService.ShowAlert(this, $"{fileName} deleted successfully.", "success");
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert(this, "Error deleting file: " + ex.Message, "error");
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