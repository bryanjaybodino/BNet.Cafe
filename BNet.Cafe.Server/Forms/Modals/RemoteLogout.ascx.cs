using BNet.Cafe.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server.Forms.Modals
{
    public partial class RemoteLogout : System.Web.UI.UserControl
    {     
        protected void LinkButton_ConfirmLogout_Click(object sender, EventArgs e)
        {
            RemoteMessagingService.LogoutPC(this, HiddenField_LogoutClientId.Value);
        }
    }
}