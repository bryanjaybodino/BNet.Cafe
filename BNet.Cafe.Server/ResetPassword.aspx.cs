using BNet.Cafe.Server.Databases;
using BNet.Cafe.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server
{
    public partial class ResetPassword : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string userIdParam = SecuredDataService.Decrypted(Request.QueryString["userId"]);
                string token = Request.QueryString["token"];

                // 1. Initial request from WinForms WebView (?userId=1)
                if (!string.IsNullOrEmpty(userIdParam))
                {
                    string newToken = PasswordResetHelper.GenerateResetUrl(userIdParam);
                    Response.Redirect(newToken, true);
                    return;
                }

                // 2. Validate token on redirected reload (?token=...)
                if (string.IsNullOrEmpty(token))
                {
                    Response.Redirect("~/ExpiredOrInvalid.aspx");
                    return;
                }

                bool isValid = PasswordResetHelper.TryValidateToken(token, out string userId, out bool isExpired);

                if (isExpired || !isValid)
                {
                    Response.Redirect("~/ExpiredOrInvalid.aspx");
                    return;
                }

                ViewState["ResetUserId"] = userId;
            }
        }

        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            string userId = ViewState["ResetUserId"] as string;

            if (string.IsNullOrEmpty(userId))
            {
                Response.Redirect("~/ExpiredOrInvalid.aspx");
                return;
            }

            var scripts = new Dictionary<string, string>();
            DBScriptService dBScriptService = new DBScriptService();
            string DBId = dBScriptService.CleanUpToUpper(userId);
            string DBPassword = dBScriptService.CleanUpToUpper(TextBox_Password.Text);

            Repositories.Users users = new Repositories.Users();
            var isSuccess = users.Update(DBId, "", DBPassword, "", "");

            if (isSuccess)
            {
                AlertService.ShowAlert(UpdatePanel1, "Password updated successfully.", "success");
            }
            else
            {
                AlertService.ShowAlert(UpdatePanel1, "Failed to update password. Please try again.", "error");
            }
        }
    }
}