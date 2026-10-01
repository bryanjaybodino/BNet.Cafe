using BNet.Cafe.Server.Services;
using System;
using System.Web.UI;

namespace BNet.Cafe.Server.Forms
{
    public partial class SupplierCreate : System.Web.UI.UserControl
    {
        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            Repositories.Suppliers suppliers = new Repositories.Suppliers();
            bool isSuccess = suppliers.Create(
                TextBox_SupplierName.Text.Trim(),
                TextBox_ContactPerson.Text.Trim(),
                TextBox_Phone.Text.Trim(),
                TextBox_Email.Text.Trim(),
                TextBox_Address.Text.Trim()
            );

            if (isSuccess)
            {
                AlertService.ShowAlert(UpdatePanel1, "Supplier successfully added.", "success", "BNetPage.aspx?Form=Suppliers");
            }
            else
            {
                AlertService.ShowAlert(UpdatePanel1, "Failed to add supplier. The supplier name might already exist.", "error");
            }
        }
    }
}