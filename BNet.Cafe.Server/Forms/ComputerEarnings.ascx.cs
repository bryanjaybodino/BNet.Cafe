using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server.Forms
{
    public partial class ComputerEarnings : System.Web.UI.UserControl
    {
        private readonly Rentals rentals = new Rentals();

        protected void Page_PreRender(object sender, EventArgs e)
        {
            string search = "";
            string computerId = Request.QueryString["id"];

            var count = rentals.GetCount(computerId);
            Label_TotalTransactions.Text = count.Total;
            Label_Users.Text = count.Users;
            Label_WalkIn.Text = count.WalkIn;
            Label_Income.Text = count.Income;

            DataTable data = rentals.GetAll(search, computerId, GridViewTemplateService.GetPaginationIndex(GridViewTable));
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
            // Handles explicit refresh trigger without re-executing stale postback actions
        }
    }
}