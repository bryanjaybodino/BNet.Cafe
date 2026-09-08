namespace BNet.Cafe.Client
{
    partial class BNetCafeTimer
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
            this.components = new System.ComponentModel.Container();
            this.Label_ClientName = new System.Windows.Forms.Label();
            this.Label_CustomerName = new System.Windows.Forms.Label();
            this.Label_TotalHours = new System.Windows.Forms.Label();
            this.Label_TimerDisplay = new System.Windows.Forms.Label();
            this.Button_Logout = new System.Windows.Forms.Button();
            this.Timer_Countdown = new System.Windows.Forms.Timer(this.components);
            this.label_TotalAmount = new System.Windows.Forms.Label();
            this.Label_TimeoutDisplay = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Label_ClientName
            // 
            this.Label_ClientName.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.Label_ClientName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.Label_ClientName.Location = new System.Drawing.Point(170, 9);
            this.Label_ClientName.Name = "Label_ClientName";
            this.Label_ClientName.Size = new System.Drawing.Size(100, 33);
            this.Label_ClientName.TabIndex = 1;
            this.Label_ClientName.Text = "PC-01";
            this.Label_ClientName.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Label_CustomerName
            // 
            this.Label_CustomerName.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.Label_CustomerName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.Label_CustomerName.Location = new System.Drawing.Point(20, 180);
            this.Label_CustomerName.Name = "Label_CustomerName";
            this.Label_CustomerName.Size = new System.Drawing.Size(250, 18);
            this.Label_CustomerName.TabIndex = 2;
            this.Label_CustomerName.Text = "User: Guest";
            this.Label_CustomerName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Label_TotalHours
            // 
            this.Label_TotalHours.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.Label_TotalHours.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.Label_TotalHours.Location = new System.Drawing.Point(12, 10);
            this.Label_TotalHours.Name = "Label_TotalHours";
            this.Label_TotalHours.Size = new System.Drawing.Size(152, 20);
            this.Label_TotalHours.TabIndex = 3;
            this.Label_TotalHours.Text = "Purchased: 0 hrs";
            this.Label_TotalHours.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Label_TimerDisplay
            // 
            this.Label_TimerDisplay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.Label_TimerDisplay.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.Label_TimerDisplay.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.Label_TimerDisplay.Location = new System.Drawing.Point(16, 73);
            this.Label_TimerDisplay.Name = "Label_TimerDisplay";
            this.Label_TimerDisplay.Size = new System.Drawing.Size(250, 48);
            this.Label_TimerDisplay.TabIndex = 4;
            this.Label_TimerDisplay.Text = "00:00:00";
            this.Label_TimerDisplay.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Button_Logout
            // 
            this.Button_Logout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(139)))), ((int)(((byte)(92)))), ((int)(((byte)(246)))));
            this.Button_Logout.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Button_Logout.FlatAppearance.BorderSize = 0;
            this.Button_Logout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Button_Logout.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.Button_Logout.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Button_Logout.Location = new System.Drawing.Point(20, 212);
            this.Button_Logout.Name = "Button_Logout";
            this.Button_Logout.Size = new System.Drawing.Size(250, 32);
            this.Button_Logout.TabIndex = 6;
            this.Button_Logout.Text = "End Session / Logout";
            this.Button_Logout.UseVisualStyleBackColor = false;
            this.Button_Logout.Click += new System.EventHandler(this.Button_Logout_Click);
            // 
            // Timer_Countdown
            // 
            this.Timer_Countdown.Interval = 1000;
            this.Timer_Countdown.Tick += new System.EventHandler(this.Timer_Countdown_Tick);
            // 
            // label_TotalAmount
            // 
            this.label_TotalAmount.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.label_TotalAmount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.label_TotalAmount.Location = new System.Drawing.Point(12, 30);
            this.label_TotalAmount.Name = "label_TotalAmount";
            this.label_TotalAmount.Size = new System.Drawing.Size(152, 20);
            this.label_TotalAmount.TabIndex = 7;
            this.label_TotalAmount.Text = "Total Amount : 0.00";
            this.label_TotalAmount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Label_TimeoutDisplay
            // 
            this.Label_TimeoutDisplay.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.Label_TimeoutDisplay.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.Label_TimeoutDisplay.Location = new System.Drawing.Point(21, 130);
            this.Label_TimeoutDisplay.Name = "Label_TimeoutDisplay";
            this.Label_TimeoutDisplay.Size = new System.Drawing.Size(250, 18);
            this.Label_TimeoutDisplay.TabIndex = 8;
            this.Label_TimeoutDisplay.Text = "Timeout : 00:00";
            this.Label_TimeoutDisplay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // BNetCafeTimer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.ClientSize = new System.Drawing.Size(282, 265);
            this.ControlBox = false;
            this.Controls.Add(this.Label_TimeoutDisplay);
            this.Controls.Add(this.label_TotalAmount);
            this.Controls.Add(this.Button_Logout);
            this.Controls.Add(this.Label_TimerDisplay);
            this.Controls.Add(this.Label_TotalHours);
            this.Controls.Add(this.Label_CustomerName);
            this.Controls.Add(this.Label_ClientName);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BNetCafeTimer";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "BNetCafe";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BNetCafeTimer_FormClosing);
            this.Load += new System.EventHandler(this.BNetCafeTimer_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label Label_ClientName;
        private System.Windows.Forms.Label Label_CustomerName;
        private System.Windows.Forms.Label Label_TotalHours;
        private System.Windows.Forms.Label Label_TimerDisplay;
        private System.Windows.Forms.Button Button_Logout;
        private System.Windows.Forms.Timer Timer_Countdown;
        private System.Windows.Forms.Label label_TotalAmount;
        private System.Windows.Forms.Label Label_TimeoutDisplay;
    }
}