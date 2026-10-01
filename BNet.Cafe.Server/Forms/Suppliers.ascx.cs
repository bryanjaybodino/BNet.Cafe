using BNet.Cafe.Server.Services;
using System;
using System.Data;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server.Forms
{
    public partial class Suppliers : System.Web.UI.UserControl
    {
        private readonly Repositories.Suppliers suppliers = new Repositories.Suppliers();

        protected void Page_PreRender(object sender, EventArgs e)
        {
            string search = TextBox_Search.Text.Trim();
            DataTable data = suppliers.GetAll(search, GridViewTemplateService.GetPaginationIndex(GridViewTable));
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