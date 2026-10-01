using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using System;
using System.Data;
using System.IO;

namespace BNet.Cafe.Server.Forms
{
    public partial class InventoryEdit : System.Web.UI.UserControl
    {
        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string itemId = Request.QueryString["id"];
                InventoryItems inventoryItems = new InventoryItems();
                DataTable item = inventoryItems.GetById(itemId);

                if (item != null && item.Rows.Count > 0)
                {
                    TextBox_ItemName.Text = item.Rows[0]["DBItemName"].ToString();
                    TextBox_Category.Text = item.Rows[0]["DBCategory"].ToString();
                    TextBox_UnitPrice.Text = item.Rows[0]["DBUnitPrice"].ToString();
                    TextBox_QuantityInStock.Text = item.Rows[0]["DBQuantityInStock"].ToString();
                    TextBox_ReorderLevel.Text = item.Rows[0]["DBReorderLevel"].ToString();

                    // Load existing image if available
                    LoadExistingImage(itemId);
                }
            }
        }

        private void LoadExistingImage(string itemId)
        {
            string uploadFolder = Server.MapPath("~/Uploads/Inventory/");
            string imagePath = Path.Combine(uploadFolder, $"{itemId}.png");

            if (File.Exists(imagePath))
            {
                try
                {
                    byte[] bytes = File.ReadAllBytes(imagePath);
                    string base64 = "data:image/png;base64," + Convert.ToBase64String(bytes);
                    HiddenField_ImageData.Value = base64;
                }
                catch
                {
                    // Fallback if image load fails
                    HiddenField_ImageData.Value = string.Empty;
                }
            }
        }

        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            string id = Request.QueryString["id"];

            // Handle Image Saving / Removal
            SaveOrUpdateInventoryImage(id);

            InventoryItems inventoryItems = new InventoryItems();
            bool isSuccess = inventoryItems.Update(
                id,
                TextBox_ItemName.Text.Trim(),
                TextBox_Category.Text.Trim(),
                TextBox_UnitPrice.Text.Trim(),
                TextBox_QuantityInStock.Text.Trim(),
                TextBox_ReorderLevel.Text.Trim()
            );

            if (isSuccess)
            {
                AlertService.ShowAlert(UpdatePanel1, "Inventory item details updated successfully.", "success", "BNetPage.aspx?Form=Inventory");
            }
            else
            {
                AlertService.ShowAlert(UpdatePanel1, "Failed to update inventory item.", "error");
            }
        }

        private void SaveOrUpdateInventoryImage(string id)
        {
            string uploadFolder = Server.MapPath("~/Uploads/Inventory/");
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            string targetPath = Path.Combine(uploadFolder, $"{id}.png");
            string base64Raw = HiddenField_ImageData.Value;

            // Case 1: Image cleared by user -> delete file if exists
            if (string.IsNullOrEmpty(base64Raw))
            {
                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }
                return;
            }

            // Case 2: New image base64 data uploaded
            try
            {
                string base64Data = base64Raw.Substring(base64Raw.IndexOf(",") + 1);
                byte[] imageBytes = Convert.FromBase64String(base64Data);

                // Compress using FileImageHelper
                FileImageHelper helper = new FileImageHelper();
                byte[] compressedBytes = helper.CompressImageByteQuality(imageBytes, quality: 85L);

                File.WriteAllBytes(targetPath, compressedBytes);
            }
            catch
            {
                // Ignore base64 format errors if data didn't change
            }
        }
    }
}