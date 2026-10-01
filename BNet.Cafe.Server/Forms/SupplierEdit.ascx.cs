using BNet.Cafe.Server.Services;
using System;
using System.Data;

namespace BNet.Cafe.Server.Forms
{
    public partial class SupplierEdit : System.Web.UI.UserControl
    {
        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Repositories.Suppliers suppliers = new Repositories.Suppliers();
                DataTable supplier = suppliers.GetById(Request.QueryString["id"]);
                if (supplier != null && supplier.Rows.Count > 0)
                {
                    TextBox_SupplierName.Text = supplier.Rows[0]["DBSupplierName"].ToString();
                    TextBox_ContactPerson.Text = supplier.Rows[0]["DBContactPerson"].ToString();
                    TextBox_Phone.Text = supplier.Rows[0]["DBPhone"].ToString();
                    TextBox_Email.Text = supplier.Rows[0]["DBEmail"].ToString();
                    TextBox_Address.Text = supplier.Rows[0]["DBAddress"].ToString();
                }
            }
        }

        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            Repositories.Suppliers suppliers = new Repositories.Suppliers();
            string id = Request.QueryString["id"];
            bool isSuccess = suppliers.Update(
                id,
                TextBox_SupplierName.Text.Trim(),
                TextBox_ContactPerson.Text.Trim(),
                TextBox_Phone.Text.Trim(),
                TextBox_Email.Text.Trim(),
                TextBox_Address.Text.Trim()
            );

            if (isSuccess)
            {
                AlertService.ShowAlert(UpdatePanel1, "Supplier details updated successfully.", "success", "BNetPage.aspx?Form=Suppliers");
            }
            else
            {
                AlertService.ShowAlert(UpdatePanel1, "Failed to update supplier details.", "error");
            }
        }
    }
}