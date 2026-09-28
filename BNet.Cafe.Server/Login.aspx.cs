using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using System;
using System.Configuration;
using System.Data;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace BNet.Cafe.Server
{
    public partial class Login : System.Web.UI.Page
    {
        IPAddressChecker IPAddressChecker = new IPAddressChecker();
        IPAddressChecker.LockUser LockUser = new IPAddressChecker.LockUser();

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

                // Check lock status on full page load/refresh
                string ip = IPAddressChecker.GetClientIP();
                string key = ip + "_login";
                string ipOnlyKey = ip + "_";

                if (LockUser.IsLocked(key) || LockUser.IsLocked(ipOnlyKey))
                {
                    int secondsLeft = LockUser.GetRemainingLockedMinutes(ipOnlyKey);
                    if (secondsLeft > 0)
                    {
                        string lockScript = $"enforceLoginLock({secondsLeft});";
                        ScriptManager.RegisterStartupScript(this, GetType(), "LoginLockState", lockScript, true);
                    }
                }
            }
        }

        protected void LinkButton_Login_Click(object sender, EventArgs e)
        {
            string ip = IPAddressChecker.GetClientIP();
            string key = ip + "_login";
            string ipOnlyKey = ip + "_";

            // 1. Check if IP/Account is currently locked
            if (LockUser.IsLocked(key) || LockUser.IsLocked(ipOnlyKey))
            {
                int secondsLeft = LockUser.GetRemainingLockedMinutes(key);
                if (secondsLeft <= 0) secondsLeft = LockUser.GetRemainingLockedMinutes(ipOnlyKey);

                ScriptManager.RegisterStartupScript(this, GetType(), "LoginLockEnforce",
                    $"enforceLoginLock({secondsLeft});", true);
                return;
            }

            string email = TextBox_Email.Text.Trim();
            string password = TextBox_Password.Text;

            DataTable dt = _usersRepository.GetLogin(email, password);

            if (dt != null && dt.Rows.Count > 0)
            {
                HttpContext.Current.Cache.Remove(key);
                HttpContext.Current.Cache.Remove(ipOnlyKey);
                DataRow row = dt.Rows[0];
                string Name = row["DBName"].ToString();
                string Email = row["DBEmail"].ToString();
                string Role = row["DBRole"].ToString();

                userCookies.createCookies(Email, Name, Role);

                if (Role == "USER")
                {
                    Response.Redirect("Portal.aspx", false);
                }
                else
                {
                    Response.Redirect("BNetPage.aspx", false);
                }
            }
            else
            {
                // FAILED: Increment attempt counter
                int attempts = LockUser.IncrementAttempt(key);
                int maxAttempts = 5;
                int remainingAttempts = maxAttempts - attempts;

                if (attempts >= maxAttempts)
                {
                    // Lock for 5 minutes (300 seconds)
                    int lockDurationMinutes = 5;
                    LockUser.Lock(key, lockDurationMinutes);
                    LockUser.Lock(ipOnlyKey, lockDurationMinutes);

                    int secondsLeft = LockUser.GetRemainingLockedMinutes(key);
                    if (secondsLeft <= 0) secondsLeft = lockDurationMinutes * 60;

                    ScriptManager.RegisterStartupScript(this, GetType(), "LoginLockEnforce",
                        $"enforceLoginLock({secondsLeft});", true);
                }
                else
                {
                    string alertMsg = $"Invalid email or password. You have {remainingAttempts} attempt(s) remaining.";
                    string script = $"showError('{HttpUtility.JavaScriptStringEncode(alertMsg)}');";
                    ScriptManager.RegisterStartupScript(this, GetType(), "LoginError", script, true);
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
                string stateRaw = $"{DateTime.UtcNow.Ticks}|{Guid.NewGuid()}";
                string encryptedState = MachineKey.Encode(Encoding.UTF8.GetBytes(stateRaw), MachineKeyProtection.Encryption);

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