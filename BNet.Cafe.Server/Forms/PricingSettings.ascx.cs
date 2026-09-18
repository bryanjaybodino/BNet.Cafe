using System;
using System.Data;
using System.Web.UI.WebControls;
using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;

namespace BNet.Cafe.Server.Forms
{
    public partial class PricingSettings : System.Web.UI.UserControl
    {
        private readonly Repositories.PricingRates pricingRepository = new Repositories.PricingRates();

        protected void Page_PreRender(object sender, EventArgs e)
        {
            LoadPricingList();
        }

        private void LoadPricingList()
        {
            DataTable dt = pricingRepository.GetAll(DropDownList_CustomerType.SelectedValue);
            GridView_Pricing.DataSource = dt;
            GridView_Pricing.DataBind();
            Label_TotalRules.Text =GridView_Pricing.Rows.Count.ToString();
        }

        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            string id = HiddenField_PriceId.Value;
            string customerType = DropDownList_CustomerType.SelectedValue;
            string price = TextBox_Price.Text.Trim();

            // Parse Hours and Minutes inputs safely
            int.TryParse(TextBox_Hours.Text.Trim(), out int hours);
            int.TryParse(TextBox_Minutes_Only.Text.Trim(), out int minutesOnly);

            // Calculate total minutes
            int totalMinutes = (hours * 60) + minutesOnly;
            string minutes = totalMinutes.ToString();

            // Validation
            if (string.IsNullOrEmpty(customerType) || totalMinutes <= 0 || string.IsNullOrEmpty(price))
            {
                AlertService.ShowAlert(UpdatePanel1, "Please fill in all required fields with valid duration.", "warning");
                return;
            }

            bool isSuccess;

            if (id == "0" || string.IsNullOrEmpty(id))
            {
                isSuccess = pricingRepository.Create(customerType, minutes, price);
            }
            else
            {
                isSuccess = pricingRepository.Update(id, customerType, minutes, price);
            }

            if (isSuccess)
            {
                ClearForm();
                string actionMsg = (id == "0" || string.IsNullOrEmpty(id)) ? "added" : "updated";
                AlertService.ShowAlert(UpdatePanel1, $"Pricing rule {actionMsg} successfully.", "success");
            }
            else
            {
                AlertService.ShowAlert(UpdatePanel1, "Failed to save pricing rule or it's already exist. Please try again.", "error");
            }
        }


        protected void LinkButton_Cancel_Click(object sender, EventArgs e)
        {
            ClearForm();
        }
        private void ClearForm()
        {
            HiddenField_PriceId.Value = "0";
            TextBox_Hours.Text = string.Empty; 
            TextBox_Minutes_Only.Text = string.Empty;
            TextBox_Price.Text = string.Empty;
            DropDownList_CustomerType.SelectedIndex = 0;
            LinkButton_Submit.Text = "<i class=\"fa fa-save\"></i> Save Rule";
        }

        /// <summary>
        /// Format customer type for display
        /// </summary>
        protected string FormatCustomerType(string customerType)
        {
            switch (customerType?.ToUpper())
            {
                case "GUEST":
                    return "Guest / Walk-In";
                case "MEMBER":
                    return "Member";
                default:
                    return customerType;
            }
        }

        /// <summary>
        /// Format minutes to hours and minutes display
        /// </summary>
        protected string FormatDuration(int totalMinutes)
        {
            if (totalMinutes <= 0) return "0 mins";

            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;

            if (hours > 0 && minutes > 0)
                return $"{hours}h {minutes}m";
            else if (hours > 0)
                return $"{hours} hr" + (hours > 1 ? "s" : "");
            else
                return $"{minutes} min" + (minutes > 1 ? "s" : "");
        }
    }
}