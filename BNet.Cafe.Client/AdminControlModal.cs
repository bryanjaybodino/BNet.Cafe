using System;
using System.Drawing;
using System.Runtime.InteropServices;
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

                // Adjust layout if elevation button is hidden
                grpSecurity.Location = new Point(20, 95);
                grpSecurity.Size = new Size(340, 160);
                btnApplyChanges.Location = new Point(20, 270);

                // Hide or remove the bottom status label since we use pop-ups now
                if (lblStatusMessage != null) lblStatusMessage.Visible = false;

                this.ClientSize = new Size(380, 335);
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

                if (usbSuccess && msiSuccess)
                {
                    MessageBox.Show(
                        "Security settings have been updated and applied successfully!",
                        "Security Updated",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                else
                {
                    MessageBox.Show(
                        "Security settings were applied with some warnings. Check if rules failed to update.",
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
    }
}