using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using BNet.Cafe.Client.Services;

namespace BNet.Cafe.Client
{
    public partial class AdminControlModal : Form
    {
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public AdminControlModal()
        {
            InitializeComponent();
            ConfigureElevationState();
            LoadCurrentSecurityStatus();
        }

        private void ConfigureElevationState()
        {
            bool isElevated = SecurityInstallerBlockerManager.IsElevated();

            if (isElevated)
            {
                lblElevationStatus.Text = "✓ Running with Administrator Privileges";
                lblElevationStatus.ForeColor = Color.FromArgb(16, 185, 129);
                btnElevate.Visible = false;
                if (lblStatusMessage != null) lblStatusMessage.Visible = false;
            }
            else
            {
                lblElevationStatus.Text = "⚠️ Running as Standard User (Limited Access)";
                lblElevationStatus.ForeColor = Color.FromArgb(217, 119, 6);
                btnElevate.Visible = true;
                if (lblStatusMessage != null) lblStatusMessage.Visible = false;
            }
        }

        private void LoadCurrentSecurityStatus()
        {
            try
            {
                chkBlockUsb.Checked = SecurityUsbManager.IsUsbDisabled();
                chkBlockInstallers.Checked = SecurityInstallerBlockerManager.AreInstallersDisabled();
                chkRunOnStartup.Checked = SecurityStartupManager.IsStartupEnabled();
                chkBlockPorn.Checked = SecurityDnsManager.IsAntiPornEnabled();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading security status: " + ex.Message, "Status Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnApplyChanges_Click(object sender, EventArgs e)
        {
            if (!SecurityInstallerBlockerManager.IsElevated())
            {
                MessageBox.Show("Administrator privileges are required to modify system security settings. Please restart as Administrator.",
                    "Elevation Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                bool usbSuccess = chkBlockUsb.Checked ? SecurityUsbManager.DisableUsb() : SecurityUsbManager.EnableUsb();
                bool msiSuccess = chkBlockInstallers.Checked ? SecurityInstallerBlockerManager.DisableInstallers() : SecurityInstallerBlockerManager.EnableInstallers();
                bool startupSuccess = chkRunOnStartup.Checked ? SecurityStartupManager.EnableStartup() : SecurityStartupManager.DisableStartup();
                bool dnsSuccess = chkBlockPorn.Checked ? SecurityDnsManager.EnableAntiPorn() : SecurityDnsManager.DisableAntiPorn();

                if (usbSuccess && msiSuccess && startupSuccess && dnsSuccess)
                {
                    MessageBox.Show(
                        "Security, startup, and DNS network configurations have been updated and applied successfully!",
                        "Settings Updated",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                else
                {
                    MessageBox.Show(
                        "Security settings were applied with some warnings. Check if rules or network adapter changes failed to update.",
                        "Warning",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to apply security settings: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void BtnExitApp_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to exit the application?",
                "Confirm Exit",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void BtnElevate_Click(object sender, EventArgs e)
        {
            try
            {
                string exePath = Application.ExecutablePath;
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c start \"\" \"{exePath}\"",
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    UseShellExecute = true,
                    Verb = "runas",
                    CreateNoWindow = false
                };

                System.Diagnostics.Process.Start(psi);
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Elevation failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void PanelHeader_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        }

        private async void BtnCheckServer_Click(object sender, EventArgs e)
        {
            // Disable buttons and show progress bar
            btnCheckServer.Enabled = false;
            btnApplyChanges.Enabled = false;
            btnExitApp.Enabled = false;

            progressBarSync.Style = ProgressBarStyle.Marquee;

            if (lblStatusMessage != null)
            {
                lblStatusMessage.Visible = true;
                lblStatusMessage.Text = "Scanning network for server IP and syncing config...";
            }

            try
            {
                // Run config re-check asynchronously to avoid freezing the UI thread
                bool success = await Task.Run(() => ConfigHelper.RecheckAndUpdateServerAsync());

                if (success)
                {
                    MessageBox.Show(
                        "Server IP re-discovered and configuration updated successfully!",
                        "Server Updated",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                else
                {
                    MessageBox.Show(
                        "Could not locate an active server on the local network. Please verify your server connection.",
                        "Server Not Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An error occurred while rechecking the server: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                // Re-enable controls and hide progress bar
                progressBarSync.Style = ProgressBarStyle.Blocks;
                btnCheckServer.Enabled = true;
                btnApplyChanges.Enabled = true;
                btnExitApp.Enabled = true;

                if (lblStatusMessage != null)
                {
                    lblStatusMessage.Text = string.Empty;
                }
            }
        }
    }
}