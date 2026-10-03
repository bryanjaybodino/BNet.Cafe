using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using System;
using System.Data;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server.Forms
{
    public partial class Sales : System.Web.UI.UserControl
    {
        private readonly InventoryTransactions transactionsRepo = new InventoryTransactions();

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DateTime dateTime = TimeService.Get();
                DateTime firstDayOfMonth = new DateTime(dateTime.Year, dateTime.Month, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                TextBox_DateRange.Text = $"{firstDayOfMonth:yyyy-MM-dd},{lastDayOfMonth:yyyy-MM-dd}";
            }

            string search = TextBox_Search.Text.Trim();
            string dateRange = TextBox_DateRange.Text;

            // Load metrics using InventoryTransactions
            var count = transactionsRepo.GetSalesCount(search, dateRange);
            Label_TotalOrders.Text = count.TotalOrders;
            Label_ItemsSold.Text = count.ItemsSold;
            Label_AvgOrderValue.Text = count.AvgOrderValue;
            Label_Revenue.Text = count.Revenue;

            // Load transactions grid
            DataTable data = transactionsRepo.GetAll(search,"SALE",dateRange, GridViewTemplateService.GetPaginationIndex(GridViewTable));
            GridViewTemplateService.SetGridView(GridViewTable, data, Panel_Pagination);
        }

        protected void GridViewTable_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridViewTable.PageIndex = e.NewPageIndex;
        }

        protected void TextBox_Search_TextChanged(object sender, EventArgs e)
        {
            GridViewTable.PageIndex = 0;
        }
    }
}