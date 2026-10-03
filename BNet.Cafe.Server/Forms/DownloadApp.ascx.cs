using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server.Forms
{
    public partial class DownloadApp : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Services.CodeGenerator codeGenerator = new Services.CodeGenerator();

            string relativeUrl = HyperLink_Url.NavigateUrl;

            // Detect scheme: X-Forwarded-Proto if behind ngrok
            string scheme = "http";
            if (Request.Headers["X-Forwarded-Proto"] != null)
            {
                scheme = Request.Headers["X-Forwarded-Proto"];
            }
            else
            {
                scheme = Request.Url.Scheme;
            }

            // Use Host header instead of Request.Url.Authority
            string host = Request.Headers["Host"] ?? Request.Url.Authority;

            // Resolve the relative path
            string path = ResolveUrl(relativeUrl);

            // Combine
            string fullUrl = $"{scheme}://{host}{path}";

            // Generate QR code
            codeGenerator.QRCode(fullUrl, imgQrCode, Server.MapPath("~/App/icon.png"));


        }
    }
}