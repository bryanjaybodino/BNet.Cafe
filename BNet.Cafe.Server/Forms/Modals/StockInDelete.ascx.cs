using System;
using System.Web.UI;
using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;

namespace BNet.Cafe.Server.Forms.Modals
{
    public partial class StockInDelete : System.Web.UI.UserControl
    {
        private readonly InventoryTransactions transactionsRepo = new InventoryTransactions();

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void LinkButton_ConfirmDelete_Click(object sender, EventArgs e)
        {
            // Verify that the postback target is actually this button
            string eventTarget = Request.Form["__EVENTTARGET"] ?? string.Empty;
            if (!eventTarget.Contains(LinkButton_ConfirmDelete.ID))
            {
                return;
            }

            string deleteId = HiddenField_DeleteId.Value;

            if (string.IsNullOrEmpty(deleteId))
            {
                AlertService.ShowAlert(this, "Invalid transaction record.", "warning");
                return;
            }

            bool isSuccess = transactionsRepo.Delete(deleteId);

            if (isSuccess)
            {
                AlertService.ShowAlert(this, "Stock record successfully deleted.", "success");
            }
            else
            {
                AlertService.ShowAlert(this, "Failed to delete stock record.", "error");
            }
        }
    }
}