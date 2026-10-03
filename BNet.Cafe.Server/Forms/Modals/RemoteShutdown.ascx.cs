using BNet.Cafe.Server.Services;
using System;
using System.Web.UI;

namespace BNet.Cafe.Server.Forms.Modals
{
    public partial class RemoteShutdown : System.Web.UI.UserControl
    {
        protected void LinkButton_ConfirmShutdown_Click(object sender, EventArgs e)
        {
            string eventTarget = Request.Form["__EVENTTARGET"] ?? string.Empty;
            if (!eventTarget.Contains(LinkButton_ConfirmShutdown.ID))
            {
                return;
            }

            string clientName = HiddenField_ShutdownClientId.Value;

            if (!string.IsNullOrEmpty(clientName))
            {
                // Assuming RemoteMessagingService follows the same interface pattern as LogoutPC
                RemoteMessagingService.ShutdownPC(this, clientName);
            }
            else
            {
                AlertService.ShowAlert(this, "Failed to send shutdown command.", "warning");
            }
        }
    }
}