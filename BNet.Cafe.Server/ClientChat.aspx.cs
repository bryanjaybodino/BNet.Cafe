using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using BNet.Cafe.Server.Sessions;
using System;

namespace BNet.Cafe.Server
{
    public partial class ClientChat : System.Web.UI.Page
    {
        private readonly User userSession = new User();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                FileCssHelper.BundleCss();
                FileJsHelpler.BundleBNetPageScripts(ScriptManager1);

                FileJsHelpler.BundleAddScripts(ScriptManager1, "Pages/RemoteMessaging");
            }
        }

        protected void LinkButton_SaveMessage_Click(object sender, EventArgs e)
        {
            string message = TextBox_ChatMessage.Text;
            string computerName = TextBox_ComputerName.Text;
            string userId = userSession.user_id;

            if (!string.IsNullOrEmpty(message))
            {
                ChatMessages repo = new ChatMessages();
                repo.Create(message, computerName, userId);
            }

            // Reset text field
            TextBox_ChatMessage.Text = string.Empty;
        }
    }
}