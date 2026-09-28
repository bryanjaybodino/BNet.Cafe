using BNet.Cafe.Server.OAuth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Web.UI;

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

                // Validate code presence and state integrity/expiration
                if (string.IsNullOrEmpty(code) || !ValidateOAuthState(returnedState))
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
                string role = "";
                string userId = "";
                if (dataTable.Rows.Count > 0)
                {
                    role = dataTable.Rows[0]["DBRole"].ToString().ToUpper();
                    userId = dataTable.Rows[0]["DBId"].ToString().ToUpper();
                }
                else
                {
                    users.Create(userInfo.email, userInfo.email, userInfo.name, role);
                }

                // Store user details
                Session["GoogleUser"] = userInfo;
                Sessions.User userCookies = new Sessions.User();
                userCookies.createCookies(userInfo.email, userInfo.name, role);

                if (role != ConstantData.UserType.Admin)
                {
                    Response.Redirect($"Portal.aspx", false);
                }
                else
                {
                    Response.Redirect("BNetPage.aspx", false);
                }
            }
            catch (Exception ex)
            {
                Response.Redirect("Login.aspx?error=" + Uri.EscapeDataString(ex.Message), false);
            }
        }

        /// <summary>
        /// Validates encrypted state parameter without requiring ASP.NET Session
        /// </summary>
        private bool ValidateOAuthState(string returnedState)
        {
            if (string.IsNullOrEmpty(returnedState))
            {
                return false;
            }

            try
            {
                byte[] decodedBytes = MachineKey.Decode(returnedState, MachineKeyProtection.Encryption);
                if (decodedBytes == null)
                {
                    return false;
                }

                string statePayload = Encoding.UTF8.GetString(decodedBytes);
                string[] parts = statePayload.Split('|');

                if (parts.Length < 2)
                {
                    return false;
                }

                long ticks = long.Parse(parts[0]);
                DateTime createdAt = new DateTime(ticks, DateTimeKind.Utc);

                // Reject state tokens older than 10 minutes
                if (DateTime.UtcNow - createdAt > TimeSpan.FromMinutes(10))
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
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

                var serializer = new JavaScriptSerializer();
                var tokenData = serializer.Deserialize<Dictionary<string, object>>(responseJson);

                if (tokenData != null && tokenData.ContainsKey("access_token"))
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