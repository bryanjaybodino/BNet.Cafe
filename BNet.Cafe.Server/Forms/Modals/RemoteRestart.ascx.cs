using BNet.Cafe.Server.Services;
using System;
using System.Web.UI;

namespace BNet.Cafe.Server.Forms.Modals
{
    public partial class RemoteRestart : System.Web.UI.UserControl
    {
        protected void LinkButton_ConfirmRestart_Click(object sender, EventArgs e)
        {
            string eventTarget = Request.Form["__EVENTTARGET"] ?? string.Empty;
            if (!eventTarget.Contains(LinkButton_ConfirmRestart.ID))
            {
                return;
            }

            string clientName = HiddenField_RestartClientId.Value;

            if (!string.IsNullOrEmpty(clientName))
            {
                RemoteMessagingService.RestartPC(this, clientName);
            }
            else
            {
                AlertService.ShowAlert(this, "Failed to send restart command.", "warning");
            }
        }
    }
}