using System;
using System.Data;
using System.Web.UI.WebControls;
using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;

namespace BNet.Cafe.Server.Forms
{
    public partial class StockIn : System.Web.UI.UserControl
    {
        private readonly InventoryItems itemsRepo = new InventoryItems();
        private readonly InventoryTransactions transactionsRepo = new InventoryTransactions();
        Sessions.User userCookies = new Sessions.User();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadItems();
                LoadRecentLogs();
            }
        }

        private void LoadItems()
        {
            DataTable dt = itemsRepo.GetAll();
            DropDownList_Item.DataSource = dt;
            DropDownList_Item.DataTextField = "DBItemName";
            DropDownList_Item.DataValueField = "DBId";
            DropDownList_Item.DataBind();
            DropDownList_Item.Items.Insert(0, new ListItem("-- Select Item --", ""));
        }

        private void LoadRecentLogs()
        {
            DataTable dt = transactionsRepo.GetAll("STOCK_IN");
            GridView_StockIn.DataSource = dt;
            GridView_StockIn.DataBind();
        }

        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            string itemId = DropDownList_Item.SelectedValue;
            string quantity = TextBox_Quantity.Text.Trim();
            string userId = userCookies.user_id;

            if (string.IsNullOrEmpty(itemId) || string.IsNullOrEmpty(quantity) || int.Parse(quantity) <= 0)
            {
                AlertService.ShowAlert(UpdatePanel1, "Please select a valid item and quantity.", "warning");
                return;
            }

            bool success = transactionsRepo.StockIn(itemId, userId, quantity);

            if (success)
            {
                ClearForm();
                LoadRecentLogs();
                AlertService.ShowAlert(UpdatePanel1, "Stock successfully added to inventory.", "success");
            }
            else
            {
                AlertService.ShowAlert(UpdatePanel1, "Failed to complete Stock In transaction.", "error");
            }
        }

        protected void LinkButton_Cancel_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            DropDownList_Item.SelectedIndex = 0;
            TextBox_Quantity.Text = string.Empty;
        }
    }
}