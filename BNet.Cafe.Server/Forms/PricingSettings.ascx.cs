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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadPricingList();
            }
        }

        private void LoadPricingList()
        {
            DataTable dt = pricingRepository.GetAll();
            GridView_Pricing.DataSource = dt;
            GridView_Pricing.DataBind();
        }

        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            string id = HiddenField_PriceId.Value;
            string customerType = DropDownList_CustomerType.SelectedValue;
            string minutes = TextBox_Minutes.Text.Trim();
            string price = TextBox_Price.Text.Trim();

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
                LoadPricingList();
                AlertService.ShowAlert(UpdatePanel1, "Pricing rate saved successfully.", "success");
            }
            else
            {
                AlertService.ShowAlert(UpdatePanel1, "Failed to save pricing rate.", "error");
            }
        }

        protected void GridView_Pricing_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditRate")
            {
                string priceId = e.CommandArgument.ToString();
                DataTable dt = pricingRepository.GetById(priceId);

                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    HiddenField_PriceId.Value = row["DBId"].ToString();
                    DropDownList_CustomerType.SelectedValue = row["DBCustomerType"].ToString();
                    TextBox_Minutes.Text = row["DBMinutes"].ToString();
                    TextBox_Price.Text = row["DBPrice"].ToString();
                    LinkButton_Submit.Text = "<i class=\"fa fa-save\"></i> Update Pricing Rule";
                }
            }
            else if (e.CommandName == "DeleteRate")
            {
                string priceId = e.CommandArgument.ToString();
                bool isDeleted = pricingRepository.Delete(priceId);

                if (isDeleted)
                {
                    ClearForm();
                    LoadPricingList();
                    AlertService.ShowAlert(UpdatePanel1, "Pricing rate deleted successfully.", "success");
                }
                else
                {
                    AlertService.ShowAlert(UpdatePanel1, "Failed to delete pricing rate.", "error");
                }
            }
        }

        protected void LinkButton_Cancel_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            HiddenField_PriceId.Value = "0";
            TextBox_Minutes.Text = string.Empty;
            TextBox_Price.Text = string.Empty;
            DropDownList_CustomerType.SelectedIndex = 0;
            LinkButton_Submit.Text = "<i class=\"fa fa-save\"></i> Save Pricing Rule";
        }
    }
}