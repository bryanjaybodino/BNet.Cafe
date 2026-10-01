using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using System;

namespace BNet.Cafe.Server.Forms
{
    public partial class InventoryCreate : System.Web.UI.UserControl
    {
        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            InventoryItems inventoryItems = new InventoryItems();
            bool isSuccess = inventoryItems.Create(
                TextBox_ItemName.Text.Trim(),
                TextBox_Category.Text.Trim(),
                TextBox_UnitPrice.Text.Trim(),
                TextBox_QuantityInStock.Text.Trim(),
                TextBox_ReorderLevel.Text.Trim()
            );

            if (isSuccess)
            {
                AlertService.ShowAlert(UpdatePanel1, "Inventory item successfully added.", "success", "BNetPage.aspx?Form=Inventory");
            }
            else
            {
                AlertService.ShowAlert(UpdatePanel1, "Failed to add item. An item with this name might already exist.", "error");
            }
        }
    }
}