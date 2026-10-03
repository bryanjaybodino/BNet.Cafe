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
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.Panel_LoginCard = new System.Windows.Forms.Panel();
            this.lblDismissHint = new System.Windows.Forms.Label();
            this.badgeCountdown = new BNet.Cafe.Client.Design.ModernBadge();
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
            // lblSubtitle
            // 
            this.lblSubtitle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.BackColor = System.Drawing.Color.Transparent;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(210)))), ((int)(((byte)(254)))));
            this.lblSubtitle.Location = new System.Drawing.Point(780, 20);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(138, 20);
            this.lblSubtitle.TabIndex = 2;
            this.lblSubtitle.Text = "BNET CAFE TIMER";
            // 
            // Panel_LoginCard
            // 
            this.Panel_LoginCard.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Panel_LoginCard.AutoSize = false; // Changed from true to false to allow custom height/width sizing
            this.Panel_LoginCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.Panel_LoginCard.Location = new System.Drawing.Point(395, 172);
            this.Panel_LoginCard.Name = "Panel_LoginCard";
            this.Panel_LoginCard.Size = new System.Drawing.Size(400, 360);
            this.Panel_LoginCard.TabIndex = 3;
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
            // badgeCountdown
            // 
            this.badgeCountdown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.badgeCountdown.BackColor = System.Drawing.Color.Transparent;
            this.badgeCountdown.BorderRadius = 14;
            this.badgeCountdown.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.badgeCountdown.ForeColor = System.Drawing.Color.White;
            this.badgeCountdown.Location = new System.Drawing.Point(776, 47);
            this.badgeCountdown.Name = "badgeCountdown";
            this.badgeCountdown.Size = new System.Drawing.Size(380, 42);
            this.badgeCountdown.TabIndex = 1;
            this.badgeCountdown.Text = "--:--:--";
            this.badgeCountdown.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.ClientSize = new System.Drawing.Size(1190, 694);
            this.Controls.Add(this.badgeCountdown);
            this.Controls.Add(this.lblDismissHint);
            this.Controls.Add(this.Panel_LoginCard);
            this.Controls.Add(this.lblSubtitle);
            this.Controls.Add(this.lblBigPcName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MainForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblBigPcName;
        private Design.ModernBadge badgeCountdown;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Panel Panel_LoginCard;
        private System.Windows.Forms.Label lblDismissHint;
    }
}