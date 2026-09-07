using BNet.Cafe.Server.Services;
using System;
using System.Web.UI;

namespace BNet.Cafe.Server.Forms
{
    public partial class ComputerEdit : System.Web.UI.UserControl
    {
        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Repositories.Computers computers = new Repositories.Computers();
                var computer = computers.GetById(Request.QueryString["id"]);
                for (int i = 0; i < computer.Rows.Count; i++)
                {
                    TextBox_ComputerName.Text = computer.Rows[i]["DBComputerName"].ToString();
                }
            }
        }

        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            Repositories.Computers computers = new Repositories.Computers();
            string id = Request.QueryString["id"];
            bool isSuccess = computers.Update(id, TextBox_ComputerName.Text);

            if (isSuccess)
            {
                AlertService.ShowAlert(UpdatePanel1, "Computer successfully updated.", "success", "BNetPage.aspx?Form=Computers");
            }
            else
            {
                AlertService.ShowAlert(UpdatePanel1, "Failed to update computer. The name might already exist.", "error");
            }
        }
    }
}