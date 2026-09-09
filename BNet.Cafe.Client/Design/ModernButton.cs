using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BNet.Cafe.Client.Design
{
    public class ModernButton : Button
    {
        public int BorderRadius { get; set; } = 12;

        public ModernButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.BackColor = Color.FromArgb(88, 80, 236);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            this.Cursor = Cursors.Hand;
            this.Size = new Size(388, 46);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
        }

        private void UpdateRegion()
        {
            using (GraphicsPath path = GetRoundedPath(this.ClientRectangle, BorderRadius))
            {
                this.Region = new Region(path);
            }
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}