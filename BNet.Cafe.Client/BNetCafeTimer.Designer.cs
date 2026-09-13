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
            this.Panel_Header = new System.Windows.Forms.Panel();
            this.Label_ClientName = new System.Windows.Forms.Label();
            this.Label_CustomerName = new System.Windows.Forms.Label();
            this.Label_SessionType = new System.Windows.Forms.Label();
            this.Label_TimerDisplay = new System.Windows.Forms.Label();
            this.Panel_Details = new System.Windows.Forms.Panel();
            this.Label_StartTime = new System.Windows.Forms.Label();
            this.Label_TotalHours = new System.Windows.Forms.Label();
            this.Label_TimeoutDisplay = new System.Windows.Forms.Label();
            this.label_TotalAmount = new System.Windows.Forms.Label();
            this.Timer_Countdown = new System.Windows.Forms.Timer(this.components);
            this.Button_AccountAction = new BNet.Cafe.Client.Design.ModernButton();
            this.Button_Logout = new BNet.Cafe.Client.Design.ModernButton();
            this.Panel_Header.SuspendLayout();
            this.Panel_Details.SuspendLayout();
            this.SuspendLayout();
            // 
            // Panel_Header
            // 
            this.Panel_Header.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(80)))), ((int)(((byte)(236)))));
            this.Panel_Header.Controls.Add(this.Label_ClientName);
            this.Panel_Header.Controls.Add(this.Label_CustomerName);
            this.Panel_Header.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Panel_Header.Dock = System.Windows.Forms.DockStyle.Top;
            this.Panel_Header.Location = new System.Drawing.Point(0, 0);
            this.Panel_Header.Name = "Panel_Header";
            this.Panel_Header.Size = new System.Drawing.Size(300, 50);
            this.Panel_Header.TabIndex = 0;
            this.Panel_Header.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Panel_Header_MouseDown);
            // 
            // Label_ClientName
            // 
            this.Label_ClientName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.Label_ClientName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.Label_ClientName.Location = new System.Drawing.Point(211, 12);
            this.Label_ClientName.Name = "Label_ClientName";
            this.Label_ClientName.Size = new System.Drawing.Size(77, 25);
            this.Label_ClientName.TabIndex = 1;
            this.Label_ClientName.Text = "PC-00";
            this.Label_ClientName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Label_ClientName.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Panel_Header_MouseDown);
            // 
            // Label_CustomerName
            // 
            this.Label_CustomerName.AutoEllipsis = true;
            this.Label_CustomerName.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.Label_CustomerName.ForeColor = System.Drawing.Color.White;
            this.Label_CustomerName.Location = new System.Drawing.Point(12, 12);
            this.Label_CustomerName.Name = "Label_CustomerName";
            this.Label_CustomerName.Size = new System.Drawing.Size(202, 25);
            this.Label_CustomerName.TabIndex = 0;
            this.Label_CustomerName.Text = "Guest / Walk-in";
            this.Label_CustomerName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Label_CustomerName.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Panel_Header_MouseDown);
            // 
            // Label_SessionType
            // 
            this.Label_SessionType.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.Label_SessionType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(102)))), ((int)(((byte)(241)))));
            this.Label_SessionType.Location = new System.Drawing.Point(15, 58);
            this.Label_SessionType.Name = "Label_SessionType";
            this.Label_SessionType.Size = new System.Drawing.Size(270, 18);
            this.Label_SessionType.TabIndex = 1;
            this.Label_SessionType.Text = "PREPAID SESSION";
            this.Label_SessionType.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Label_TimerDisplay
            // 
            this.Label_TimerDisplay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.Label_TimerDisplay.Font = new System.Drawing.Font("Consolas", 24F, System.Drawing.FontStyle.Bold);
            this.Label_TimerDisplay.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(56)))), ((int)(((byte)(202)))));
            this.Label_TimerDisplay.Location = new System.Drawing.Point(15, 80);
            this.Label_TimerDisplay.Name = "Label_TimerDisplay";
            this.Label_TimerDisplay.Size = new System.Drawing.Size(270, 55);
            this.Label_TimerDisplay.TabIndex = 2;
            this.Label_TimerDisplay.Text = "00:00:00";
            this.Label_TimerDisplay.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Panel_Details
            // 
            this.Panel_Details.BackColor = System.Drawing.Color.White;
            this.Panel_Details.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Panel_Details.Controls.Add(this.Label_StartTime);
            this.Panel_Details.Controls.Add(this.Label_TotalHours);
            this.Panel_Details.Controls.Add(this.Label_TimeoutDisplay);
            this.Panel_Details.Controls.Add(this.label_TotalAmount);
            this.Panel_Details.Location = new System.Drawing.Point(15, 145);
            this.Panel_Details.Name = "Panel_Details";
            this.Panel_Details.Size = new System.Drawing.Size(270, 140);
            this.Panel_Details.TabIndex = 3;
            // 
            // Label_StartTime
            // 
            this.Label_StartTime.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.Label_StartTime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.Label_StartTime.Location = new System.Drawing.Point(10, 10);
            this.Label_StartTime.Name = "Label_StartTime";
            this.Label_StartTime.Size = new System.Drawing.Size(248, 22);
            this.Label_StartTime.TabIndex = 0;
            this.Label_StartTime.Text = "Started At: --:--";
            // 
            // Label_TotalHours
            // 
            this.Label_TotalHours.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.Label_TotalHours.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.Label_TotalHours.Location = new System.Drawing.Point(10, 40);
            this.Label_TotalHours.Name = "Label_TotalHours";
            this.Label_TotalHours.Size = new System.Drawing.Size(248, 22);
            this.Label_TotalHours.TabIndex = 1;
            this.Label_TotalHours.Text = "Purchased: 0 hr 0 min";
            // 
            // Label_TimeoutDisplay
            // 
            this.Label_TimeoutDisplay.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.Label_TimeoutDisplay.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.Label_TimeoutDisplay.Location = new System.Drawing.Point(10, 70);
            this.Label_TimeoutDisplay.Name = "Label_TimeoutDisplay";
            this.Label_TimeoutDisplay.Size = new System.Drawing.Size(248, 22);
            this.Label_TimeoutDisplay.TabIndex = 2;
            this.Label_TimeoutDisplay.Text = "Timeout: --:--";
            // 
            // label_TotalAmount
            // 
            this.label_TotalAmount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label_TotalAmount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.label_TotalAmount.Location = new System.Drawing.Point(10, 102);
            this.label_TotalAmount.Name = "label_TotalAmount";
            this.label_TotalAmount.Size = new System.Drawing.Size(248, 22);
            this.label_TotalAmount.TabIndex = 3;
            this.label_TotalAmount.Text = "Total Amount: ₱ 0.00";
            // 
            // Timer_Countdown
            // 
            this.Timer_Countdown.Interval = 1000;
            this.Timer_Countdown.Tick += new System.EventHandler(this.Timer_Countdown_Tick);
            // 
            // Button_AccountAction
            // 
            this.Button_AccountAction.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(102)))), ((int)(((byte)(241)))));
            this.Button_AccountAction.BorderRadius = 8;
            this.Button_AccountAction.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Button_AccountAction.FlatAppearance.BorderSize = 0;
            this.Button_AccountAction.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Button_AccountAction.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.Button_AccountAction.ForeColor = System.Drawing.Color.White;
            this.Button_AccountAction.Location = new System.Drawing.Point(15, 295);
            this.Button_AccountAction.Name = "Button_AccountAction";
            this.Button_AccountAction.Size = new System.Drawing.Size(270, 38);
            this.Button_AccountAction.TabIndex = 5;
            this.Button_AccountAction.Text = "Change Password";
            this.Button_AccountAction.UseVisualStyleBackColor = false;
            this.Button_AccountAction.Visible = false;
            this.Button_AccountAction.Click += new System.EventHandler(this.Button_AccountAction_Click);
            // 
            // Button_Logout
            // 
            this.Button_Logout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.Button_Logout.BorderRadius = 8;
            this.Button_Logout.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Button_Logout.FlatAppearance.BorderSize = 0;
            this.Button_Logout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Button_Logout.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.Button_Logout.ForeColor = System.Drawing.Color.Crimson;
            this.Button_Logout.Location = new System.Drawing.Point(15, 340);
            this.Button_Logout.Name = "Button_Logout";
            this.Button_Logout.Size = new System.Drawing.Size(270, 42);
            this.Button_Logout.TabIndex = 4;
            this.Button_Logout.Text = "End Session / Logout";
            this.Button_Logout.UseVisualStyleBackColor = false;
            this.Button_Logout.Click += new System.EventHandler(this.Button_Logout_Click);
            // 
            // BNetCafeTimer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.ClientSize = new System.Drawing.Size(300, 395);
            this.ControlBox = false;
            this.Controls.Add(this.Button_AccountAction);
            this.Controls.Add(this.Button_Logout);
            this.Controls.Add(this.Panel_Details);
            this.Controls.Add(this.Label_TimerDisplay);
            this.Controls.Add(this.Label_SessionType);
            this.Controls.Add(this.Panel_Header);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BNetCafeTimer";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BNetCafeTimer_FormClosing);
            this.Load += new System.EventHandler(this.BNetCafeTimer_Load);
            this.Panel_Header.ResumeLayout(false);
            this.Panel_Details.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel Panel_Header;
        private System.Windows.Forms.Label Label_ClientName;
        private System.Windows.Forms.Label Label_CustomerName;
        private System.Windows.Forms.Label Label_SessionType;
        private System.Windows.Forms.Label Label_TimerDisplay;
        private System.Windows.Forms.Panel Panel_Details;
        private System.Windows.Forms.Label Label_StartTime;
        private System.Windows.Forms.Label Label_TotalHours;
        private System.Windows.Forms.Label Label_TimeoutDisplay;
        private System.Windows.Forms.Label label_TotalAmount;
        private BNet.Cafe.Client.Design.ModernButton Button_AccountAction;
        private BNet.Cafe.Client.Design.ModernButton Button_Logout;
        private System.Windows.Forms.Timer Timer_Countdown;
    }
}