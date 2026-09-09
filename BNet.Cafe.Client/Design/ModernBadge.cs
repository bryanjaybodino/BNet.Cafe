using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BNet.Cafe.Client.Design
{
    public class ModernBadge : Label
    {
        public int BorderRadius { get; set; } = 14;

        public ModernBadge()
        {
            this.BackColor = Color.FromArgb(220, 252, 231);
            this.ForeColor = Color.FromArgb(22, 101, 52);
            this.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            this.TextAlign = ContentAlignment.MiddleCenter;
            this.Size = new Size(190, 28);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
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