using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using System;
using System.Configuration;
using System.Data;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace BNet.Cafe.Server
{
    public partial class Login : System.Web.UI.Page
    {
        Sessions.User userCookies = new Sessions.User();
        private readonly Users _usersRepository = new Users();
        protected void Page_Load(object sender, EventArgs e)
        {
            // Prevent caching
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            Response.AppendHeader("Pragma", "no-cache");

            if (!IsPostBack)
            {
                FileJsHelpler.BundleAddScripts(ScriptManager1, "Login/Script");

                // Check if user is already logged in
                if (userCookies.count > 0)
                {
                    Response.Redirect("BNetPage.aspx", false);
                    return;
                }

                // Check for errors passed in query string (e.g. OAuth failures)
                if (!string.IsNullOrEmpty(Request.QueryString["error"]))
                {
                    string errorMsg = Request.QueryString["error"];
                    string script = $"showError('{HttpUtility.JavaScriptStringEncode(errorMsg)}');";
                    ScriptManager.RegisterStartupScript(this, GetType(), "ShowError", script, true);
                }
            }
        }

        protected void LinkButton_Login_Click(object sender, EventArgs e)
        {
            string email = TextBox_Email.Text.Trim();
            string password = TextBox_Password.Text;

            DataTable dt = _usersRepository.GetLogin(email, password);

            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                string Id = row["DBId"].ToString();
                string Name = row["DBName"].ToString();
                string Email = row["DBEmail"].ToString();
                string Role = row["DBRole"].ToString();
                string DateCreated = row["DBDateCreated"].ToString();
                string TimeCreated = row["DBTimeCreated"].ToString();
                string IsDeleted = Convert.ToBoolean(row["DBIsDeleted"]).ToString();
                string TotalDuration = Convert.ToInt32(row["DBTotalDuration"]).ToString();
                string FormattedTotalDuration = row["DBFormattedTotalDuration"].ToString();
                userCookies.createCookies(Email, Name, Role);

                if (Role == "USER")
                {
                    Response.Redirect($"Portal.aspx", false);
                }
                else
                {
                    Response.Redirect("BNetPage.aspx", false);
                }
            }
            else
            {
                string script = "showError('Invalid email or password.');";
                ScriptManager.RegisterStartupScript(this, GetType(), "LoginError", script, true);
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
                // Encrypt timestamp + nonce into state token
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
                string script = $"showError('{HttpUtility.JavaScriptStringEncode(ex.Message)}');";
                ScriptManager.RegisterStartupScript(this, GetType(), "OAuthError", script, true);
            }
        }
    }
}