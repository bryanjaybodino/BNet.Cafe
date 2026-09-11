using BNet.Cafe.Server.OAuth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server
{
    public partial class Callback : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string code = Request.QueryString["code"];
                string error = Request.QueryString["error"];

                if (!string.IsNullOrEmpty(error))
                {
                    Response.Redirect("Login.aspx?error=" + Uri.EscapeDataString(error), false);
                    return;
                }

                string returnedState = Request.QueryString["state"];
                string storedState = Session["oauth_state"] as string;

                if (string.IsNullOrEmpty(code) || returnedState != storedState)
                {
                    Response.Redirect("Login.aspx?error=invalid_state", false);
                    return;
                }


                // Token exchange
                string accessToken = ExchangeCodeForToken(code);
                if (string.IsNullOrEmpty(accessToken))
                {
                    Response.Redirect("Login.aspx?error=token_exchange_failed", false);
                    return;
                }

                // Get user info
                GoogleUserInfo userInfo = GetUserInfoFromGoogle(accessToken);
                if (userInfo == null)
                {
                    Response.Redirect("Login.aspx?error=no_email", false);
                    return;
                }


                Repositories.Users users = new Repositories.Users();
                DataTable dataTable = users.GetByEmail(userInfo.email);
                string role = "USER";
                string userId = "";
                if (dataTable.Rows.Count > 0)
                {
                    role = dataTable.Rows[0]["DBRole"].ToString().ToUpper();
                    userId = dataTable.Rows[0]["DBId"].ToString().ToUpper();
                }
                else
                {
                    users.Create(userInfo.email, userInfo.name, role);
                }

                // Store in session
                Session["GoogleUser"] = userInfo;
                Sessions.User userCookies = new Sessions.User();
                userCookies.createCookies(userInfo.email, userInfo.name, role);

                if (role == "USER")
                {
                    Repositories.Balances balances = new Repositories.Balances();
                    double balance = balances.GetBalanceByUserId(userId);
                    balances.Create(userId, "-" + balance.ToString(), "0", "Logging-In");
                    Response.Redirect($"Portal.aspx?UserId={userId}&Name={userInfo.name}&Email={userInfo.email}&Balance={balance}", false);
                }
                else
                {
                    // Redirect to dashboard
                    Response.Redirect("BNetPage.aspx", false);
                }
            }
            catch (Exception ex)
            {
                Response.Redirect("Login.aspx?error=" + Uri.EscapeDataString(ex.Message), false);
            }
        }

        /// <summary>
        /// Exchange authorization code for access token
        /// </summary>
        private string ExchangeCodeForToken(string code)
        {
            try
            {
                string clientId = ConfigurationManager.AppSettings["Google_ClientId"];
                string clientSecret = ConfigurationManager.AppSettings["Google_ClientSecret"];
                string redirectUri = ConfigurationManager.AppSettings["Google_RedirectUri"];
                string tokenEndpoint = ConfigurationManager.AppSettings["Google_TokenEndpoint"];

                string postData = string.Format(
                    "client_id={0}&client_secret={1}&code={2}&redirect_uri={3}&grant_type=authorization_code",
                    Uri.EscapeDataString(clientId),
                    Uri.EscapeDataString(clientSecret),
                    Uri.EscapeDataString(code),
                    Uri.EscapeDataString(redirectUri)
                );

                byte[] postBytes = Encoding.UTF8.GetBytes(postData);
                var request = (HttpWebRequest)WebRequest.Create(tokenEndpoint);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Accept = "application/json";
                request.ContentLength = postBytes.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(postBytes, 0, postBytes.Length);
                }

                string responseJson;
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseJson = reader.ReadToEnd();
                }

                // Parse JSON response
                var serializer = new JavaScriptSerializer();
                var tokenData = serializer.Deserialize<Dictionary<string, object>>(responseJson);

                if (tokenData.ContainsKey("access_token"))
                {
                    return tokenData["access_token"].ToString();
                }

                return null;
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null)
                {
                    using (var errorStream = webEx.Response.GetResponseStream())
                    using (var reader = new StreamReader(errorStream))
                    {
                        string errorResponse = reader.ReadToEnd();
                        // Log error if needed
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Get user info from Google API
        /// </summary>
        private GoogleUserInfo GetUserInfoFromGoogle(string accessToken)
        {
            try
            {
                string userInfoEndpoint = ConfigurationManager.AppSettings["Google_UserInfoEndpoint"];

                var request = (HttpWebRequest)WebRequest.Create(userInfoEndpoint);
                request.Method = "GET";
                request.Accept = "application/json";
                request.Headers.Add("Authorization", "Bearer " + accessToken);

                string responseJson;
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseJson = reader.ReadToEnd();
                }

                var serializer = new JavaScriptSerializer();
                var userInfo = serializer.Deserialize<GoogleUserInfo>(responseJson);

                return userInfo;
            }
            catch
            {
                return null;
            }
        }
    }
}