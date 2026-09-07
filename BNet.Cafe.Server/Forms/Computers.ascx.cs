using BNet.Cafe.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server.Forms
{
    public partial class Computers : System.Web.UI.UserControl
    {
        Repositories.Computers computers = new Repositories.Computers();

        protected void Page_PreRender(object sender, EventArgs e)
        {
            var data = computers.GetAll(TextBox_Search.Text, GridViewTemplateService.GetPaginationIndex(GridViewTable));
            GridViewTemplateService.SetGridView(GridViewTable, data, Panel_Pagination);
        }
        protected void GridViewTable_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridViewTable.PageIndex = e.NewPageIndex;
        }
    }
}