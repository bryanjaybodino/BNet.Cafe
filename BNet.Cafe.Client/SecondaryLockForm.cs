using System.Drawing;
using System.Windows.Forms;
using BNet.Cafe.Client.Design;

namespace BNet.Cafe.Client
{
    public partial class SecondaryLockForm : Form
    {
        private PictureBox _pictureBox;

        public PictureBox WallpaperPictureBox => _pictureBox;

        public SecondaryLockForm(Rectangle bounds)
        {
            InitializeComponent();
            this.Bounds = bounds;
            InitializePictureBox();
        }

        private void InitializePictureBox()
        {
            _pictureBox = new BufferedPictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.FromArgb(15, 23, 42)
            };

            this.Controls.Add(_pictureBox);
        }
    }
}