namespace BNet.Cafe.Client.Design
{
    partial class NotificationForm
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
            this.Panel_BottomColorBar = new System.Windows.Forms.Panel();
            this.Label_Title = new System.Windows.Forms.Label();
            this.Label_Message = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Panel_BottomColorBar
            // 
            this.Panel_BottomColorBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(80)))), ((int)(((byte)(236)))));
            this.Panel_BottomColorBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Panel_BottomColorBar.Location = new System.Drawing.Point(0, 72);
            this.Panel_BottomColorBar.Name = "Panel_BottomColorBar";
            this.Panel_BottomColorBar.Size = new System.Drawing.Size(360, 3);
            this.Panel_BottomColorBar.TabIndex = 0;
            // 
            // Label_Title
            // 
            this.Label_Title.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Label_Title.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.Label_Title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.Label_Title.Location = new System.Drawing.Point(24, 12);
            this.Label_Title.Name = "Label_Title";
            this.Label_Title.Size = new System.Drawing.Size(312, 20);
            this.Label_Title.TabIndex = 1;
            this.Label_Title.Text = "Notification Title";
            // 
            // Label_Message
            // 
            this.Label_Message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Label_Message.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.Label_Message.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.Label_Message.Location = new System.Drawing.Point(24, 34);
            this.Label_Message.Name = "Label_Message";
            this.Label_Message.Size = new System.Drawing.Size(312, 24);
            this.Label_Message.TabIndex = 2;
            this.Label_Message.Text = "Notification message details display here.";
            // 
            // NotificationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.ClientSize = new System.Drawing.Size(360, 75);
            this.ControlBox = false;
            this.Controls.Add(this.Label_Message);
            this.Controls.Add(this.Label_Title);
            this.Controls.Add(this.Panel_BottomColorBar);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NotificationForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TopMost = true;
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.NotificationForm_Paint);
            this.Resize += new System.EventHandler(this.NotificationForm_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel Panel_BottomColorBar;
        private System.Windows.Forms.Label Label_Title;
        private System.Windows.Forms.Label Label_Message;
    }
}