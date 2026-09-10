using BNet.Cafe.Server.Ashx;
using BNet.Cafe.Server.Databases.Tables;
using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server.Forms.Modals
{
    public partial class RemoteTimeTransfer : UserControl
    {
        private Repositories.Computers computers = new Repositories.Computers();
        protected void Page_PreRender(object sender, EventArgs e)
        {
            DropDownList_TargetComputer.Items.Clear();

            ClientData clientData = new ClientData();
            var liveData = clientData.FetchData();
            var availablePcs = liveData.Where(x => string.IsNullOrEmpty(x.TimeStart)).ToList();
            var dataTable = computers.GetAll();

            DropDownList_TargetComputer.Items.Clear();

            foreach (var pc in availablePcs)
            {
                // Find the matching database row by Computer Name
                var row = dataTable.AsEnumerable()
                                   .FirstOrDefault(r => r["DBComputerName"].ToString() == pc.ClientName);

                if (row != null)
                {
                    string computerId = row["DBId"].ToString();
                    DropDownList_TargetComputer.Items.Add(new ListItem(pc.ClientName, computerId));
                }
            }

            bool hasItems = DropDownList_TargetComputer.Items.Count > 0;
            DropDownList_TargetComputer.Enabled = hasItems;
            LinkButton_ConfirmTransfer.Enabled = hasItems;
            Label_NoAvailablePc.Visible = !hasItems;

            if (!hasItems)
            {
                DropDownList_TargetComputer.Items.Add(new ListItem("-- No Available PCs Online --", ""));
            }
        }
        protected void LinkButton_ConfirmTransfer_Click(object sender, EventArgs e)
        {
            string sourcePcName = HiddenField_SourceComputerName.Value;
            string targetPcName = DropDownList_TargetComputer.SelectedItem.Text;
            string targetPcId = DropDownList_TargetComputer.SelectedItem.Value;
            string id = HiddenField_SourceComputerId.Value;

            if (string.IsNullOrEmpty(targetPcName))
            {
                AlertService.ShowAlert(this, "Please select a valid target PC.", "warning");
                return;
            }

            ClientData clientData = new ClientData();
            var liveData = clientData.FetchData();
            var sourceData = liveData.FirstOrDefault(x => x.ClientName == sourcePcName);

            if (sourceData == null || string.IsNullOrEmpty(sourceData.TimeStart))
            {
                AlertService.ShowAlert(this, "Source PC is no longer active or occupied.", "warning");
                return;
            }

            // Calculate remaining duration/session info
            DateTime.TryParse(sourceData.TimeStart, out DateTime start);
            DateTime.TryParse(sourceData.TimeEnd, out DateTime end);

            TimeSpan duration = end - start;
            int hours = (int)duration.TotalHours;
            int minutes = duration.Minutes;
            bool isOpenTime = (hours > 100000);

            if (isOpenTime)
            {
                duration = TimeService.Get() - start;
                hours = (int)duration.TotalHours;
                minutes = duration.Minutes;
            }
            int totalTime = (int)duration.TotalMinutes;
            decimal billing = (decimal)CalculateRentalPrice.CalculatePrice(totalTime);


            Repositories.Rentals rentals = new Repositories.Rentals();
            var rental = rentals.GetByComputerId(id);
            for (int i = 0; i < rental.Rows.Count; i++)
            {
                string rentalId = rental.Rows[i]["DBId"].ToString();
                string customerId = rental.Rows[i]["DBCustomerId"].ToString();
                bool isSuccess = rentals.Update(rentalId, targetPcId, customerId, totalTime.ToString().Replace(",", ""), billing.ToString().Replace(",", ""));
                if (isSuccess)
                {
                    RemoteMessagingService.LogoutPC(this, sourcePcName);
                    RemoteMessagingService.TransferSession(this, targetPcName, customerId, totalTime, billing, start);
                    AlertService.ShowAlert(this, $"Successfully transferred session from {sourcePcName} to {targetPcName}.", "success");
                }
                else
                {
                    AlertService.ShowAlert(this, "Failed to transfer.", "warning");
                }
            }
        }
    }
}