using BNet.Cafe.Server.Services;
using System;
using System.Configuration;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace BNet.Cafe.Server
{
    public partial class Login : System.Web.UI.Page
    {
        Sessions.User userCookies = new Sessions.User();

        protected void Page_Load(object sender, EventArgs e)
        {
            // Prevent caching
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            Response.AppendHeader("Pragma", "no-cache");

            if (!IsPostBack)
            {
                // Check if user already logged in
                if (userCookies.count > 0)
                {
                    Response.Redirect("BNetPage.aspx", false);
                }
            }
        }

        protected void btnGoogleSignIn_Click(object sender, EventArgs e)
        {
            InitiateGoogleOAuth();
        }

        private void InitiateGoogleOAuth()
        {
            try
            {
                // Encrypt timestamp + nonce into state token to bypass WebBrowser/WebView session loss
                string stateRaw = $"{DateTime.UtcNow.Ticks}|{Guid.NewGuid()}";
                string encryptedState = MachineKey.Encode(Encoding.UTF8.GetBytes(stateRaw), MachineKeyProtection.Encryption);

                // Build OAuth authorization URL
                string clientId = ConfigurationManager.AppSettings["Google_ClientId"];
                string redirectUri = ConfigurationManager.AppSettings["Google_RedirectUri"];
                string authEndpoint = ConfigurationManager.AppSettings["Google_AuthorizationEndpoint"];

                string authUrl = string.Format(
                    "{0}?client_id={1}&redirect_uri={2}&response_type=code&scope={3}&state={4}&prompt={5}",
                    authEndpoint,
                    Uri.EscapeDataString(clientId),
                    Uri.EscapeDataString(redirectUri),
                    Uri.EscapeDataString("openid email profile"),
                    Uri.EscapeDataString(encryptedState),
                    Uri.EscapeDataString("select_account")
                );

                Response.Redirect(authUrl, false);
            }
            catch (Exception ex)
            {
                Response.Redirect("Login.aspx?error=" + Uri.EscapeDataString(ex.Message), false);
            }
        }
    }
}