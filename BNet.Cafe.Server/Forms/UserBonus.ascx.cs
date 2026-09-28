using BNet.Cafe.Server.Ashx;
using BNet.Cafe.Server.ConstantData;
using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server.Forms
{
    public partial class UserBonus : System.Web.UI.UserControl
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
                HiddenField_Role.Value = role;
                double totalMinutes = balances.GetBalanceByUserId(userId);
                Label_CurrentBalance.Text = $"{totalMinutes} Mins";

                if (role == ConstantData.UserType.Admin)
                {
                    AlertService.ShowAlert(this, "This account is admin no need to top-up", "warning");
                    Panel_Form.Enabled = false;
                    LinkButton_Submit.Visible = false;
                    return;
                }

                var pricingRates = new Repositories.PricingRates();
                var data = pricingRates.GetAll(role, 0);
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
            string duration = CalculateDurationFromAmountHandler.CalculateDuration(Convert.ToDecimal(amount), HiddenField_Role.Value).ToString();
            string description = TextBox_Description.Text.Trim();

            if (string.IsNullOrEmpty(description))
            {
                description = "Bonus Credit";
            }

            bool isSuccess = balances.Create(userId, duration, "0", description);

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