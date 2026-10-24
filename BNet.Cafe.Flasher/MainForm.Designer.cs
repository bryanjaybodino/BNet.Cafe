namespace BNet.Cafe.Flasher
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tblInputs = new System.Windows.Forms.TableLayoutPanel();
            this.lblSketch = new System.Windows.Forms.Label();
            this.txtSketch = new System.Windows.Forms.TextBox();
            this.btnSketch = new System.Windows.Forms.Button();
            this.lblPort = new System.Windows.Forms.Label();
            this.cmbPort = new System.Windows.Forms.ComboBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.flowButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCompile = new System.Windows.Forms.Button();
            this.btnUpload = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.pnlEditor = new System.Windows.Forms.Panel();
            this.lblEditor = new System.Windows.Forms.Label();
            this.pnlLog = new System.Windows.Forms.Panel();
            this.txtLog = new System.Windows.Forms.RichTextBox();
            this.progress = new System.Windows.Forms.ProgressBar();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.txtEditor = new BNet.Cafe.Flasher.InoEditor();
            this.tblInputs.SuspendLayout();
            this.flowButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.pnlEditor.SuspendLayout();
            this.pnlLog.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // tblInputs
            // 
            this.tblInputs.AutoSize = true;
            this.tblInputs.ColumnCount = 3;
            this.tblInputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tblInputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblInputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tblInputs.Controls.Add(this.lblSketch, 0, 0);
            this.tblInputs.Controls.Add(this.txtSketch, 1, 0);
            this.tblInputs.Controls.Add(this.btnSketch, 2, 0);
            this.tblInputs.Controls.Add(this.lblPort, 0, 1);
            this.tblInputs.Controls.Add(this.cmbPort, 1, 1);
            this.tblInputs.Controls.Add(this.btnRefresh, 2, 1);
            this.tblInputs.Dock = System.Windows.Forms.DockStyle.Top;
            this.tblInputs.Location = new System.Drawing.Point(0, 0);
            this.tblInputs.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tblInputs.Name = "tblInputs";
            this.tblInputs.Padding = new System.Windows.Forms.Padding(13, 12, 13, 12);
            this.tblInputs.RowCount = 2;
            this.tblInputs.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblInputs.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblInputs.Size = new System.Drawing.Size(1179, 104);
            this.tblInputs.TabIndex = 0;
            // 
            // lblSketch
            // 
            this.lblSketch.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSketch.AutoSize = true;
            this.lblSketch.Location = new System.Drawing.Point(17, 24);
            this.lblSketch.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSketch.Name = "lblSketch";
            this.lblSketch.Size = new System.Drawing.Size(83, 16);
            this.lblSketch.TabIndex = 0;
            this.lblSketch.Text = "Sketch (.ino):";
            // 
            // txtSketch
            // 
            this.txtSketch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSketch.Location = new System.Drawing.Point(108, 16);
            this.txtSketch.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtSketch.Name = "txtSketch";
            this.txtSketch.ReadOnly = true;
            this.txtSketch.Size = new System.Drawing.Size(918, 22);
            this.txtSketch.TabIndex = 2;
            // 
            // btnSketch
            // 
            this.btnSketch.AutoSize = true;
            this.btnSketch.Location = new System.Drawing.Point(1034, 16);
            this.btnSketch.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSketch.Name = "btnSketch";
            this.btnSketch.Size = new System.Drawing.Size(100, 32);
            this.btnSketch.TabIndex = 3;
            this.btnSketch.Text = "Open...";
            this.btnSketch.UseVisualStyleBackColor = true;
            this.btnSketch.Click += new System.EventHandler(this.btnSketch_Click);
            // 
            // lblPort
            // 
            this.lblPort.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(17, 64);
            this.lblPort.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(66, 16);
            this.lblPort.TabIndex = 4;
            this.lblPort.Text = "COM port:";
            // 
            // cmbPort
            // 
            this.cmbPort.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPort.FormattingEnabled = true;
            this.cmbPort.Location = new System.Drawing.Point(108, 56);
            this.cmbPort.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cmbPort.Name = "cmbPort";
            this.cmbPort.Size = new System.Drawing.Size(918, 24);
            this.cmbPort.TabIndex = 6;
            this.cmbPort.SelectedIndexChanged += new System.EventHandler(this.cmbPort_SelectedIndexChanged);
            // 
            // btnRefresh
            // 
            this.btnRefresh.AutoSize = true;
            this.btnRefresh.Location = new System.Drawing.Point(1034, 56);
            this.btnRefresh.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(128, 32);
            this.btnRefresh.TabIndex = 7;
            this.btnRefresh.Text = "Detect Board";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // flowButtons
            // 
            this.flowButtons.AutoSize = true;
            this.flowButtons.Controls.Add(this.btnSave);
            this.flowButtons.Controls.Add(this.btnCompile);
            this.flowButtons.Controls.Add(this.btnUpload);
            this.flowButtons.Controls.Add(this.btnCancel);
            this.flowButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowButtons.Location = new System.Drawing.Point(0, 104);
            this.flowButtons.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.flowButtons.Name = "flowButtons";
            this.flowButtons.Padding = new System.Windows.Forms.Padding(13, 0, 13, 10);
            this.flowButtons.Size = new System.Drawing.Size(1179, 61);
            this.flowButtons.TabIndex = 1;
            // 
            // btnSave
            // 
            this.btnSave.AutoSize = true;
            this.btnSave.Enabled = false;
            this.btnSave.Location = new System.Drawing.Point(17, 4);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSave.Name = "btnSave";
            this.btnSave.Padding = new System.Windows.Forms.Padding(13, 5, 13, 5);
            this.btnSave.Size = new System.Drawing.Size(153, 42);
            this.btnSave.TabIndex = 8;
            this.btnSave.Text = "Save (Ctrl+S)";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCompile
            // 
            this.btnCompile.AutoSize = true;
            this.btnCompile.Location = new System.Drawing.Point(178, 4);
            this.btnCompile.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCompile.Name = "btnCompile";
            this.btnCompile.Padding = new System.Windows.Forms.Padding(13, 5, 13, 5);
            this.btnCompile.Size = new System.Drawing.Size(116, 42);
            this.btnCompile.TabIndex = 9;
            this.btnCompile.Text = "Compile";
            this.btnCompile.UseVisualStyleBackColor = true;
            this.btnCompile.Click += new System.EventHandler(this.btnCompile_Click);
            // 
            // btnUpload
            // 
            this.btnUpload.AutoSize = true;
            this.btnUpload.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.btnUpload.Location = new System.Drawing.Point(302, 4);
            this.btnUpload.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Padding = new System.Windows.Forms.Padding(13, 5, 13, 5);
            this.btnUpload.Size = new System.Drawing.Size(221, 43);
            this.btnUpload.TabIndex = 10;
            this.btnUpload.Text = "Compile && Upload";
            this.btnUpload.UseVisualStyleBackColor = true;
            this.btnUpload.Click += new System.EventHandler(this.btnUpload_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.AutoSize = true;
            this.btnCancel.Enabled = false;
            this.btnCancel.Location = new System.Drawing.Point(531, 4);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Padding = new System.Windows.Forms.Padding(13, 5, 13, 5);
            this.btnCancel.Size = new System.Drawing.Size(105, 42);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // splitMain
            // 
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 165);
            this.splitMain.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.splitMain.Name = "splitMain";
            this.splitMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.Controls.Add(this.pnlEditor);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.pnlLog);
            this.splitMain.Size = new System.Drawing.Size(1179, 661);
            this.splitMain.SplitterDistance = 417;
            this.splitMain.SplitterWidth = 5;
            this.splitMain.TabIndex = 2;
            // 
            // pnlEditor
            // 
            this.pnlEditor.Controls.Add(this.txtEditor);
            this.pnlEditor.Controls.Add(this.lblEditor);
            this.pnlEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlEditor.Location = new System.Drawing.Point(0, 0);
            this.pnlEditor.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pnlEditor.Name = "pnlEditor";
            this.pnlEditor.Padding = new System.Windows.Forms.Padding(13, 0, 13, 5);
            this.pnlEditor.Size = new System.Drawing.Size(1179, 417);
            this.pnlEditor.TabIndex = 0;
            // 
            // lblEditor
            // 
            this.lblEditor.AutoSize = true;
            this.lblEditor.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblEditor.Location = new System.Drawing.Point(13, 0);
            this.lblEditor.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblEditor.Name = "lblEditor";
            this.lblEditor.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblEditor.Size = new System.Drawing.Size(229, 20);
            this.lblEditor.TabIndex = 13;
            this.lblEditor.Text = "Sketch editor - open a .ino file to edit it";
            // 
            // pnlLog
            // 
            this.pnlLog.Controls.Add(this.txtLog);
            this.pnlLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLog.Location = new System.Drawing.Point(0, 0);
            this.pnlLog.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pnlLog.Name = "pnlLog";
            this.pnlLog.Padding = new System.Windows.Forms.Padding(13, 5, 13, 12);
            this.pnlLog.Size = new System.Drawing.Size(1179, 239);
            this.pnlLog.TabIndex = 0;
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.Color.Black;
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtLog.ForeColor = System.Drawing.Color.Gainsboro;
            this.txtLog.Location = new System.Drawing.Point(13, 5);
            this.txtLog.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.Size = new System.Drawing.Size(1153, 222);
            this.txtLog.TabIndex = 13;
            this.txtLog.Text = "";
            this.txtLog.WordWrap = false;
            // 
            // progress
            // 
            this.progress.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progress.Location = new System.Drawing.Point(0, 826);
            this.progress.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.progress.Name = "progress";
            this.progress.Size = new System.Drawing.Size(1179, 10);
            this.progress.TabIndex = 3;
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip.Location = new System.Drawing.Point(0, 836);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusStrip.Size = new System.Drawing.Size(1179, 26);
            this.statusStrip.TabIndex = 4;
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(50, 20);
            this.lblStatus.Text = "Ready";
            // 
            // txtEditor
            // 
            this.txtEditor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtEditor.Enabled = false;
            this.txtEditor.Location = new System.Drawing.Point(13, 20);
            this.txtEditor.Margin = new System.Windows.Forms.Padding(4);
            this.txtEditor.Name = "txtEditor";
            this.txtEditor.Size = new System.Drawing.Size(1153, 392);
            this.txtEditor.TabIndex = 12;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1179, 862);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.flowButtons);
            this.Controls.Add(this.tblInputs);
            this.Controls.Add(this.progress);
            this.Controls.Add(this.statusStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MinimumSize = new System.Drawing.Size(1034, 728);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "BNet Cafe - Arduino Firmware Flasher";
            this.tblInputs.ResumeLayout(false);
            this.tblInputs.PerformLayout();
            this.flowButtons.ResumeLayout(false);
            this.flowButtons.PerformLayout();
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.pnlEditor.ResumeLayout(false);
            this.pnlEditor.PerformLayout();
            this.pnlLog.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tblInputs;
        private System.Windows.Forms.Label lblSketch;
        private System.Windows.Forms.TextBox txtSketch;
        private System.Windows.Forms.Button btnSketch;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.ComboBox cmbPort;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.FlowLayoutPanel flowButtons;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCompile;
        private System.Windows.Forms.Button btnUpload;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.Panel pnlEditor;
        private BNet.Cafe.Flasher.InoEditor txtEditor;
        private System.Windows.Forms.Label lblEditor;
        private System.Windows.Forms.Panel pnlLog;
        private System.Windows.Forms.RichTextBox txtLog;
        private System.Windows.Forms.ProgressBar progress;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
    }
}