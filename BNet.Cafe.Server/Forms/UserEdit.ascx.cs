using BNet.Cafe.Server.Services;
using System;
using System.Data;
using System.Web.UI;

namespace BNet.Cafe.Server.Forms
{
    public partial class UserEdit : System.Web.UI.UserControl
    {
        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Repositories.Users users = new Repositories.Users();
                DataTable user = users.GetById(Request.QueryString["id"]);
                if (user != null && user.Rows.Count > 0)
                {
                    TextBox_Name.Text = user.Rows[0]["DBName"].ToString();
                    TextBox_Email.Text = user.Rows[0]["DBEmail"].ToString();
                    TextBox_Password.Text = user.Rows[0]["DBPassword"].ToString();

                    string role = user.Rows[0]["DBRole"].ToString().ToUpper();
                    if (DropDownList_Role.Items.FindByValue(role) != null)
                    {
                        DropDownList_Role.SelectedValue = role;
                    }
                }
            }
        }

        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            Repositories.Users users = new Repositories.Users();
            string id = Request.QueryString["id"];
            bool isSuccess = users.Update(id, TextBox_Email.Text, TextBox_Password.Text, TextBox_Name.Text, DropDownList_Role.SelectedValue);

            if (isSuccess)
            {
                AlertService.ShowAlert(UpdatePanel1, "User successfully updated.", "success", "BNetPage.aspx?Form=Users");
            }
            else
            {
                AlertService.ShowAlert(UpdatePanel1, "Failed to update user details.", "error");
            }
        }
    }
}