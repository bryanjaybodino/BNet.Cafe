using BNet.Cafe.Server.Services;
using System;

namespace BNet.Cafe.Server.Forms.Modals
{
    public partial class SupplierDelete : System.Web.UI.UserControl
    {
        protected void LinkButton_ConfirmDelete_Click(object sender, EventArgs e)
        {
            string eventTarget = Request.Form["__EVENTTARGET"] ?? string.Empty;
            if (!eventTarget.Contains(LinkButton_ConfirmDelete.ID))
            {
                return;
            }

            Repositories.Suppliers suppliers = new Repositories.Suppliers();
            bool isSuccess = suppliers.Delete(HiddenField_DeleteId.Value);

            if (isSuccess)
            {
                AlertService.ShowAlert(this, "Supplier successfully deleted.", "success");
            }
            else
            {
                AlertService.ShowAlert(this, "Failed to delete supplier.", "error");
            }
        }
    }
}