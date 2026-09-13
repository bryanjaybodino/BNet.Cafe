using BNet.Cafe.Server.Services;
using System;
using System.Web.UI;

namespace BNet.Cafe.Server.Forms
{
    public partial class UserCreate : System.Web.UI.UserControl
    {
        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            Repositories.Users users = new Repositories.Users();
            bool isSuccess = users.Create(TextBox_Email.Text, TextBox_Password.Text, TextBox_Name.Text, DropDownList_Role.SelectedValue);

            if (isSuccess)
            {
                AlertService.ShowAlert(UpdatePanel1, "User successfully added.", "success", "BNetPage.aspx?Form=Users");
            }
            else
            {
                AlertService.ShowAlert(UpdatePanel1, "Failed to add user. The email address might already exist.", "error");
            }
        }
    }
}