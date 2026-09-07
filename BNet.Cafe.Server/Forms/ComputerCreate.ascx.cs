using BNet.Cafe.Server.Services;
using System;
using System.Web.UI;

namespace BNet.Cafe.Server.Forms
{
    public partial class ComputerCreate : System.Web.UI.UserControl
    {
        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            Repositories.Computers computers = new Repositories.Computers();
            bool isSuccess = computers.Create(TextBox_ComputerName.Text);

            if (isSuccess)
            {
                AlertService.ShowAlert(UpdatePanel1, "Computer successfully added.", "success", "BNetPage.aspx?Form=Computers");
            }
            else
            {
                AlertService.ShowAlert(UpdatePanel1, "Failed to add computer. The name might already exist.", "error");
            }
        }
    }
}