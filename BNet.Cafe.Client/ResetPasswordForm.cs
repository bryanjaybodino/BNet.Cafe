using BNet.Cafe.Client.Services;
using Microsoft.Web.WebView2.Core;
using System;
using System.Configuration;
using System.Security.Policy;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace BNet.Cafe.Client
{
    public partial class ResetPasswordForm : Form
    {
        public ResetPasswordForm()
        {
            InitializeComponent();
        }
        public async void ResetPassword(string userId)
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

                // Bind loading progress bar events
                webView.CoreWebView2.NavigationStarting += WebView_NavigationStarting;
                webView.CoreWebView2.NavigationCompleted += WebView_NavigationCompleted;

                // Clear active cookies
                webView.CoreWebView2.CookieManager.DeleteAllCookies();

                // Load URL from app configuration
                string baseUrl = ConfigHelper.AppUrl?.TrimEnd('/');
                string handlerUrl = $"{baseUrl}/ResetPassword.aspx?userId={userId}";
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