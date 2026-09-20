using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BNet.Cafe.Client.Design
{
    public class ModernTextBox : Panel
    {
        private readonly TextBox innerTextBox = new TextBox();
        private Color _borderColor = Color.FromArgb(226, 232, 240);

        public int BorderRadius { get; set; } = 10;

        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                this.Invalidate(); // Forces the panel to immediately redraw its border
            }
        }

        public override string Text
        {
            get => innerTextBox.Text;
            set => innerTextBox.Text = value;
        }

        public char PasswordChar
        {
            get => innerTextBox.PasswordChar;
            set => innerTextBox.PasswordChar = value;
        }

        public ModernTextBox()
        {
            this.BackColor = Color.White;
            this.Padding = new Padding(12, 10, 12, 10);
            this.Size = new Size(388, 44);

            innerTextBox.BorderStyle = BorderStyle.None;
            innerTextBox.Dock = DockStyle.Fill;
            innerTextBox.Font = new Font("Segoe UI", 11f);
            innerTextBox.ForeColor = Color.FromArgb(15, 23, 42);

            this.Controls.Add(innerTextBox);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
        }

        private void UpdateRegion()
        {
            if (this.Width > 0 && this.Height > 0)
            {
                using (GraphicsPath path = GetRoundedPath(this.ClientRectangle, BorderRadius))
                {
                    this.Region = new Region(path);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Offset by 1px so the stroke isn't clipped out by this.Region
            Rectangle borderRect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);

            using (Pen pen = new Pen(_borderColor, 2f))
            using (GraphicsPath path = GetRoundedPath(borderRect, BorderRadius))
            {
                e.Graphics.DrawPath(pen, path);
            }
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            if (diameter > rect.Width) diameter = rect.Width;
            if (diameter > rect.Height) diameter = rect.Height;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}