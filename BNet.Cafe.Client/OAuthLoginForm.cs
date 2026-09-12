using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace BNet.Cafe.Client
{
    public partial class OAuthLoginForm : Form
    {
        public OAuthLoginForm()
        {
            InitializeComponent();
        }

        private async void OAuthLoginForm_Load(object sender, EventArgs e)
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

                // Clear all active cookies to force re-authentication (password prompt)
                webView.CoreWebView2.CookieManager.DeleteAllCookies();

                string registerUrl = ConfigurationManager.AppSettings["RegisterUrl"];
                webView.Source = new Uri(registerUrl);
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
            CheckAndProcessCallback(new Uri(e.Uri), e);
        }

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            progressBar.Visible = false;
        }

        private void CheckAndProcessCallback(Uri targetUri, CoreWebView2NavigationStartingEventArgs cancelArgs)
        {
            if (targetUri == null) return;

            string fullUrl = targetUri.ToString();

            if (fullUrl.IndexOf("Portal.aspx", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string userId = GetQueryParameter(targetUri, "UserId");
                if (!string.IsNullOrEmpty(userId))
                {
                    if (cancelArgs != null)
                    {
                        cancelArgs.Cancel = true;
                    }
                    this.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show(
                            "Your account has been created. Please top up at the counter to continue.",
                            "Registered Successfully",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                        this.Close();
                    }));
                }
            }
        }

        private string GetQueryParameter(Uri uri, string parameterName)
        {
            if (uri == null || string.IsNullOrEmpty(uri.Query))
                return null;

            var queryParams = HttpUtility.ParseQueryString(uri.Query);
            return queryParams[parameterName];
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}