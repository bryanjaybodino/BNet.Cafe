using System;
using System.Drawing;
using System.Windows.Forms;

namespace BNet.Cafe.Client.Controls
{
    public partial class InsufficientBalanceModal : Form
    {
        public InsufficientBalanceModal()
        {
            InitializeComponent();
            ConfigureStyle();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
        }

        private void ConfigureStyle()
        {
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Account Balance Notice";
            this.TopMost = true;
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // If the user clicks the 'X' button or closes it without clicking OK, 
            // treat it as a cancel/reset action.
            if (this.DialogResult != DialogResult.OK)
            {
                this.DialogResult = DialogResult.Cancel;
            }
        }
    }
}