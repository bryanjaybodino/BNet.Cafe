using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using BNet.Cafe.Server.Sessions;
using System;
using System.Web.Services;

namespace BNet.Cafe.Server
{
    public partial class ClientChat : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                FileCssHelper.BundleCss();
                FileJsHelpler.BundleAddScripts(ScriptManager1, "Pages/RemoteMessaging");
                FileJsHelpler.BundleAddScripts(ScriptManager1, "ClientChat/Script");
            }
        }
    }
}