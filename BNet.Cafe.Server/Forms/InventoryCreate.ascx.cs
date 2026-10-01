using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using System;
using System.IO;

namespace BNet.Cafe.Server.Forms
{
    public partial class InventoryCreate : System.Web.UI.UserControl
    {
        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            InventoryItems inventoryItems = new InventoryItems();
            var (isSuccess, newItemId) = inventoryItems.Create(
                TextBox_ItemName.Text.Trim(),
                TextBox_Category.Text.Trim(),
                TextBox_UnitPrice.Text.Trim(),
                TextBox_QuantityInStock.Text.Trim(),
                TextBox_ReorderLevel.Text.Trim()
            );

            if (isSuccess)
            {
                // Save the image using the newly created ID
                SaveInventoryImage(newItemId);

                AlertService.ShowAlert(UpdatePanel1, "Inventory item successfully added.", "success", "BNetPage.aspx?Form=Inventory");
            }
            else
            {
                AlertService.ShowAlert(UpdatePanel1, "Failed to add item. An item with this name might already exist.", "error");
            }
        }

        private string SaveInventoryImage(string itemId)
        {
            string base64Raw = HiddenField_ImageData.Value;
            if (string.IsNullOrEmpty(base64Raw) || string.IsNullOrEmpty(itemId)) return string.Empty;

            try
            {
                string base64Data = base64Raw.Substring(base64Raw.IndexOf(",") + 1);
                byte[] imageBytes = Convert.FromBase64String(base64Data);

                FileImageHelper helper = new FileImageHelper();
                byte[] compressedBytes = helper.CompressImageByteQuality(imageBytes, quality: 85L);

                string uploadFolder = Server.MapPath("~/Uploads/Inventory/");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Save using itemId.png to match InventoryEdit loading logic
                string fileName = $"{itemId}.png";
                string targetPath = Path.Combine(uploadFolder, fileName);
                File.WriteAllBytes(targetPath, compressedBytes);

                return "~/Uploads/Inventory/" + fileName;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}