namespace BNet.Cafe.Client.Controls
{
    partial class RegisterControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblLoginLink = new BNet.Cafe.Client.Design.BufferedLabel();
            this.btnSubmit = new BNet.Cafe.Client.Design.BufferedButton();
            this.txtConfirmPassword = new BNet.Cafe.Client.Design.BufferedTextBox();
            this.lblConfirmPassword = new BNet.Cafe.Client.Design.BufferedLabel();
            this.txtPassword = new BNet.Cafe.Client.Design.BufferedTextBox();
            this.lblPassword = new BNet.Cafe.Client.Design.BufferedLabel();
            this.txtEmail = new BNet.Cafe.Client.Design.BufferedTextBox();
            this.lblEmail = new BNet.Cafe.Client.Design.BufferedLabel();
            this.txtFullName = new BNet.Cafe.Client.Design.BufferedTextBox();
            this.lblFullName = new BNet.Cafe.Client.Design.BufferedLabel();
            this.lblTitle = new BNet.Cafe.Client.Design.BufferedLabel();
            this.SuspendLayout();
            // 
            // lblLoginLink
            // 
            this.lblLoginLink.AutoSize = true;
            this.lblLoginLink.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblLoginLink.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Underline);
            this.lblLoginLink.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(147)))), ((int)(((byte)(197)))), ((int)(((byte)(253)))));
            this.lblLoginLink.Location = new System.Drawing.Point(75, 475);
            this.lblLoginLink.Name = "lblLoginLink";
            this.lblLoginLink.Size = new System.Drawing.Size(243, 21);
            this.lblLoginLink.TabIndex = 10;
            this.lblLoginLink.Text = "Already have an account? Log In";
            this.lblLoginLink.Click += new System.EventHandler(this.BtnBack_Click);
            // 
            // btnSubmit
            // 
            this.btnSubmit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSubmit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnSubmit.BorderRadius = 10;
            this.btnSubmit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSubmit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSubmit.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnSubmit.ForeColor = System.Drawing.Color.White;
            this.btnSubmit.Location = new System.Drawing.Point(34, 415);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(332, 46);
            this.btnSubmit.TabIndex = 9;
            this.btnSubmit.Text = "Register";
            this.btnSubmit.UseVisualStyleBackColor = false;
            this.btnSubmit.Click += new System.EventHandler(this.BtnSubmit_Click);
            // 
            // txtConfirmPassword
            // 
            this.txtConfirmPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtConfirmPassword.BackColor = System.Drawing.Color.White;
            this.txtConfirmPassword.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
            this.txtConfirmPassword.BorderRadius = 10;
            this.txtConfirmPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.txtConfirmPassword.Location = new System.Drawing.Point(34, 350);
            this.txtConfirmPassword.Name = "txtConfirmPassword";
            this.txtConfirmPassword.Padding = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this.txtConfirmPassword.PasswordChar = '•';
            this.txtConfirmPassword.Size = new System.Drawing.Size(332, 44);
            this.txtConfirmPassword.TabIndex = 8;
            // 
            // lblConfirmPassword
            // 
            this.lblConfirmPassword.AutoSize = true;
            this.lblConfirmPassword.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblConfirmPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(231)))), ((int)(((byte)(255)))));
            this.lblConfirmPassword.Location = new System.Drawing.Point(32, 325);
            this.lblConfirmPassword.Name = "lblConfirmPassword";
            this.lblConfirmPassword.Size = new System.Drawing.Size(148, 21);
            this.lblConfirmPassword.TabIndex = 7;
            this.lblConfirmPassword.Text = "Confirm Password";
            // 
            // txtPassword
            // 
            this.txtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPassword.BackColor = System.Drawing.Color.White;
            this.txtPassword.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
            this.txtPassword.BorderRadius = 10;
            this.txtPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.txtPassword.Location = new System.Drawing.Point(34, 270);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Padding = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this.txtPassword.PasswordChar = '•';
            this.txtPassword.Size = new System.Drawing.Size(332, 44);
            this.txtPassword.TabIndex = 6;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(231)))), ((int)(((byte)(255)))));
            this.lblPassword.Location = new System.Drawing.Point(32, 245);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(82, 21);
            this.lblPassword.TabIndex = 5;
            this.lblPassword.Text = "Password";
            // 
            // txtEmail
            // 
            this.txtEmail.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEmail.BackColor = System.Drawing.Color.White;
            this.txtEmail.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
            this.txtEmail.BorderRadius = 10;
            this.txtEmail.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.txtEmail.Location = new System.Drawing.Point(34, 190);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Padding = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this.txtEmail.PasswordChar = '\0';
            this.txtEmail.Size = new System.Drawing.Size(332, 44);
            this.txtEmail.TabIndex = 4;
            // 
            // lblEmail
            // 
            this.lblEmail.AutoSize = true;
            this.lblEmail.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblEmail.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(231)))), ((int)(((byte)(255)))));
            this.lblEmail.Location = new System.Drawing.Point(32, 165);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new System.Drawing.Size(154, 21);
            this.lblEmail.TabIndex = 3;
            this.lblEmail.Text = "Username or Email";
            // 
            // txtFullName
            // 
            this.txtFullName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFullName.BackColor = System.Drawing.Color.White;
            this.txtFullName.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
            this.txtFullName.BorderRadius = 10;
            this.txtFullName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.txtFullName.Location = new System.Drawing.Point(34, 110);
            this.txtFullName.Name = "txtFullName";
            this.txtFullName.Padding = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this.txtFullName.PasswordChar = '\0';
            this.txtFullName.Size = new System.Drawing.Size(332, 44);
            this.txtFullName.TabIndex = 2;
            // 
            // lblFullName
            // 
            this.lblFullName.AutoSize = true;
            this.lblFullName.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblFullName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(231)))), ((int)(((byte)(255)))));
            this.lblFullName.Location = new System.Drawing.Point(32, 85);
            this.lblFullName.Name = "lblFullName";
            this.lblFullName.Size = new System.Drawing.Size(88, 21);
            this.lblFullName.TabIndex = 1;
            this.lblFullName.Text = "Full Name";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(30, 25);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(257, 41);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Register Account";
            // 
            // RegisterControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.lblLoginLink);
            this.Controls.Add(this.btnSubmit);
            this.Controls.Add(this.txtConfirmPassword);
            this.Controls.Add(this.lblConfirmPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.lblEmail);
            this.Controls.Add(this.txtFullName);
            this.Controls.Add(this.lblFullName);
            this.Controls.Add(this.lblTitle);
            this.Name = "RegisterControl";
            this.Size = new System.Drawing.Size(400, 520);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private Design.BufferedLabel lblTitle;
        private Design.BufferedLabel lblFullName;
        private Design.BufferedLabel lblEmail;
        private Design.BufferedLabel lblPassword;
        private Design.BufferedLabel lblConfirmPassword;
        private Design.BufferedTextBox txtFullName;
        private Design.BufferedTextBox txtEmail;
        private Design.BufferedTextBox txtPassword;
        private Design.BufferedTextBox txtConfirmPassword;
        private Design.BufferedButton btnSubmit;
        private Design.BufferedLabel lblLoginLink;
    }
}