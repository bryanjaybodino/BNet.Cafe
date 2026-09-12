using BNet.Cafe.Server.Ashx;
using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server.Forms.Modals
{
    public partial class RemoteTimePause : UserControl
    {
        protected void Page_PreRender(object sender, EventArgs e)
        {
            DropDownList_OccupiedPcs.Items.Clear();

            ClientData clientData = new ClientData();
            var liveData = clientData.FetchData();

            // Get all active/occupied computers currently connected
            var activePcs = liveData.Where(x => !string.IsNullOrEmpty(x.TimeStart)).ToList();

            foreach (var pc in activePcs)
            {
                string statusLabel = (pc.IsPaused.ToUpper()=="TRUE") ? " (Paused)" : " (Running)";
                DropDownList_OccupiedPcs.Items.Add(new ListItem(pc.ClientName + statusLabel, pc.ClientName));
            }

            bool hasItems = DropDownList_OccupiedPcs.Items.Count > 0;
            DropDownList_OccupiedPcs.Enabled = hasItems;
            LinkButton_PauseSelected.Enabled = hasItems;
            LinkButton_ResumeSelected.Enabled = hasItems;

            if (!hasItems)
            {
                DropDownList_OccupiedPcs.Items.Add(new ListItem("-- No Occupied PCs Connected --", ""));
            }
        }

        protected void LinkButton_ExecuteAction_Click(object sender, EventArgs e)
        {
            string actionType = HiddenField_ActionType.Value;
            string selectedPc = HiddenField_SelectedComputerName.Value;

            ClientData clientData = new ClientData();
            var liveData = clientData.FetchData();

            switch (actionType)
            {
                case "PAUSE_SELECTED":
                    if (!string.IsNullOrEmpty(selectedPc))
                    {
                        RemoteMessagingService.SendTextMessage(this, selectedPc,ConstantData.RentalCommand.PAUSE);
                        AlertService.ShowAlert(this, $"Sent PAUSE signal to {selectedPc}.", "success");
                    }
                    break;

                case "RESUME_SELECTED":
                    if (!string.IsNullOrEmpty(selectedPc))
                    {
                        RemoteMessagingService.SendTextMessage(this, selectedPc, ConstantData.RentalCommand.RESUME);
                        AlertService.ShowAlert(this, $"Sent RESUME signal to {selectedPc}.", "success");
                    }
                    break;

                case "PAUSE_ALL":
                    var activePcsToPause = liveData.Where(x => !string.IsNullOrEmpty(x.TimeStart)).ToList();
                    foreach (var pc in activePcsToPause)
                    {
                        RemoteMessagingService.SendTextMessage(this, pc.ClientName, ConstantData.RentalCommand.PAUSE);
                    }
                    AlertService.ShowAlert(this, "Sent PAUSE signal to all active PCs.", "success");
                    break;

                case "RESUME_ALL":
                    var activePcsToResume = liveData.Where(x => !string.IsNullOrEmpty(x.TimeStart)).ToList();
                    foreach (var pc in activePcsToResume)
                    {
                        RemoteMessagingService.SendTextMessage(this, pc.ClientName, ConstantData.RentalCommand.RESUME);
                    }
                    AlertService.ShowAlert(this, "Sent RESUME signal to all active PCs.", "success");
                    break;
            }
        }
    }
}