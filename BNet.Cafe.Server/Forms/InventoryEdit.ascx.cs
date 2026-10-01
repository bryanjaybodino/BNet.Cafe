using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using System;
using System.Data;

namespace BNet.Cafe.Server.Forms
{
    public partial class InventoryEdit : System.Web.UI.UserControl
    {
        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                InventoryItems inventoryItems = new InventoryItems();
                DataTable item = inventoryItems.GetById(Request.QueryString["id"]);
                if (item != null && item.Rows.Count > 0)
                {
                    TextBox_ItemName.Text = item.Rows[0]["DBItemName"].ToString();
                    TextBox_Category.Text = item.Rows[0]["DBCategory"].ToString();
                    TextBox_UnitPrice.Text = item.Rows[0]["DBUnitPrice"].ToString();
                    TextBox_QuantityInStock.Text = item.Rows[0]["DBQuantityInStock"].ToString();
                    TextBox_ReorderLevel.Text = item.Rows[0]["DBReorderLevel"].ToString();
                }
            }
        }

        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            InventoryItems inventoryItems = new InventoryItems();
            string id = Request.QueryString["id"];
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
    }
}