using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server.Forms
{
    public partial class Points : System.Web.UI.UserControl
    {
        private readonly BNet.Cafe.Server.Repositories.Points pointsRepo = new BNet.Cafe.Server.Repositories.Points();

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

            var count = pointsRepo.GetCount(userId, dateRange);
            Label_TotalPointsLogs.Text = count.Total;
            Label_Users.Text = count.Users;
            Label_TotalPoints.Text = count.TotalPoints;

            DataTable data = pointsRepo.GetAll(search, userId, dateRange, GridViewTemplateService.GetPaginationIndex(GridViewTable));
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