using System;
using System.Data;
using System.Web.UI;
using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;

namespace BNet.Cafe.Server.Forms.Modals
{
    public partial class StockInDelete : System.Web.UI.UserControl
    {
        private readonly InventoryTransactions transactionsRepo = new InventoryTransactions();
        private readonly InventoryItems itemsRepo = new InventoryItems();

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

            // 1. Retrieve transaction details to find Item ID and added Quantity
            DataTable dtTrans = transactionsRepo.GetById(deleteId);
            if (dtTrans == null || dtTrans.Rows.Count == 0)
            {
                AlertService.ShowAlert(this, "Transaction record not found.", "error");
                return;
            }

            DataRow transRow = dtTrans.Rows[0];
            string itemId = transRow["DBItemId"].ToString();
            int transactionQty = Convert.ToInt32(transRow["DBQuantity"]);

            // 2. Retrieve current inventory item stock
            DataTable dtItem = itemsRepo.GetById(itemId);
            if (dtItem != null && dtItem.Rows.Count > 0)
            {
                int currentStock = Convert.ToInt32(dtItem.Rows[0]["DBQuantityInStock"]);
                int newStock = currentStock - transactionQty;

                // Ensure stock doesn't drop below zero
                if (newStock < 0) newStock = 0;

                // Update item stock in inventory
                itemsRepo.UpdateStock(itemId, newStock.ToString());
            }

            // 3. Delete transaction log record
            bool isSuccess = transactionsRepo.Delete(deleteId);

            if (isSuccess)
            {
                AlertService.ShowAlert(this, "Stock record deleted and inventory updated successfully.", "success");
            }
            else
            {
                AlertService.ShowAlert(this, "Failed to delete stock record.", "error");
            }
        }
    }
}