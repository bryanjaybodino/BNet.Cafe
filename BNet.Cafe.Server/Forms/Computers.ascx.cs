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
        ComputerService computerService = new ComputerService();
        protected void Page_PreRender(object sender, EventArgs e)
        {
            GridViewTable.DataSource = computerService.GetAll(TextBox_Search.Text);
            GridViewTable.DataBind();

        }
    }
}