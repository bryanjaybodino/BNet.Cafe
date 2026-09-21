using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server.Forms
{
    public partial class TopUp : System.Web.UI.UserControl
    {
        private readonly Balances balances = new Balances();

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DateTime dateTime = TimeService.Get();
                DateTime firstDayOfMonth = new DateTime(dateTime.Year, dateTime.Month, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                TextBox_DateRage.Text = $"{firstDayOfMonth:yyyy-MM-dd},{lastDayOfMonth:yyyy-MM-dd}";
            }

            string search = TextBox_Search.Text.Trim();
            string userId = "";
            string dateRange = TextBox_DateRage.Text;

            var count = balances.GetCount(userId, dateRange);
            Label_TotalTopUps.Text = count.Total;
            Label_Users.Text = count.Users;
            Label_Duration.Text = count.Duration;
            Label_Income.Text = count.Income;

            DataTable data = balances.GetAll(search, userId, dateRange, GridViewTemplateService.GetPaginationIndex(GridViewTable));
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

        protected void LinkButton_Refresh_Click(object sender, EventArgs e)
        {
            // Handles explicit refresh trigger
        }
    }
}