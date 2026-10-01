using System;
using System.Data;
using System.Web.UI.WebControls;
using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;

namespace BNet.Cafe.Server.Forms
{
    public partial class POS : System.Web.UI.UserControl
    {
        private readonly InventoryItems itemsRepo = new InventoryItems();
        private readonly InventoryTransactions transactionsRepo = new InventoryTransactions();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCatalog();
            }
        }

        private void LoadCatalog()
        {
            DataTable dt = itemsRepo.GetAll();
            GridView_POSItems.DataSource = dt;
            GridView_POSItems.DataBind();
        }

        protected void GridView_POSItems_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "AddToCart")
            {
                string itemId = e.CommandArgument.ToString();
                DataTable dt = itemsRepo.GetById(itemId);

                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    HiddenField_SelectedItemId.Value = row["DBId"].ToString();
                    TextBox_SelectedItemName.Text = row["DBItemName"].ToString();
                    TextBox_UnitPrice.Text = Convert.ToDouble(row["DBUnitPrice"]).ToString("F2");
                    TextBox_Quantity.Text = "1";
                    CalculateTotal(null, null);
                }
            }
        }

        protected void CalculateTotal(object sender, EventArgs e)
        {
            double price = double.TryParse(TextBox_UnitPrice.Text, out double p) ? p : 0;
            int qty = int.TryParse(TextBox_Quantity.Text, out int q) ? q : 0;
            Label_Total.Text = (price * qty).ToString("F2");
        }

        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            string itemId = HiddenField_SelectedItemId.Value;
            string quantityStr = TextBox_Quantity.Text.Trim();
            string userId = "ADMIN"; // Replace with current Session User ID

            if (string.IsNullOrEmpty(itemId) || !int.TryParse(quantityStr, out int quantity) || quantity <= 0)
            {
                AlertService.ShowAlert(UpdatePanel1, "Please select an item and a valid quantity.", "warning");
                return;
            }

            // Verify Stock level
            DataTable dt = itemsRepo.GetById(itemId);
            if (dt != null && dt.Rows.Count > 0)
            {
                int currentStock = Convert.ToInt32(dt.Rows[0]["DBQuantityInStock"]);
                if (quantity > currentStock)
                {
                    AlertService.ShowAlert(UpdatePanel1, $"Insufficient stock. Current stock: {currentStock}", "error");
                    return;
                }
            }

            bool success = transactionsRepo.StockOut(itemId, userId, quantity.ToString());

            if (success)
            {
                ClearForm();
                LoadCatalog();
                AlertService.ShowAlert(UpdatePanel1, "Sale completed successfully.", "success");
            }
            else
            {
                AlertService.ShowAlert(UpdatePanel1, "Failed to complete transaction.", "error");
            }
        }

        private void ClearForm()
        {
            HiddenField_SelectedItemId.Value = string.Empty;
            TextBox_SelectedItemName.Text = string.Empty;
            TextBox_UnitPrice.Text = "0.00";
            TextBox_Quantity.Text = "1";
            Label_Total.Text = "0.00";
        }
    }
}