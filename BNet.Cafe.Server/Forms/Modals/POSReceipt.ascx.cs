using System;
using System.Globalization;
using System.Web.UI;

namespace BNet.Cafe.Server.Forms.Modals
{
    public partial class POSReceipt : System.Web.UI.UserControl
    {
        /// <summary>
        /// Call this method from POS host controls/pages to present the success modal and perform receipt calculation.
        /// </summary>
        public void Show(double totalAmount)
        {
            string formattedTotal = totalAmount.ToString("F2", CultureInfo.InvariantCulture);
            string script = $"setTimeout(function() {{ openPOSReceiptModal('{formattedTotal}'); }}, 100);";

            ScriptManager.RegisterStartupScript(
                this.Page,
                this.Page.GetType(),
                "ShowPOSReceiptModalScript_" + Guid.NewGuid().ToString("N"),
                script,
                true
            );
        }
    }
}