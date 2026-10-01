using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using System;

namespace BNet.Cafe.Server.Forms.Modals
{
    public partial class InventoryDelete : System.Web.UI.UserControl
    {
        protected void LinkButton_ConfirmDelete_Click(object sender, EventArgs e)
        {
            string eventTarget = Request.Form["__EVENTTARGET"] ?? string.Empty;
            if (!eventTarget.Contains(LinkButton_ConfirmDelete.ID))
            {
                return;
            }

            InventoryItems inventoryItems = new InventoryItems();
            bool isSuccess = inventoryItems.Delete(HiddenField_DeleteId.Value);

            if (isSuccess)
            {
                AlertService.ShowAlert(this, "Inventory item deleted successfully.", "success");
            }
            else
            {
                AlertService.ShowAlert(this, "Failed to delete inventory item.", "error");
            }
        }
    }
}