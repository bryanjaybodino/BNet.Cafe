using BNet.Cafe.Server.Ashx;
using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services.Description;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server.Forms.Modals
{
    public partial class RemoteLogout : System.Web.UI.UserControl
    {
        protected void LinkButton_ConfirmLogout_Click(object sender, EventArgs e)
        {
            // Verify that the postback target is actually this button
            string eventTarget = Request.Form["__EVENTTARGET"] ?? string.Empty;
            if (!eventTarget.Contains(LinkButton_ConfirmLogout.ID))
            {
                return;
            }

            bool isOpenTime = (HiddenField_IsOpenTime.Value == "TRUE");
            if (isOpenTime)
            {
                Repositories.Rentals rentals = new Repositories.Rentals();
                string id = HiddenField_LogoutId.Value;
                var rental = rentals.GetByComputerId(id);
                for (int i = 0; i < rental.Rows.Count; i++)
                {
                    string rentalId = rental.Rows[i]["DBId"].ToString();
                    string userId = rental.Rows[i]["DBUserId"].ToString();

                    ClientData clientData = new ClientData();
                    var liveData = clientData.FetchData();
                    var fetchData = liveData.FirstOrDefault(x => x.ClientName == HiddenField_LogoutClientId.Value);

                    if (fetchData != null)
                    {
                        if (!string.IsNullOrEmpty(fetchData.TimeStart) && DateTime.TryParse(fetchData.TimeStart, out DateTime start))
                        {
                            TimeSpan duration = TimeService.Get() - start;
                            int totalTime = (int)duration.TotalMinutes;
                            double billing = CalculateAmountFromDuration.CalculatePrice(totalTime);
                            bool isSuccess = rentals.Update(rentalId, id, userId, totalTime.ToString().Replace(",", ""), billing.ToString().Replace(",", ""));
                            if (isSuccess)
                            {
                                RemoteMessagingService.LogoutPC(this, HiddenField_LogoutClientId.Value);
                            }
                            else
                            {
                                AlertService.ShowAlert(this, "Failed to logout.", "warning");
                            }
                        }
                    }
                }
            }
            else
            {
                RemoteMessagingService.LogoutPC(this, HiddenField_LogoutClientId.Value);
            }
        }
    }
}