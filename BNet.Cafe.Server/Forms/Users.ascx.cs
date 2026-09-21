using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server.Forms
{
    public partial class Users : System.Web.UI.UserControl
    {
        private readonly Repositories.Users users = new Repositories.Users();

        protected void Page_PreRender(object sender, EventArgs e)
        {
            string search = TextBox_Search.Text.Trim();
            DataTable data = users.GetAll(search, GridViewTemplateService.GetPaginationIndex(GridViewTable));
            GridViewTemplateService.SetGridView(GridViewTable, data, Panel_Pagination);
            for (int i = 0; i < GridViewTable.Rows.Count; i++)
            {
                Label Label_DBRole = (Label)GridViewTable.Rows[i].FindControl("Label_DBRole");
                Panel Panel_TopUp = (Panel)GridViewTable.Rows[i].FindControl("Panel_TopUp");
                Panel_TopUp.Visible= (Label_DBRole.Text.ToUpper() == "USER") ; 
                
            }
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