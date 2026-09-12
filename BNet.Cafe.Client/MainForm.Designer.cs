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
            this.panelLeftHero = new System.Windows.Forms.Panel();
            this.lblBigPcName = new System.Windows.Forms.Label();
            this.labelStat3Val = new System.Windows.Forms.Label();
            this.labelStat3Title = new System.Windows.Forms.Label();
            this.labelStat2Val = new System.Windows.Forms.Label();
            this.labelStat2Title = new System.Windows.Forms.Label();
            this.labelStat1Val = new System.Windows.Forms.Label();
            this.labelStat1Title = new System.Windows.Forms.Label();
            this.labelHeroSubtitle = new System.Windows.Forms.Label();
            this.labelHeroTitle = new System.Windows.Forms.Label();
            this.panelRightContent = new System.Windows.Forms.Panel();
            this.panelLoginContainer = new System.Windows.Forms.Panel();
            this.lblCopyright = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.lblUsername = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblWelcome = new System.Windows.Forms.Label();
            this.lblCategory = new System.Windows.Forms.Label();
            this.Button_Login = new BNet.Cafe.Client.Design.ModernButton();
            this.lblStatusBadge = new BNet.Cafe.Client.Design.ModernBadge();
            this.Button_Register = new BNet.Cafe.Client.Design.ModernButton();
            this.TextBox_Password = new BNet.Cafe.Client.Design.ModernTextBox();
            this.TextBox_Username = new BNet.Cafe.Client.Design.ModernTextBox();
            this.panelLeftHero.SuspendLayout();
            this.panelRightContent.SuspendLayout();
            this.panelLoginContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelLeftHero
            // 
            this.panelLeftHero.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(80)))), ((int)(((byte)(236)))));
            this.panelLeftHero.Controls.Add(this.lblBigPcName);
            this.panelLeftHero.Controls.Add(this.labelStat3Val);
            this.panelLeftHero.Controls.Add(this.labelStat3Title);
            this.panelLeftHero.Controls.Add(this.labelStat2Val);
            this.panelLeftHero.Controls.Add(this.labelStat2Title);
            this.panelLeftHero.Controls.Add(this.labelStat1Val);
            this.panelLeftHero.Controls.Add(this.labelStat1Title);
            this.panelLeftHero.Controls.Add(this.labelHeroSubtitle);
            this.panelLeftHero.Controls.Add(this.labelHeroTitle);
            this.panelLeftHero.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLeftHero.Location = new System.Drawing.Point(0, 0);
            this.panelLeftHero.Name = "panelLeftHero";
            this.panelLeftHero.Size = new System.Drawing.Size(460, 694);
            this.panelLeftHero.TabIndex = 0;
            // 
            // lblBigPcName
            // 
            this.lblBigPcName.AutoSize = true;
            this.lblBigPcName.Font = new System.Drawing.Font("Segoe UI Black", 72F, System.Drawing.FontStyle.Bold);
            this.lblBigPcName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(115)))), ((int)(((byte)(245)))));
            this.lblBigPcName.Location = new System.Drawing.Point(20, 15);
            this.lblBigPcName.Name = "lblBigPcName";
            this.lblBigPcName.Size = new System.Drawing.Size(415, 159);
            this.lblBigPcName.TabIndex = 8;
            this.lblBigPcName.Text = "PC-00";
            // 
            // labelStat3Val
            // 
            this.labelStat3Val.AutoSize = true;
            this.labelStat3Val.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.labelStat3Val.ForeColor = System.Drawing.Color.White;
            this.labelStat3Val.Location = new System.Drawing.Point(280, 560);
            this.labelStat3Val.Name = "labelStat3Val";
            this.labelStat3Val.Size = new System.Drawing.Size(77, 37);
            this.labelStat3Val.TabIndex = 6;
            this.labelStat3Val.Text = "24/7";
            // 
            // labelStat3Title
            // 
            this.labelStat3Title.AutoSize = true;
            this.labelStat3Title.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.labelStat3Title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(210)))), ((int)(((byte)(254)))));
            this.labelStat3Title.Location = new System.Drawing.Point(282, 592);
            this.labelStat3Title.Name = "labelStat3Title";
            this.labelStat3Title.Size = new System.Drawing.Size(58, 20);
            this.labelStat3Title.TabIndex = 7;
            this.labelStat3Title.Text = "Uptime";
            // 
            // labelStat2Val
            // 
            this.labelStat2Val.AutoSize = true;
            this.labelStat2Val.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.labelStat2Val.ForeColor = System.Drawing.Color.White;
            this.labelStat2Val.Location = new System.Drawing.Point(160, 560);
            this.labelStat2Val.Name = "labelStat2Val";
            this.labelStat2Val.Size = new System.Drawing.Size(96, 37);
            this.labelStat2Val.TabIndex = 4;
            this.labelStat2Val.Text = "< 5ms";
            // 
            // labelStat2Title
            // 
            this.labelStat2Title.AutoSize = true;
            this.labelStat2Title.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.labelStat2Title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(210)))), ((int)(((byte)(254)))));
            this.labelStat2Title.Location = new System.Drawing.Point(162, 592);
            this.labelStat2Title.Name = "labelStat2Title";
            this.labelStat2Title.Size = new System.Drawing.Size(71, 20);
            this.labelStat2Title.TabIndex = 5;
            this.labelStat2Title.Text = "Avg. Ping";
            // 
            // labelStat1Val
            // 
            this.labelStat1Val.AutoSize = true;
            this.labelStat1Val.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.labelStat1Val.ForeColor = System.Drawing.Color.White;
            this.labelStat1Val.Location = new System.Drawing.Point(40, 560);
            this.labelStat1Val.Name = "labelStat1Val";
            this.labelStat1Val.Size = new System.Drawing.Size(105, 37);
            this.labelStat1Val.TabIndex = 2;
            this.labelStat1Val.Text = "1 Gbps";
            // 
            // labelStat1Title
            // 
            this.labelStat1Title.AutoSize = true;
            this.labelStat1Title.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.labelStat1Title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(210)))), ((int)(((byte)(254)))));
            this.labelStat1Title.Location = new System.Drawing.Point(42, 592);
            this.labelStat1Title.Name = "labelStat1Title";
            this.labelStat1Title.Size = new System.Drawing.Size(88, 20);
            this.labelStat1Title.TabIndex = 3;
            this.labelStat1Title.Text = "Fiber Speed";
            // 
            // labelHeroSubtitle
            // 
            this.labelHeroSubtitle.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.labelHeroSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(231)))), ((int)(((byte)(255)))));
            this.labelHeroSubtitle.Location = new System.Drawing.Point(42, 423);
            this.labelHeroSubtitle.Name = "labelHeroSubtitle";
            this.labelHeroSubtitle.Size = new System.Drawing.Size(360, 60);
            this.labelHeroSubtitle.TabIndex = 1;
            this.labelHeroSubtitle.Text = "Log in to access high-speed internet, track your gaming session, and manage your " +
    "account seamlessly.";
            // 
            // labelHeroTitle
            // 
            this.labelHeroTitle.Font = new System.Drawing.Font("Segoe UI", 28F, System.Drawing.FontStyle.Bold);
            this.labelHeroTitle.ForeColor = System.Drawing.Color.White;
            this.labelHeroTitle.Location = new System.Drawing.Point(40, 220);
            this.labelHeroTitle.Name = "labelHeroTitle";
            this.labelHeroTitle.Size = new System.Drawing.Size(380, 203);
            this.labelHeroTitle.TabIndex = 0;
            this.labelHeroTitle.Text = "Your ultimate gaming experience.";
            // 
            // panelRightContent
            // 
            this.panelRightContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.panelRightContent.Controls.Add(this.panelLoginContainer);
            this.panelRightContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRightContent.Location = new System.Drawing.Point(460, 0);
            this.panelRightContent.Name = "panelRightContent";
            this.panelRightContent.Size = new System.Drawing.Size(730, 694);
            this.panelRightContent.TabIndex = 1;
            // 
            // panelLoginContainer
            // 
            this.panelLoginContainer.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.panelLoginContainer.Controls.Add(this.lblStatusBadge);
            this.panelLoginContainer.Controls.Add(this.lblCopyright);
            this.panelLoginContainer.Controls.Add(this.Button_Login);
            this.panelLoginContainer.Controls.Add(this.Button_Register);
            this.panelLoginContainer.Controls.Add(this.TextBox_Password);
            this.panelLoginContainer.Controls.Add(this.lblPassword);
            this.panelLoginContainer.Controls.Add(this.TextBox_Username);
            this.panelLoginContainer.Controls.Add(this.lblUsername);
            this.panelLoginContainer.Controls.Add(this.lblSubtitle);
            this.panelLoginContainer.Controls.Add(this.lblWelcome);
            this.panelLoginContainer.Controls.Add(this.lblCategory);
            this.panelLoginContainer.Location = new System.Drawing.Point(165, 100);
            this.panelLoginContainer.Name = "panelLoginContainer";
            this.panelLoginContainer.Size = new System.Drawing.Size(400, 480);
            this.panelLoginContainer.TabIndex = 0;
            // 
            // lblCopyright
            // 
            this.lblCopyright.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblCopyright.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblCopyright.Location = new System.Drawing.Point(6, 399);
            this.lblCopyright.Name = "lblCopyright";
            this.lblCopyright.Size = new System.Drawing.Size(388, 20);
            this.lblCopyright.TabIndex = 8;
            this.lblCopyright.Text = "© 2026 BNet Cafe Client · All rights reserved";
            this.lblCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblPassword.Location = new System.Drawing.Point(3, 205);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(76, 20);
            this.lblPassword.TabIndex = 5;
            this.lblPassword.Text = "Password";
            // 
            // lblUsername
            // 
            this.lblUsername.AutoSize = true;
            this.lblUsername.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblUsername.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblUsername.Location = new System.Drawing.Point(3, 125);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(182, 20);
            this.lblUsername.TabIndex = 3;
            this.lblUsername.Text = "Username or Member ID";
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblSubtitle.Location = new System.Drawing.Point(3, 76);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(263, 21);
            this.lblSubtitle.TabIndex = 2;
            this.lblSubtitle.Text = "Sign in to start your internet session.";
            // 
            // lblWelcome
            // 
            this.lblWelcome.AutoSize = true;
            this.lblWelcome.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.lblWelcome.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.lblWelcome.Location = new System.Drawing.Point(0, 30);
            this.lblWelcome.Name = "lblWelcome";
            this.lblWelcome.Size = new System.Drawing.Size(276, 50);
            this.lblWelcome.TabIndex = 1;
            this.lblWelcome.Text = "Welcome back";
            // 
            // lblCategory
            // 
            this.lblCategory.AutoSize = true;
            this.lblCategory.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblCategory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(80)))), ((int)(((byte)(236)))));
            this.lblCategory.Location = new System.Drawing.Point(3, 10);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(121, 20);
            this.lblCategory.TabIndex = 0;
            this.lblCategory.Text = "CLIENT PORTAL";
            // 
            // Button_Login
            // 
            this.Button_Login.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(80)))), ((int)(((byte)(236)))));
            this.Button_Login.BorderRadius = 12;
            this.Button_Login.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Button_Login.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Button_Login.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.Button_Login.ForeColor = System.Drawing.Color.White;
            this.Button_Login.Location = new System.Drawing.Point(6, 293);
            this.Button_Login.Name = "Button_Login";
            this.Button_Login.Size = new System.Drawing.Size(388, 46);
            this.Button_Login.TabIndex = 7;
            this.Button_Login.Text = "Sign In";
            this.Button_Login.UseVisualStyleBackColor = false;
            this.Button_Login.Click += new System.EventHandler(this.Button_Login_Click);
            // 
            // lblStatusBadge
            // 
            this.lblStatusBadge.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(252)))), ((int)(((byte)(231)))));
            this.lblStatusBadge.BorderRadius = 14;
            this.lblStatusBadge.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblStatusBadge.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(101)))), ((int)(((byte)(52)))));
            this.lblStatusBadge.Location = new System.Drawing.Point(104, 429);
            this.lblStatusBadge.Name = "lblStatusBadge";
            this.lblStatusBadge.Size = new System.Drawing.Size(190, 28);
            this.lblStatusBadge.TabIndex = 9;
            this.lblStatusBadge.Text = "● Station PC-01 Online";
            this.lblStatusBadge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Button_Register
            // 
            this.Button_Register.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.Button_Register.BorderRadius = 12;
            this.Button_Register.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Button_Register.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Button_Register.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.Button_Register.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.Button_Register.Location = new System.Drawing.Point(6, 345);
            this.Button_Register.Name = "Button_Register";
            this.Button_Register.Size = new System.Drawing.Size(388, 46);
            this.Button_Register.TabIndex = 8;
            this.Button_Register.Text = "Sign Up With Goolge";
            this.Button_Register.UseVisualStyleBackColor = false;
            this.Button_Register.Click += new System.EventHandler(this.Button_Register_Click);
            // 
            // TextBox_Password
            // 
            this.TextBox_Password.BackColor = System.Drawing.Color.White;
            this.TextBox_Password.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.TextBox_Password.BorderRadius = 10;
            this.TextBox_Password.Location = new System.Drawing.Point(6, 228);
            this.TextBox_Password.Name = "TextBox_Password";
            this.TextBox_Password.Padding = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this.TextBox_Password.PasswordChar = '•';
            this.TextBox_Password.Size = new System.Drawing.Size(388, 44);
            this.TextBox_Password.TabIndex = 6;
            // 
            // TextBox_Username
            // 
            this.TextBox_Username.BackColor = System.Drawing.Color.White;
            this.TextBox_Username.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.TextBox_Username.BorderRadius = 10;
            this.TextBox_Username.Location = new System.Drawing.Point(6, 148);
            this.TextBox_Username.Name = "TextBox_Username";
            this.TextBox_Username.Padding = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this.TextBox_Username.PasswordChar = '\0';
            this.TextBox_Username.Size = new System.Drawing.Size(388, 44);
            this.TextBox_Username.TabIndex = 4;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1190, 694);
            this.ControlBox = false;
            this.Controls.Add(this.panelRightContent);
            this.Controls.Add(this.panelLeftHero);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "BNet Cafe Client";
            this.TopMost = true;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.panelLeftHero.ResumeLayout(false);
            this.panelLeftHero.PerformLayout();
            this.panelRightContent.ResumeLayout(false);
            this.panelLoginContainer.ResumeLayout(false);
            this.panelLoginContainer.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelLeftHero;
        private System.Windows.Forms.Label lblBigPcName;
        private System.Windows.Forms.Label labelHeroTitle;
        private System.Windows.Forms.Label labelHeroSubtitle;
        private System.Windows.Forms.Label labelStat1Val;
        private System.Windows.Forms.Label labelStat1Title;
        private System.Windows.Forms.Label labelStat2Val;
        private System.Windows.Forms.Label labelStat2Title;
        private System.Windows.Forms.Label labelStat3Val;
        private System.Windows.Forms.Label labelStat3Title;
        private System.Windows.Forms.Panel panelRightContent;
        private System.Windows.Forms.Panel panelLoginContainer;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.Label lblWelcome;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblUsername;
        private BNet.Cafe.Client.Design.ModernTextBox TextBox_Username;
        private System.Windows.Forms.Label lblPassword;
        private BNet.Cafe.Client.Design.ModernTextBox TextBox_Password;
        private BNet.Cafe.Client.Design.ModernButton Button_Login;
        private BNet.Cafe.Client.Design.ModernButton Button_Register;
        private System.Windows.Forms.Label lblCopyright;
        private BNet.Cafe.Client.Design.ModernBadge lblStatusBadge;
    }
}