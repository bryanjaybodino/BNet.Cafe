namespace BNet.Cafe.Client
{
    partial class MainForm
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
            this.lblBigPcName = new System.Windows.Forms.Label();
            this.badgeCountdown = new BNet.Cafe.Client.Design.ModernBadge();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.Panel_LoginCard = new System.Windows.Forms.Panel();
            this.lblErrorMessage = new System.Windows.Forms.Label();
            this.Button_Login = new System.Windows.Forms.Button();
            this.TextBox_Password = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.TextBox_Username = new System.Windows.Forms.TextBox();
            this.lblUsername = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblDismissHint = new System.Windows.Forms.Label();
            this.Panel_LoginCard.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblBigPcName
            // 
            this.lblBigPcName.AutoSize = true;
            this.lblBigPcName.BackColor = System.Drawing.Color.Transparent;
            this.lblBigPcName.Font = new System.Drawing.Font("Segoe UI Black", 48F, System.Drawing.FontStyle.Bold);
            this.lblBigPcName.ForeColor = System.Drawing.Color.White;
            this.lblBigPcName.Location = new System.Drawing.Point(30, 20);
            this.lblBigPcName.Name = "lblBigPcName";
            this.lblBigPcName.Size = new System.Drawing.Size(276, 106);
            this.lblBigPcName.TabIndex = 0;
            this.lblBigPcName.Text = "PC-00";
            // 
            // badgeCountdown
            // 
            this.badgeCountdown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.badgeCountdown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.badgeCountdown.BorderRadius = 14;
            this.badgeCountdown.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.badgeCountdown.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(185)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.badgeCountdown.Location = new System.Drawing.Point(776, 47);
            this.badgeCountdown.Name = "badgeCountdown";
            this.badgeCountdown.Size = new System.Drawing.Size(380, 42);
            this.badgeCountdown.TabIndex = 1;
            this.badgeCountdown.Text = "⚠️ Auto-Shutdown in: 05:00";
            this.badgeCountdown.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.BackColor = System.Drawing.Color.Transparent;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(210)))), ((int)(((byte)(254)))));
            this.lblSubtitle.Location = new System.Drawing.Point(780, 20);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(158, 20);
            this.lblSubtitle.TabIndex = 2;
            this.lblSubtitle.Text = "IDLE SYSTEM NOTICE";
            // 
            // Panel_LoginCard
            // 
            this.Panel_LoginCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.Panel_LoginCard.Controls.Add(this.lblErrorMessage);
            this.Panel_LoginCard.Controls.Add(this.Button_Login);
            this.Panel_LoginCard.Controls.Add(this.TextBox_Password);
            this.Panel_LoginCard.Controls.Add(this.lblPassword);
            this.Panel_LoginCard.Controls.Add(this.TextBox_Username);
            this.Panel_LoginCard.Controls.Add(this.lblUsername);
            this.Panel_LoginCard.Controls.Add(this.lblTitle);
            this.Panel_LoginCard.Location = new System.Drawing.Point(395, 172);
            this.Panel_LoginCard.Name = "Panel_LoginCard";
            this.Panel_LoginCard.Size = new System.Drawing.Size(400, 350);
            this.Panel_LoginCard.TabIndex = 3;
            // 
            // lblErrorMessage
            // 
            this.lblErrorMessage.AutoSize = true;
            this.lblErrorMessage.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblErrorMessage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(113)))), ((int)(((byte)(113)))));
            this.lblErrorMessage.Location = new System.Drawing.Point(40, 218);
            this.lblErrorMessage.Name = "lblErrorMessage";
            this.lblErrorMessage.Size = new System.Drawing.Size(103, 20);
            this.lblErrorMessage.TabIndex = 6;
            this.lblErrorMessage.Text = "Error message";
            this.lblErrorMessage.Visible = false;
            // 
            // Button_Login
            // 
            this.Button_Login.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.Button_Login.FlatAppearance.BorderSize = 0;
            this.Button_Login.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Button_Login.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.Button_Login.ForeColor = System.Drawing.Color.White;
            this.Button_Login.Location = new System.Drawing.Point(40, 250);
            this.Button_Login.Name = "Button_Login";
            this.Button_Login.Size = new System.Drawing.Size(320, 42);
            this.Button_Login.TabIndex = 5;
            this.Button_Login.Text = "Log In";
            this.Button_Login.UseVisualStyleBackColor = false;
            // 
            // TextBox_Password
            // 
            this.TextBox_Password.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.TextBox_Password.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextBox_Password.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.TextBox_Password.ForeColor = System.Drawing.Color.White;
            this.TextBox_Password.Location = new System.Drawing.Point(40, 176);
            this.TextBox_Password.Name = "TextBox_Password";
            this.TextBox_Password.PasswordChar = '●';
            this.TextBox_Password.Size = new System.Drawing.Size(320, 32);
            this.TextBox_Password.TabIndex = 4;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(231)))), ((int)(((byte)(255)))));
            this.lblPassword.Location = new System.Drawing.Point(40, 150);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(80, 23);
            this.lblPassword.TabIndex = 3;
            this.lblPassword.Text = "Password";
            // 
            // TextBox_Username
            // 
            this.TextBox_Username.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.TextBox_Username.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextBox_Username.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.TextBox_Username.ForeColor = System.Drawing.Color.White;
            this.TextBox_Username.Location = new System.Drawing.Point(40, 106);
            this.TextBox_Username.Name = "TextBox_Username";
            this.TextBox_Username.Size = new System.Drawing.Size(320, 32);
            this.TextBox_Username.TabIndex = 2;
            // 
            // lblUsername
            // 
            this.lblUsername.AutoSize = true;
            this.lblUsername.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblUsername.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(231)))), ((int)(((byte)(255)))));
            this.lblUsername.Location = new System.Drawing.Point(40, 80);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(87, 23);
            this.lblUsername.TabIndex = 1;
            this.lblUsername.Text = "Username";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(99, 25);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(205, 37);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Member Login";
            // 
            // lblDismissHint
            // 
            this.lblDismissHint.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.lblDismissHint.AutoSize = true;
            this.lblDismissHint.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.lblDismissHint.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblDismissHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(231)))), ((int)(((byte)(255)))));
            this.lblDismissHint.Location = new System.Drawing.Point(435, 619);
            this.lblDismissHint.Name = "lblDismissHint";
            this.lblDismissHint.Padding = new System.Windows.Forms.Padding(16, 8, 16, 8);
            this.lblDismissHint.Size = new System.Drawing.Size(312, 39);
            this.lblDismissHint.TabIndex = 4;
            this.lblDismissHint.Text = "Move mouse or press key to return";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.ClientSize = new System.Drawing.Size(1190, 694);
            this.Controls.Add(this.lblDismissHint);
            this.Controls.Add(this.Panel_LoginCard);
            this.Controls.Add(this.badgeCountdown);
            this.Controls.Add(this.lblSubtitle);
            this.Controls.Add(this.lblBigPcName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MainForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Panel_LoginCard.ResumeLayout(false);
            this.Panel_LoginCard.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblBigPcName;
        private Design.ModernBadge badgeCountdown;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Panel Panel_LoginCard;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.TextBox TextBox_Username;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox TextBox_Password;
        private System.Windows.Forms.Button Button_Login;
        private System.Windows.Forms.Label lblErrorMessage;
        private System.Windows.Forms.Label lblDismissHint;
    }
}