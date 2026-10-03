using System.Windows.Forms;

namespace BNet.Cafe.Client.Design
{
    public class BufferedButton : ModernButton
    {
        public BufferedButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);
            UpdateStyles();
        }
    }
}