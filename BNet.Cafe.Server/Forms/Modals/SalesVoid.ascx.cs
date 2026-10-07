using System;
using System.Data;
using System.Web.UI;
using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;

namespace BNet.Cafe.Server.Forms.Modals
{
    public partial class SalesVoid : System.Web.UI.UserControl
    {
        private readonly InventoryTransactions transactionsRepo = new InventoryTransactions();
        private readonly InventoryItems itemsRepo = new InventoryItems();

        protected void LinkButton_ConfirmVoid_Click(object sender, EventArgs e)
        {
            string eventTarget = Request.Form["__EVENTTARGET"] ?? string.Empty;
            if (!eventTarget.Contains(LinkButton_ConfirmVoid.ID))
            {
                return;
            }

            string voidId = HiddenField_VoidId.Value;

            if (string.IsNullOrEmpty(voidId))
            {
                AlertService.ShowAlert(this, "Invalid transaction record.", "warning");
                return;
            }

            // 1. Get transaction details
            DataTable dtTrans = transactionsRepo.GetById(voidId);
            if (dtTrans == null || dtTrans.Rows.Count == 0)
            {
                AlertService.ShowAlert(this, "Transaction record not found.", "error");
                return;
            }

            DataRow transRow = dtTrans.Rows[0];
            string itemId = transRow["DBItemId"].ToString();
            int transactionQty = Convert.ToInt32(transRow["DBQuantity"]);

            // 2. Add sold stock back to inventory
            DataTable dtItem = itemsRepo.GetById(itemId);
            if (dtItem != null && dtItem.Rows.Count > 0)
            {
                int currentStock = Convert.ToInt32(dtItem.Rows[0]["DBQuantityInStock"]);
                int newStock = currentStock + transactionQty; // Revert stock back

                itemsRepo.UpdateStock(itemId, newStock.ToString());
            }

            // 3. Delete or mark transaction as void
            bool isSuccess = transactionsRepo.Delete(voidId);

            if (isSuccess)
            {
                AlertService.ShowAlert(this, "Sale voided and inventory stock restored successfully.", "success");
            }
            else
            {
                AlertService.ShowAlert(this, "Failed to void sale transaction.", "error");
            }
        }
    }
}