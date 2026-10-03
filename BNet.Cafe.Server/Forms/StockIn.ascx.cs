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
        private readonly Sessions.User userCookies = new Sessions.User();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadItems();
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            DataTable dt = transactionsRepo.GetAll("STOCK_IN", GridViewTemplateService.GetPaginationIndex(GridView_StockIn));
            GridViewTemplateService.SetGridView(GridView_StockIn, dt, Panel_Pagination);
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

        protected void GridView_StockIn_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView_StockIn.PageIndex = e.NewPageIndex;
        }

        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            string itemId = DropDownList_Item.SelectedValue;
            string quantity = TextBox_Quantity.Text.Trim();
            string cost = TextBox_Cost.Text.Trim();
            string userId = userCookies.user_id;

            if (string.IsNullOrEmpty(itemId) || string.IsNullOrEmpty(quantity) || !int.TryParse(quantity, out int qty) || qty <= 0)
            {
                AlertService.ShowAlert(UpdatePanel1, "Please select a valid item and quantity.", "warning");
                return;
            }

            if (string.IsNullOrEmpty(cost) || !decimal.TryParse(cost, out decimal parsedCost) || parsedCost < 0)
            {
                AlertService.ShowAlert(UpdatePanel1, "Please enter a valid cost amount.", "warning");
                return;
            }

            bool success = transactionsRepo.StockIn(itemId, userId, quantity, cost);

            if (success)
            {
                ClearForm();
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
            TextBox_Cost.Text = string.Empty;
        }
    }
}