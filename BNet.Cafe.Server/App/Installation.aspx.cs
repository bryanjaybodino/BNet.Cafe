using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server.App
{
    public partial class Installation : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            Image_Icon.ImageUrl = "~/App/icon.png?v=" + Guid.NewGuid();

            ScriptManager1.CompositeScript.Scripts.Add(new ScriptReference("script.js"));

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



            // Combine
            string fullUrl = $"{scheme}://{host}{ResolveUrl(HyperLink_Url.NavigateUrl)}";


            string imageUrl = $"{scheme}://{host}{ResolveUrl(Image_Icon.ImageUrl)}";
            HyperLink_Url.Text = imageUrl;
            string json = File.ReadAllText(Server.MapPath("~/App/manifest-template.json"));

            json = json.Replace("@name", "BNet Cafe")
                     .Replace("@short_name", "BNet Cafe")
                     .Replace("@start_url", fullUrl)
                     .Replace("@icon", imageUrl);
            File.WriteAllText(Server.MapPath("~/App/manifest.json"), json);
        }
    }
}
