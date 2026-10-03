using System.Windows.Forms;

namespace BNet.Cafe.Client.Design
{
    public class BufferedTextBox : ModernTextBox
    {
        public BufferedTextBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);
            UpdateStyles();
        }
    }
}