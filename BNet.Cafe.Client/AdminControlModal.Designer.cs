namespace BNet.Cafe.Client
{
    partial class AdminControlModal
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblElevationStatus = new System.Windows.Forms.Label();
            this.btnElevate = new System.Windows.Forms.Button();
            this.grpSecurity = new System.Windows.Forms.GroupBox();
            this.chkBlockPorn = new System.Windows.Forms.CheckBox();
            this.chkRunOnStartup = new System.Windows.Forms.CheckBox();
            this.chkBlockInstallers = new System.Windows.Forms.CheckBox();
            this.chkBlockUsb = new System.Windows.Forms.CheckBox();
            this.btnApplyChanges = new System.Windows.Forms.Button();
            this.progressBarSync = new System.Windows.Forms.ProgressBar();
            this.btnCheckServer = new System.Windows.Forms.Button();
            this.btnExitApp = new System.Windows.Forms.Button();
            this.lblStatusMessage = new System.Windows.Forms.Label();
            this.panelHeader.SuspendLayout();
            this.grpSecurity.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(80)))), ((int)(((byte)(236)))));
            this.panelHeader.Controls.Add(this.lblTitle);
            this.panelHeader.Controls.Add(this.btnClose);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(380, 45);
            this.panelHeader.TabIndex = 0;
            this.panelHeader.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PanelHeader_MouseDown);
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(12, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(293, 25);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "🛡️ Admin Security Control Panel";
            this.lblTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PanelHeader_MouseDown);
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.Transparent;
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(335, 5);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(35, 35);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "✕";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // lblElevationStatus
            // 
            this.lblElevationStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblElevationStatus.Location = new System.Drawing.Point(20, 60);
            this.lblElevationStatus.Name = "lblElevationStatus";
            this.lblElevationStatus.Size = new System.Drawing.Size(340, 25);
            this.lblElevationStatus.TabIndex = 1;
            this.lblElevationStatus.Text = "Checking elevation status...";
            // 
            // btnElevate
            // 
            this.btnElevate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(119)))), ((int)(((byte)(6)))));
            this.btnElevate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnElevate.FlatAppearance.BorderSize = 0;
            this.btnElevate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnElevate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnElevate.ForeColor = System.Drawing.Color.White;
            this.btnElevate.Location = new System.Drawing.Point(20, 90);
            this.btnElevate.Name = "btnElevate";
            this.btnElevate.Size = new System.Drawing.Size(340, 35);
            this.btnElevate.TabIndex = 2;
            this.btnElevate.Text = "Restart as Administrator";
            this.btnElevate.UseVisualStyleBackColor = false;
            this.btnElevate.Click += new System.EventHandler(this.BtnElevate_Click);
            // 
            // grpSecurity
            // 
            this.grpSecurity.Controls.Add(this.chkBlockPorn);
            this.grpSecurity.Controls.Add(this.chkRunOnStartup);
            this.grpSecurity.Controls.Add(this.chkBlockInstallers);
            this.grpSecurity.Controls.Add(this.chkBlockUsb);
            this.grpSecurity.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpSecurity.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.grpSecurity.Location = new System.Drawing.Point(20, 135);
            this.grpSecurity.Name = "grpSecurity";
            this.grpSecurity.Size = new System.Drawing.Size(340, 175);
            this.grpSecurity.TabIndex = 3;
            this.grpSecurity.TabStop = false;
            this.grpSecurity.Text = " System Security Restrictions ";
            // 
            // chkBlockPorn
            // 
            this.chkBlockPorn.AutoSize = true;
            this.chkBlockPorn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkBlockPorn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkBlockPorn.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkBlockPorn.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.chkBlockPorn.Location = new System.Drawing.Point(8, 140);
            this.chkBlockPorn.Name = "chkBlockPorn";
            this.chkBlockPorn.Size = new System.Drawing.Size(248, 24);
            this.chkBlockPorn.TabIndex = 3;
            this.chkBlockPorn.Text = "Block Adult / Pornographic Sites";
            this.chkBlockPorn.UseVisualStyleBackColor = true;
            // 
            // chkRunOnStartup
            // 
            this.chkRunOnStartup.AutoSize = true;
            this.chkRunOnStartup.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkRunOnStartup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkRunOnStartup.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkRunOnStartup.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.chkRunOnStartup.Location = new System.Drawing.Point(8, 105);
            this.chkRunOnStartup.Name = "chkRunOnStartup";
            this.chkRunOnStartup.Size = new System.Drawing.Size(271, 24);
            this.chkRunOnStartup.TabIndex = 2;
            this.chkRunOnStartup.Text = "Run Application on Windows Startup";
            this.chkRunOnStartup.UseVisualStyleBackColor = true;
            // 
            // chkBlockInstallers
            // 
            this.chkBlockInstallers.AutoSize = true;
            this.chkBlockInstallers.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkBlockInstallers.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkBlockInstallers.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkBlockInstallers.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.chkBlockInstallers.Location = new System.Drawing.Point(8, 70);
            this.chkBlockInstallers.Name = "chkBlockInstallers";
            this.chkBlockInstallers.Size = new System.Drawing.Size(294, 24);
            this.chkBlockInstallers.TabIndex = 1;
            this.chkBlockInstallers.Text = "Block Software Installers (.msi packages)";
            this.chkBlockInstallers.UseVisualStyleBackColor = true;
            // 
            // chkBlockUsb
            // 
            this.chkBlockUsb.AutoSize = true;
            this.chkBlockUsb.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkBlockUsb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkBlockUsb.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkBlockUsb.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.chkBlockUsb.Location = new System.Drawing.Point(8, 35);
            this.chkBlockUsb.Name = "chkBlockUsb";
            this.chkBlockUsb.Size = new System.Drawing.Size(330, 24);
            this.chkBlockUsb.TabIndex = 0;
            this.chkBlockUsb.Text = "Block USB Storage && Smartphones (MTP/PTP)";
            this.chkBlockUsb.UseVisualStyleBackColor = true;
            // 
            // btnApplyChanges
            // 
            this.btnApplyChanges.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(102)))), ((int)(((byte)(241)))));
            this.btnApplyChanges.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnApplyChanges.FlatAppearance.BorderSize = 0;
            this.btnApplyChanges.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApplyChanges.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnApplyChanges.ForeColor = System.Drawing.Color.White;
            this.btnApplyChanges.Location = new System.Drawing.Point(20, 323);
            this.btnApplyChanges.Name = "btnApplyChanges";
            this.btnApplyChanges.Size = new System.Drawing.Size(340, 38);
            this.btnApplyChanges.TabIndex = 4;
            this.btnApplyChanges.Text = "Save && Apply Security Settings";
            this.btnApplyChanges.UseVisualStyleBackColor = false;
            this.btnApplyChanges.Click += new System.EventHandler(this.BtnApplyChanges_Click);
            // 
            // progressBarSync
            // 
            this.progressBarSync.Location = new System.Drawing.Point(20, 370);
            this.progressBarSync.MarqueeAnimationSpeed = 30;
            this.progressBarSync.Name = "progressBarSync";
            this.progressBarSync.Size = new System.Drawing.Size(340, 18);
            this.progressBarSync.TabIndex = 7;
            // 
            // btnCheckServer
            // 
            this.btnCheckServer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(165)))), ((int)(((byte)(233)))));
            this.btnCheckServer.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCheckServer.FlatAppearance.BorderSize = 0;
            this.btnCheckServer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCheckServer.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnCheckServer.ForeColor = System.Drawing.Color.White;
            this.btnCheckServer.Location = new System.Drawing.Point(20, 396);
            this.btnCheckServer.Name = "btnCheckServer";
            this.btnCheckServer.Size = new System.Drawing.Size(340, 38);
            this.btnCheckServer.TabIndex = 5;
            this.btnCheckServer.Text = "🔍 Recheck Server && Update Config";
            this.btnCheckServer.UseVisualStyleBackColor = false;
            this.btnCheckServer.Click += new System.EventHandler(this.BtnCheckServer_Click);
            // 
            // btnExitApp
            // 
            this.btnExitApp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.btnExitApp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExitApp.FlatAppearance.BorderSize = 0;
            this.btnExitApp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExitApp.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnExitApp.ForeColor = System.Drawing.Color.White;
            this.btnExitApp.Location = new System.Drawing.Point(20, 443);
            this.btnExitApp.Name = "btnExitApp";
            this.btnExitApp.Size = new System.Drawing.Size(340, 38);
            this.btnExitApp.TabIndex = 6;
            this.btnExitApp.Text = "Exit Application";
            this.btnExitApp.UseVisualStyleBackColor = false;
            this.btnExitApp.Click += new System.EventHandler(this.BtnExitApp_Click);
            // 
            // lblStatusMessage
            // 
            this.lblStatusMessage.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Italic);
            this.lblStatusMessage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblStatusMessage.Location = new System.Drawing.Point(20, 486);
            this.lblStatusMessage.Name = "lblStatusMessage";
            this.lblStatusMessage.Size = new System.Drawing.Size(340, 25);
            this.lblStatusMessage.TabIndex = 8;
            this.lblStatusMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // AdminControlModal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.ClientSize = new System.Drawing.Size(380, 525);
            this.Controls.Add(this.lblStatusMessage);
            this.Controls.Add(this.btnExitApp);
            this.Controls.Add(this.btnCheckServer);
            this.Controls.Add(this.progressBarSync);
            this.Controls.Add(this.btnApplyChanges);
            this.Controls.Add(this.grpSecurity);
            this.Controls.Add(this.btnElevate);
            this.Controls.Add(this.lblElevationStatus);
            this.Controls.Add(this.panelHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AdminControlModal";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.grpSecurity.ResumeLayout(false);
            this.grpSecurity.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblElevationStatus;
        private System.Windows.Forms.Button btnElevate;
        private System.Windows.Forms.GroupBox grpSecurity;
        private System.Windows.Forms.CheckBox chkBlockUsb;
        private System.Windows.Forms.CheckBox chkBlockInstallers;
        private System.Windows.Forms.CheckBox chkRunOnStartup;
        private System.Windows.Forms.CheckBox chkBlockPorn;
        private System.Windows.Forms.Button btnApplyChanges;
        private System.Windows.Forms.ProgressBar progressBarSync;
        private System.Windows.Forms.Button btnCheckServer;
        private System.Windows.Forms.Button btnExitApp;
        private System.Windows.Forms.Label lblStatusMessage;
    }
}