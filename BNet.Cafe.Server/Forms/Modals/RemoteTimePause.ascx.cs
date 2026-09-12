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

            var activePcs = liveData.Where(x => !string.IsNullOrEmpty(x.TimeStart)).ToList();

            if (activePcs.Count > 0)
            {
                // ALL PCs Option: Check if all active PCs are currently paused
                bool allPaused = activePcs.All(p => p.IsPaused.ToUpper() == "TRUE");
                ListItem allItem = new ListItem("-- All Active PCs --", "ALL_PCS");
                allItem.Attributes["data-paused"] = allPaused ? "true" : "false";
                DropDownList_OccupiedPcs.Items.Add(allItem);

                foreach (var pc in activePcs)
                {
                    bool isPaused = pc.IsPaused.ToUpper() == "TRUE";
                    string statusLabel = isPaused ? " (Paused)" : " (Running)";

                    ListItem item = new ListItem(pc.ClientName + statusLabel, pc.ClientName);
                    item.Attributes["data-paused"] = isPaused ? "true" : "false";

                    DropDownList_OccupiedPcs.Items.Add(item);
                }

                DropDownList_OccupiedPcs.Enabled = true;
                LinkButton_ToggleState.Enabled = true;
            }
            else
            {
                DropDownList_OccupiedPcs.Items.Add(new ListItem("-- No Occupied PCs Connected --", ""));
                DropDownList_OccupiedPcs.Enabled = false;
                LinkButton_ToggleState.Enabled = false;
            }
        }

        protected void LinkButton_ExecuteAction_Click(object sender, EventArgs e)
        {
            string actionType = HiddenField_ActionType.Value; // "PAUSE" or "RESUME"
            string targetPc = HiddenField_SelectedComputerName.Value;

            if (string.IsNullOrEmpty(targetPc) || string.IsNullOrEmpty(actionType)) return;

            ClientData clientData = new ClientData();
            var liveData = clientData.FetchData();

            string command = (actionType == "PAUSE") ? ConstantData.RentalCommand.PAUSE : ConstantData.RentalCommand.RESUME;

            if (targetPc == "ALL_PCS")
            {
                var activePcs = liveData.Where(x => !string.IsNullOrEmpty(x.TimeStart)).ToList();
                foreach (var pc in activePcs)
                {
                    RemoteMessagingService.SendTextMessage(this, pc.ClientName, command);
                }
                AlertService.ShowAlert(this, $"Sent {actionType} signal to all active PCs.", "success");
            }
            else
            {
                RemoteMessagingService.SendTextMessage(this, targetPc, command);
                AlertService.ShowAlert(this, $"Sent {actionType} signal to {targetPc}.", "success");
            }
        }
    }
}