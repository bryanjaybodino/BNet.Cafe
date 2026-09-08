using BNet.Cafe.Server.Repositories;
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
            ClientData clientData = new ClientData();
            var liveData = clientData.FetchData();
            for (int i = 0; i < GridViewTable.Rows.Count; i++)
            {
                Label Label_DBComputerName = (Label)GridViewTable.Rows[i].FindControl("Label_DBComputerName");
                var fetchData = liveData.Where(x => x.ClientName == Label_DBComputerName.Text).FirstOrDefault();

                fetchData.

            }
        }
        protected void GridViewTable_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridViewTable.PageIndex = e.NewPageIndex;
        }
    }
}