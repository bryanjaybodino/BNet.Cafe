using BNet.Cafe.Server.Services;
using System;
using System.Web.UI;

namespace BNet.Cafe.Server.Forms
{
    public partial class ComputerCreate : System.Web.UI.UserControl
    {
        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            ComputerService computerService = new ComputerService();
            bool isComputerNameValid = computerService.Create(TextBox_ComputerName.Text);

            if (isComputerNameValid)
            {
                string script = @"
                    Sys.Application.add_load(function() {
                        ShowAlert('Computer successfully added.', 'success');
                        setTimeout(function(){ 
                            window.location.href = 'BNetPage.aspx?Form=Computers'; 
                        }, 1500);
                    });";

                ScriptManager.RegisterStartupScript(
                    UpdatePanel1,
                    UpdatePanel1.GetType(),
                    "SuccessAlert",
                    script,
                    true
                );
            }
            else
            {
                string script = @"
                    Sys.Application.add_load(function() {
                        ShowAlert('Failed to add computer. The name might already exist.', 'error');
                    });";

                ScriptManager.RegisterStartupScript(
                    UpdatePanel1,
                    UpdatePanel1.GetType(),
                    "ErrorAlert",
                    script,
                    true
                );
            }
        }
    }
}