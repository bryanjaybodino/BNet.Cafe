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
                HiddenField_Role.Value = role;

                double totalMinutes = balances.GetBalanceByUserId(userId);
                Label_CurrentBalance.Text = $"{totalMinutes} Mins";

                if (role == ConstantData.UserType.Admin)
                {
                    AlertService.ShowAlert(this, "This account is admin no need to manage top-up or deduction.", "warning");
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

        protected void DropDownList_Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isDeduction = DropDownList_Type.SelectedValue == "Deduction";

            if (isDeduction)
            {
                TextBox_Description.Text = "Balance Deduction";
                LinkButton_Submit.CssClass = "btn btn-danger";
            }
            else
            {
                TextBox_Description.Text = "Top-Up Load Credit";
                LinkButton_Submit.CssClass = "btn btn-primary";
            }
        }

        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            string userId = Request.QueryString["id"];
            bool isDeduction = DropDownList_Type.SelectedValue == "Deduction";

            if (!decimal.TryParse(TextBox_Amount.Text.Trim(), out decimal parsedAmount) || parsedAmount < 0)
            {
                AlertService.ShowAlert(UpdatePanel1, "Please enter a valid amount.", "warning");
                return;
            }

            double durationVal = CalculateDurationFromAmountHandler.CalculateDuration(parsedAmount, HiddenField_Role.Value);
            double currentBalance = balances.GetBalanceByUserId(userId);

            if (isDeduction)
            {
                if (durationVal > currentBalance)
                {
                    AlertService.ShowAlert(UpdatePanel1, $"Cannot deduct more than user's current balance ({currentBalance} Mins).", "warning");
                    return;
                }

                // Invert values to negative for deduction
                durationVal = -Math.Abs(durationVal);
                parsedAmount = -Math.Abs(parsedAmount);
            }

            string duration = durationVal.ToString();
            string amount = parsedAmount.ToString();
            string description = TextBox_Description.Text.Trim();

            if (string.IsNullOrEmpty(description))
            {
                description = isDeduction ? "Balance Deduction" : "Top-Up Load Credit";
            }

            bool isSuccess = balances.Create(userId, duration, amount, description);

            if (isSuccess)
            {
                string successMessage = isDeduction
                    ? "Deduction successful! Balance updated."
                    : "Top-Up successful! Balance added to user account.";

                AlertService.ShowAlert(UpdatePanel1, successMessage, "success", "BNetPage.aspx?Form=Users");
            }
            else
            {
                AlertService.ShowAlert(UpdatePanel1, "Failed to process transaction.", "error");
            }
        }
    }
}