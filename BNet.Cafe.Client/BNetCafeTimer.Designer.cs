namespace BNet.Cafe.Client
{
    partial class BNetCafeTimer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.sidebarPanel = new System.Windows.Forms.Panel();
            this.sidebarBorder = new System.Windows.Forms.Panel();
            this.lblAppTitle = new System.Windows.Forms.Label();
            this.mainContentPanel = new System.Windows.Forms.Panel();
            this.timerCard = new System.Windows.Forms.Panel();
            this.lblPc = new System.Windows.Forms.Label();
            this.cbPcSelection = new System.Windows.Forms.ComboBox();
            this.lblTime = new System.Windows.Forms.Label();
            this.numMinutes = new System.Windows.Forms.NumericUpDown();
            this.lblTimerDisplay = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.sessionTimer = new System.Windows.Forms.Timer(this.components);

            this.sidebarPanel.SuspendLayout();
            this.mainContentPanel.SuspendLayout();
            this.timerCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMinutes)).BeginInit();
            this.SuspendLayout();

            // 
            // Color Palette
            // 
            System.Drawing.Color primary = System.Drawing.ColorTranslator.FromHtml("#3b82f6");
            System.Drawing.Color primaryDark = System.Drawing.ColorTranslator.FromHtml("#1e40af");
            System.Drawing.Color secondary = System.Drawing.ColorTranslator.FromHtml("#8b5cf6");
            System.Drawing.Color bgLight = System.Drawing.ColorTranslator.FromHtml("#f8fafc");
            System.Drawing.Color bgLightSecondary = System.Drawing.ColorTranslator.FromHtml("#ffffff");
            System.Drawing.Color bgLightTertiary = System.Drawing.ColorTranslator.FromHtml("#f1f5f9");
            System.Drawing.Color textLight = System.Drawing.ColorTranslator.FromHtml("#1e293b");
            System.Drawing.Color textLightSecondary = System.Drawing.ColorTranslator.FromHtml("#64748b");
            System.Drawing.Color borderLight = System.Drawing.ColorTranslator.FromHtml("#e2e8f0");
            System.Drawing.Color btnPrimaryText = System.Drawing.ColorTranslator.FromHtml("#ffffff");

            // 
            // sidebarPanel
            // 
            this.sidebarPanel.BackColor = bgLightSecondary;
            this.sidebarPanel.Controls.Add(this.sidebarBorder);
            this.sidebarPanel.Controls.Add(this.lblAppTitle);
            this.sidebarPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.sidebarPanel.Location = new System.Drawing.Point(0, 0);
            this.sidebarPanel.Name = "sidebarPanel";
            this.sidebarPanel.Padding = new System.Windows.Forms.Padding(15);
            this.sidebarPanel.Size = new System.Drawing.Size(220, 500);
            this.sidebarPanel.TabIndex = 0;

            // 
            // sidebarBorder
            // 
            this.sidebarBorder.BackColor = borderLight;
            this.sidebarBorder.Dock = System.Windows.Forms.DockStyle.Right;
            this.sidebarBorder.Location = new System.Drawing.Point(219, 0);
            this.sidebarBorder.Name = "sidebarBorder";
            this.sidebarBorder.Size = new System.Drawing.Size(1, 500);
            this.sidebarBorder.TabIndex = 1;

            // 
            // lblAppTitle
            // 
            this.lblAppTitle.AutoSize = true;
            this.lblAppTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblAppTitle.ForeColor = primaryDark;
            this.lblAppTitle.Location = new System.Drawing.Point(20, 20);
            this.lblAppTitle.Name = "lblAppTitle";
            this.lblAppTitle.Size = new System.Drawing.Size(120, 30);
            this.lblAppTitle.TabIndex = 0;
            this.lblAppTitle.Text = "BNet Cafe";

            // 
            // mainContentPanel
            // 
            this.mainContentPanel.BackColor = bgLight;
            this.mainContentPanel.Controls.Add(this.timerCard);
            this.mainContentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainContentPanel.Location = new System.Drawing.Point(220, 0);
            this.mainContentPanel.Name = "mainContentPanel";
            this.mainContentPanel.Padding = new System.Windows.Forms.Padding(30);
            this.mainContentPanel.Size = new System.Drawing.Size(580, 500);
            this.mainContentPanel.TabIndex = 1;

            // 
            // timerCard
            // 
            this.timerCard.BackColor = bgLightSecondary;
            this.timerCard.Controls.Add(this.lblPc);
            this.timerCard.Controls.Add(this.cbPcSelection);
            this.timerCard.Controls.Add(this.lblTime);
            this.timerCard.Controls.Add(this.numMinutes);
            this.timerCard.Controls.Add(this.lblTimerDisplay);
            this.timerCard.Controls.Add(this.btnStart);
            this.timerCard.Controls.Add(this.btnStop);
            this.timerCard.Location = new System.Drawing.Point(40, 30);
            this.timerCard.Name = "timerCard";
            this.timerCard.Padding = new System.Windows.Forms.Padding(20);
            this.timerCard.Size = new System.Drawing.Size(480, 360);
            this.timerCard.TabIndex = 0;
            this.timerCard.Paint += new System.Windows.Forms.PaintEventHandler(this.TimerCard_Paint);

            // 
            // lblPc
            // 
            this.lblPc.AutoSize = true;
            this.lblPc.ForeColor = textLightSecondary;
            this.lblPc.Location = new System.Drawing.Point(20, 20);
            this.lblPc.Name = "lblPc";
            this.lblPc.Size = new System.Drawing.Size(107, 19);
            this.lblPc.TabIndex = 0;
            this.lblPc.Text = "Select PC Station:";

            // 
            // cbPcSelection
            // 
            this.cbPcSelection.BackColor = bgLightTertiary;
            this.cbPcSelection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPcSelection.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbPcSelection.ForeColor = textLight;
            this.cbPcSelection.FormattingEnabled = true;
            this.cbPcSelection.Items.AddRange(new object[] {
            "PC-01 (Gaming VIP)",
            "PC-02 (Gaming VIP)",
            "PC-03 (Standard)",
            "PC-04 (Standard)"});
            this.cbPcSelection.Location = new System.Drawing.Point(20, 45);
            this.cbPcSelection.Name = "cbPcSelection";
            this.cbPcSelection.Size = new System.Drawing.Size(430, 25);
            this.cbPcSelection.TabIndex = 1;

            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.ForeColor = textLightSecondary;
            this.lblTime.Location = new System.Drawing.Point(20, 90);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(155, 19);
            this.lblTime.TabIndex = 2;
            this.lblTime.Text = "Rental Duration (Minutes):";

            // 
            // numMinutes
            // 
            this.numMinutes.BackColor = bgLightTertiary;
            this.numMinutes.ForeColor = textLight;
            this.numMinutes.Location = new System.Drawing.Point(20, 115);
            this.numMinutes.Maximum = new decimal(new int[] { 600, 0, 0, 0 });
            this.numMinutes.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numMinutes.Name = "numMinutes";
            this.numMinutes.Size = new System.Drawing.Size(430, 25);
            this.numMinutes.TabIndex = 3;
            this.numMinutes.Value = new decimal(new int[] { 60, 0, 0, 0 });

            // 
            // lblTimerDisplay
            // 
            this.lblTimerDisplay.BackColor = bgLightTertiary;
            this.lblTimerDisplay.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Bold);
            this.lblTimerDisplay.ForeColor = primary;
            this.lblTimerDisplay.Location = new System.Drawing.Point(20, 160);
            this.lblTimerDisplay.Name = "lblTimerDisplay";
            this.lblTimerDisplay.Size = new System.Drawing.Size(430, 80);
            this.lblTimerDisplay.TabIndex = 4;
            this.lblTimerDisplay.Text = "00:00:00";
            this.lblTimerDisplay.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // 
            // btnStart
            // 
            this.btnStart.BackColor = primary;
            this.btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStart.FlatAppearance.BorderSize = 0;
            this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStart.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnStart.ForeColor = btnPrimaryText;
            this.btnStart.Location = new System.Drawing.Point(20, 270);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(200, 45);
            this.btnStart.TabIndex = 5;
            this.btnStart.Text = "Start Session";
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);

            // 
            // btnStop
            // 
            this.btnStop.BackColor = secondary;
            this.btnStop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStop.FlatAppearance.BorderSize = 0;
            this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStop.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnStop.ForeColor = btnPrimaryText;
            this.btnStop.Location = new System.Drawing.Point(250, 270);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(200, 45);
            this.btnStop.TabIndex = 6;
            this.btnStop.Text = "End Session";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this.BtnStop_Click);

            // 
            // sessionTimer
            // 
            this.sessionTimer.Interval = 1000;
            this.sessionTimer.Tick += new System.EventHandler(this.SessionTimer_Tick);

            // 
            // BNetCafeTimer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = bgLight;
            this.ClientSize = new System.Drawing.Size(800, 500);
            this.Controls.Add(this.mainContentPanel);
            this.Controls.Add(this.sidebarPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Name = "BNetCafeTimer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "BNet Cafe - PC Rental Timer";
            this.sidebarPanel.ResumeLayout(false);
            this.sidebarPanel.PerformLayout();
            this.mainContentPanel.ResumeLayout(false);
            this.timerCard.ResumeLayout(false);
            this.timerCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMinutes)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel sidebarPanel;
        private System.Windows.Forms.Panel sidebarBorder;
        private System.Windows.Forms.Label lblAppTitle;
        private System.Windows.Forms.Panel mainContentPanel;
        private System.Windows.Forms.Panel timerCard;
        private System.Windows.Forms.Label lblPc;
        private System.Windows.Forms.ComboBox cbPcSelection;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.NumericUpDown numMinutes;
        private System.Windows.Forms.Label lblTimerDisplay;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Timer sessionTimer;
    }
}