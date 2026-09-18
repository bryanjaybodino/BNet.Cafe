using BNet.Cafe.Server.Ashx;
using BNet.Cafe.Server.ConstantData;
using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using System;
using System.Data;
using System.Web.UI;

namespace BNet.Cafe.Server.Forms
{
    public partial class UserTopUp : System.Web.UI.UserControl
    {
        private readonly Repositories.Users users = new Repositories.Users();
        private readonly Balances balances = new Balances();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string userId = Request.QueryString["id"];

                if (string.IsNullOrEmpty(userId))
                {
                    AlertService.ShowAlert(this, "No user specified.", "warning", "BNetPage.aspx?Form=Users");
                    return;
                }

                LoadUserData(userId);
            }
        }

        private void LoadUserData(string userId)
        {
            DataTable userData = users.GetById(userId);

            if (userData != null && userData.Rows.Count > 0)
            {
                Label_UserName.Text = userData.Rows[0]["DBName"].ToString();
                Label_UserEmail.Text = userData.Rows[0]["DBEmail"].ToString();
                string role = userData.Rows[0]["DBRole"].ToString();

                double totalMinutes = balances.GetBalanceByUserId(userId);
                Label_CurrentBalance.Text = $"{totalMinutes} Mins";

                if (role == "ADMIN")
                {
                    AlertService.ShowAlert(this, "This account is admin no need to top-up", "warning");
                    Panel_Form.Enabled = false;
                    LinkButton_Submit.Visible = false;
                    return;
                }

                var pricingRates = new Repositories.PricingRates();
                var data = pricingRates.GetAll(UserType.User, 0);
                if (data.Rows.Count == 0)
                {
                    AlertService.ShowAlert(this, "No active pricing configuration found on your pricing settings. Please contact support if this issue persists.", "danger");
                    Panel_Form.Enabled = false;
                    LinkButton_Submit.Visible = false;
                    return;
                }
            }
            else
            {
                AlertService.ShowAlert(this, "User record not found.", "error", "BNetPage.aspx?Form=Users");
            }
        }

        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            string userId = Request.QueryString["id"];
            string amount = TextBox_Amount.Text.Trim();
            string duration = CalculateDurationFromAmount.CalculateDuration(Convert.ToDecimal(amount)).ToString();
            string description = TextBox_Description.Text.Trim();

            if (string.IsNullOrEmpty(description))
            {
                description = "Top-Up Load Credit";
            }

            bool isSuccess = balances.Create(userId, duration, amount, description);

            if (isSuccess)
            {
                AlertService.ShowAlert(UpdatePanel1, "Top-Up successful! Balance added to user account.", "success", "BNetPage.aspx?Form=Users");
            }
            else
            {
                AlertService.ShowAlert(UpdatePanel1, "Failed to process Top-Up transaction.", "error");
            }
        }
    }
}