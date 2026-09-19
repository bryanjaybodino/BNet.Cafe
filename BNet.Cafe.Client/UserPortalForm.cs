using BNet.Cafe.Client.Services;
using Microsoft.Web.WebView2.Core;
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace BNet.Cafe.Client
{
    public partial class UserPortalForm : Form
    {
        private string userId = string.Empty;
        public UserPortalForm(string userId)
        {
            this.userId = SecuredDataService.Encrypted(userId);
            InitializeComponent();
        }

        private async void UserPortalForm_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.BringToFront();
            this.Activate();

            try
            {
                // Create an Ephemeral (In-Memory) environment so session/auth cookies are NEVER saved to disk
                var environment = await CoreWebView2Environment.CreateAsync(
                    browserExecutableFolder: null,
                    userDataFolder: null,
                    options: new CoreWebView2EnvironmentOptions()
                );

                // Initialize WebView2 with the clean environment
                await webView.EnsureCoreWebView2Async(environment);

                // Disable dev tools & context menus
                webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                webView.CoreWebView2.Settings.AreDevToolsEnabled = false;

                // Bind events
                webView.CoreWebView2.NavigationStarting += WebView_NavigationStarting;
                webView.CoreWebView2.NavigationCompleted += WebView_NavigationCompleted;

                // Clear all active cookies to force clean session
                webView.CoreWebView2.CookieManager.DeleteAllCookies();

                string baseUrl = ConfigHelper.AppUrl?.TrimEnd('/');
                string handlerUrl = $"{baseUrl}/Portal.aspx?UserId={userId}";
                webView.Source = new Uri(handlerUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to initialize browser session: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void WebView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            progressBar.Visible = true;
        }

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            progressBar.Visible = false;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}