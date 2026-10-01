using BNet.Cafe.Server.Forms;
using BNet.Cafe.Server.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server
{
    public partial class BNetPage : System.Web.UI.Page
    {
        Sessions.User userCookies = new Sessions.User();

        private const string SHA_FILE_NAME = "CurrentCommitSha";
        private string ShaFolderPath => Server.MapPath("~/App_Data/");
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                if (Request.QueryString["Logout"] != null)
                {
                    userCookies.RemoveCookies();
                    Response.Redirect("~/Login.aspx"); // Redirect to your login page
                    return;
                }

                FileCssHelper.BundleCss();
                FileJsHelpler.BundleBNetPageScripts(ScriptManager1);
                if (userCookies.count == 0)
                {
                    Response.Redirect("~/Login.aspx");
                }
                label_FullName.Text = userCookies.user_fullname.ToUpper();
                if (userCookies.isUser)
                {
                    Response.Redirect($"~/Portal.aspx?Email={userCookies.user_email}", false);
                }


                Label_InitialName.Text = string.Join("",
                    userCookies.user_fullname
                        .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .Select(name => name[0])
                ).ToUpper();


                // Run GitHub check asynchronously once per session
                if (Session["GitHubUpdateChecked"] == null)
                {
                    RegisterAsyncTask(new PageAsyncTask(CheckGitHubUpdateAsync));
                }
            }

            // Get the "Form" query string parameter value, default to "Dashboard" if missing
            string formName = Request.QueryString["Form"] ?? "Dashboard";
            LoadMyUserControl(formName);
            LoadMyHyperLink(formName);
            Load_Scripts(formName);
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
                // Reset active state for all links
                HyperLink_Dashboard.CssClass = "";
                HyperLink_Computers.CssClass = "";
                HyperLink_SeatMap.CssClass = "";
                HyperLink_Remote.CssClass = "";
                HyperLink_PricingSettings.CssClass = "";

                HyperLink_POS.CssClass = "";
                HyperLink_Inventory.CssClass = "";
                HyperLink_Suppliers.CssClass = "";

                HyperLink_SportTimer.CssClass = "";

                HyperLink_Billings.CssClass = "";
                HyperLink_Users.CssClass = "";
                HyperLink_TopUp.CssClass = "";
                HyperLink_WallPaper.CssClass = "";
                HyperLink_Commits.CssClass = "";

                // Check active route
                if (formName.Contains(HyperLink_Dashboard.ToolTip)) HyperLink_Dashboard.CssClass = "active";
                else if (formName.Contains(HyperLink_Computers.ToolTip)) HyperLink_Computers.CssClass = "active";
                else if (formName.Contains(HyperLink_SeatMap.ToolTip)) HyperLink_SeatMap.CssClass = "active";
                else if (formName.Contains(HyperLink_Remote.ToolTip)) HyperLink_Remote.CssClass = "active";
                else if (formName.Contains(HyperLink_PricingSettings.ToolTip)) HyperLink_PricingSettings.CssClass = "active";

                else if (formName.Contains(HyperLink_POS.ToolTip)) HyperLink_POS.CssClass = "active";
                else if (formName.Contains(HyperLink_Inventory.ToolTip)) HyperLink_Inventory.CssClass = "active";
                else if (formName.Contains(HyperLink_Suppliers.ToolTip)) HyperLink_Suppliers.CssClass = "active";

                else if (formName.Contains(HyperLink_SportTimer.ToolTip)) HyperLink_SportTimer.CssClass = "active";

                else if (formName.Contains(HyperLink_Billings.ToolTip)) HyperLink_Billings.CssClass = "active";
                else if (formName.Contains(HyperLink_Users.ToolTip)) HyperLink_Users.CssClass = "active";
                else if (formName.Contains(HyperLink_TopUp.ToolTip)) HyperLink_TopUp.CssClass = "active";
                else if (formName.Contains(HyperLink_WallPaper.ToolTip)) HyperLink_WallPaper.CssClass = "active";
                else if (formName.Contains(HyperLink_Commits.ToolTip)) HyperLink_Commits.CssClass = "active";
            }
        }

        private void Load_Scripts(string formName)
        {
            if (IsPostBack) return;

            string[] scripts;

            switch (formName?.ToUpper())
            {
                case "DASHBOARD":
                    scripts = new[] { "BNetChart/BNetBaseChart", "BNetChart/BNetBarChart", "BNetChart/BNetDonutChart", "BNetChart/BNetLineChart" };
                    break;
                case "COMPUTERS":
                    scripts = new[] { "Pages/RemoteMessaging", "Pages/Computers" };
                    break;
                case "COMPUTERCREATE":
                case "COMPUTEREDIT":
                    scripts = new[] { "Pages/ComputerManage" };
                    break;
                case "USERCREATE":
                case "USEREDIT":
                    scripts = new[] { "Pages/UserManage" };
                    break;
                case "USERTOPUP":
                case "USERBONUS":
                    scripts = new[] { "Pages/UserTopUp" };
                    break;
                case "REMOTE":
                    scripts = new[] { "Pages/RemoteMessaging", "Pages/Remote" };
                    break;
                case "RENTALMANAGE":
                    scripts = new[] { "Pages/RentalManage", "Pages/RemoteMessaging" };
                    break;
                case "SEATMAP":
                    scripts = new[] { "Pages/SeatMap", "Pages/Computers" };
                    break;
                case "PRICINGSETTINGS":
                    scripts = new[] { "Pages/PricingSettings" };
                    break;
                case "WALLPAPER":
                    scripts = new[] { "Pages/Wallpaper" };
                    break;
                case "SUPPLIERCREATE":
                case "SUPPLIEREDIT":
                    scripts = new[] { "Pages/SupplierManage" };
                    break;
                case "INVENTORYCREATE":
                case "INVENTORYEDIT":
                    scripts = new[] { "Pages/InventoryManage" };
                    break;
                default:
                    scripts = Array.Empty<string>();
                    break;
            }

            foreach (string script in scripts)
            {
                FileJsHelpler.BundleAddScripts(ScriptManager1, script);
            }
        }
        private async Task CheckGitHubUpdateAsync()
        {
            try
            {
                if (!Directory.Exists(ShaFolderPath))
                {
                    Directory.CreateDirectory(ShaFolderPath);
                }

                string localSavedSha = FileTextHelper.GetText(ShaFolderPath, SHA_FILE_NAME).Trim();

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "BNet-Cafe-Server-App");

                    string apiUrl = "https://api.github.com/repos/bryanjaybodino/BNet.Cafe.Timer/commits/main";
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        JObject root = JObject.Parse(json);

                        string latestSha = root.Value<string>("sha") ?? string.Empty;

                        Session["GitHubUpdateChecked"] = true;

                        // Trigger modal if it's the first load OR if a new commit exists
                        if (string.IsNullOrEmpty(localSavedSha) || !string.Equals(localSavedSha, latestSha, StringComparison.OrdinalIgnoreCase))
                        {
                            // Save latest SHA to text file so it doesn't show again
                            FileTextHelper.UpdateText(latestSha, ShaFolderPath, SHA_FILE_NAME);

                            string commitMsg = root.SelectToken("commit.message")?.ToString() ?? "";
                            string authorName = root.SelectToken("commit.author.name")?.ToString() ?? "Contributor";
                            string commitDateStr = root.SelectToken("commit.author.date")?.ToString() ?? "";
                            string htmlUrl = root.Value<string>("html_url");

                            DateTime.TryParse(commitDateStr, out DateTime commitDate);

                            var updatePayload = new
                            {
                                sha = latestSha.Length >= 7 ? latestSha.Substring(0, 7) : latestSha,
                                message = commitMsg.Split('\n')[0],
                                author = authorName,
                                date = commitDate.ToString("MMM dd, yyyy HH:mm"),
                                url = htmlUrl
                            };

                            string jsonPayload = JsonConvert.SerializeObject(updatePayload);
                            string script = $"setTimeout(function() {{ if (typeof showGitHubUpdateModal === 'function') {{ showGitHubUpdateModal({jsonPayload}); }} }}, 200);";

                            ScriptManager.RegisterStartupScript(this, GetType(), "ShowGitHubUpdate", script, true);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Silently ignore network or file access exceptions
            }
        }
    }
}