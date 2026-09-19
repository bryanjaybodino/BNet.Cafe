using BNet.Cafe.Server.Services;
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
        Sessions.User userCookies = new Sessions.User();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                FileCssHelper.BundleCss();
                FileJsHelpler.BundleBNetPageScripts(ScriptManager1);
                if (userCookies.count == 0)
                {
                    Response.Redirect("Login.aspx");
                }
                label_FullName.Text = userCookies.user_fullname.ToUpper();
                if(userCookies.isUser)
                {
                    Response.Redirect($"Portal.aspx?Email={userCookies.user_email}", false);
                }


                Label_InitialName.Text = string.Join("",
                    userCookies.user_fullname
                        .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .Select(name => name[0])
                ).ToUpper();

            }

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
                myControl.ID = "BNet";

                PlaceHolder_Container.Controls.Clear();
                PlaceHolder_Container.Controls.Add(myControl);
            }
        }


        private void LoadMyHyperLink(string formName)
        {
            if (!IsPostBack)
            {
                // Reset classes
                HyperLink_Dashboard.CssClass = "";
                HyperLink_Computers.CssClass = "";
                HyperLink_Users.CssClass = "";
                HyperLink_Remote.CssClass = "";
                HyperLink_Billings.CssClass = "";


                if (formName.Contains(HyperLink_Dashboard.ToolTip))
                {
                    HyperLink_Dashboard.CssClass = "active";
                }
                else if (formName.Contains(HyperLink_Computers.ToolTip))
                {
                    HyperLink_Computers.CssClass = "active";
                }
                else if (formName.Contains(HyperLink_Users.ToolTip))
                {
                    HyperLink_Users.CssClass = "active";
                }
                else if (formName.Contains(HyperLink_Remote.ToolTip))
                {
                    HyperLink_Remote.CssClass = "active";
                }
                else if (formName.Contains(HyperLink_Billings.ToolTip))
                {
                    HyperLink_Billings.CssClass = "active";
                }
            }
        }
    }
}