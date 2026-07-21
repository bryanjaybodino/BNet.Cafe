using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server
{
    public partial class Login : System.Web.UI.Page
    {
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
                if (Session["GoogleUser"] != null)
                {
                    Response.Redirect("BNetPage.aspx", false);
                }
                else
                {
                    try
                    {
                        // Generate CSRF state token
                        string state = Guid.NewGuid().ToString();
                        Session["oauth_state"] = state;
                        Session["oauth_provider"] = "GOOGLE";

                        // Build OAuth authorization URL
                        string clientId = ConfigurationManager.AppSettings["Google_ClientId"];
                        string redirectUri = ConfigurationManager.AppSettings["Google_RedirectUri"];
                        string authEndpoint = ConfigurationManager.AppSettings["Google_AuthorizationEndpoint"];

                        string authUrl = string.Format(
                            "{0}?client_id={1}&redirect_uri={2}&response_type=code&scope={3}&state={4}",
                            authEndpoint,
                            Uri.EscapeDataString(clientId),
                            Uri.EscapeDataString(redirectUri),
                            Uri.EscapeDataString("openid email profile"),
                            Uri.EscapeDataString(state)
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
    }
}