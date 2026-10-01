using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server.Forms.Modals
{
    public partial class POSReceipt : System.Web.UI.UserControl
    {
        /// <summary>
        /// Call this method from POS host controls/pages to present the success modal and perform receipt calculation.
        /// </summary>


        public void Show(double totalAmount)
        {
            // Format using InvariantCulture to avoid comma/decimal symbol issues in JS parameter
            string formattedTotal = totalAmount.ToString("F2", CultureInfo.InvariantCulture);
            string script = $"setTimeout(function() {{ openPOSReceiptModal('{formattedTotal}'); }}, 100);";

            // Register on Page so ASP.NET AJAX UpdatePanel includes it in the partial postback response
            ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "ShowPOSReceiptModalScript_" + Guid.NewGuid().ToString("N"), script, true);
        }
    }
}