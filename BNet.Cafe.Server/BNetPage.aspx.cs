using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server
{
    public partial class BNetPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Get the "Form" query string parameter value, default to "Dashboard" if missing
            string formName = Request.QueryString["Form"] ?? "Dashboard";
            LoadMyUserControl(formName);
            LoadMyHyperLink(formName);
        }

        private void LoadMyUserControl(string formName)
        {
            // Relative virtual path for Page.LoadControl
            string virtualPath = $"~/Forms/{formName}.ascx";

            // Physical path only used to check if the file exists on disk
            string physicalPath = Server.MapPath(virtualPath);

            // 1. Pass physicalPath into File.Exists()
            if (File.Exists(physicalPath))
            {
                // 2. Pass virtualPath to Page.LoadControl()
                Control myControl = Page.LoadControl(virtualPath);

                // Assign an ID so ViewState functions properly
                myControl.ID = "myDynamicControl";

                PlaceHolder_Container.Controls.Clear();
                PlaceHolder_Container.Controls.Add(myControl);
            }
        }


        private void LoadMyHyperLink(string formName)
        {
            // Reset classes
            HyperLink_Dashboard.CssClass = "";
            HyperLink_Computers.CssClass = "";
            HyperLink_Users.CssClass = "";


            if (HyperLink_Dashboard.ID.Contains(formName))
            {
                HyperLink_Dashboard.CssClass = "active";
            }
            else if (HyperLink_Computers.ID.Contains(formName))
            {
                HyperLink_Computers.CssClass = "active";
            }
            else if (HyperLink_Users.ID.Contains(formName))
            {
                HyperLink_Users.CssClass = "active";
            }
            else if (HyperLink_Remote.ID.Contains(formName))
            {
                HyperLink_Remote.CssClass = "active";
            }
        }
    }
}