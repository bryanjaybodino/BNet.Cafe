using System.Windows.Forms;

namespace BNet.Cafe.Client.Design
{
    public class BufferedLabel : Label
    {
        public BufferedLabel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);
            UpdateStyles();
        }
    }
}