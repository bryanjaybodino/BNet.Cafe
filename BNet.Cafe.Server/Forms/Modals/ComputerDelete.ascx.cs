using BNet.Cafe.Server.Services;
using System;
using System.Web.UI;

namespace BNet.Cafe.Server.Forms.Modals
{
    public partial class ComputerDelete : System.Web.UI.UserControl
    {
        protected void LinkButton_ConfirmDelete_Click(object sender, EventArgs e)
        {
            // Verify that the postback target is actually this button
            string eventTarget = Request.Form["__EVENTTARGET"] ?? string.Empty;
            if (!eventTarget.Contains(LinkButton_ConfirmDelete.ID))
            {
                return;
            }
            Repositories.Computers computers = new Repositories.Computers();
            bool isSuccess = computers.Delete(HiddenField_DeleteId.Value);

            if (isSuccess)
            {
                AlertService.ShowAlert(this, "Computer successfully deleted.", "success");
            }
            else
            {
                AlertService.ShowAlert(this, "Failed to delete.", "error");
            }
        }
    }
}