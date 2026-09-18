using System;
using System.Web.UI;
using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;

namespace BNet.Cafe.Server.Forms.Modals
{
    public partial class PricingSettingDelete : UserControl
    {
        protected void LinkButton_ConfirmDelete_Click(object sender, EventArgs e)
        {
            string eventTarget = Request.Form["__EVENTTARGET"] ?? string.Empty;
            if (!eventTarget.Contains(LinkButton_ConfirmDelete.ID))
            {
                return;
            }

            PricingRates pricingRepository = new PricingRates();
            bool isSuccess = pricingRepository.Delete(HiddenField_DeletePriceId.Value);

            if (isSuccess)
            {
                AlertService.ShowAlert(this, "Pricing rule deleted successfully.", "success");
            }
            else
            {
                AlertService.ShowAlert(this, "Failed to delete pricing rule.", "error");
            }
        }
    }
}