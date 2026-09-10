using BNet.Cafe.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server.Forms.Modals
{
    public partial class RemoteOpenTime : System.Web.UI.UserControl
    {
        protected void LinkButton_ConfirmOpenTime_Click(object sender, EventArgs e)
        {
            // Verify that the postback target is actually this button
            string eventTarget = Request.Form["__EVENTTARGET"] ?? string.Empty;
            if (!eventTarget.Contains(LinkButton_ConfirmOpenTime.ID))
            {
                return;
            }

            string computerId = Request.QueryString["id"];
            string computerName = HiddenField_OpenTimeClientId.Value.Trim();
            string customerId = HiddenField_OpenTimeCustomerName.Value;
            string rentalId = HiddenField_OpenTimeRentalId.Value;
            string command = (HiddenField_OpenTimeStatus.Value == "Occupied") ? ConstantData.RentalCommand.UPDATE : ConstantData.RentalCommand.CREATE;

            // Extract and parse form controls safely
            int duration = 0;
            decimal amount = 0;

            Repositories.Rentals rentals = new Repositories.Rentals();

            bool isSuccess = false;
            if (command == ConstantData.RentalCommand.UPDATE)
            {
                isSuccess = rentals.Update(rentalId, computerId, customerId, duration.ToString().Replace(",", ""), amount.ToString().Replace(",", ""));
            }
            else
            {
                isSuccess = rentals.Create(computerId, customerId, duration.ToString().Replace(",", ""), amount.ToString().Replace(",", ""));
            }

            if (isSuccess)
            {
                RemoteMessagingService.SendToPC(this.Page, computerName, customerId, duration, amount, command);
                AlertService.ShowAlert(this.Page, "Rental session successfully created.", "success", "BNetPage.aspx?Form=Computers");
            }
            else
            {
                AlertService.ShowAlert(this.Page, "Failed to create rental session.", "error");
            }

        }
    }
}