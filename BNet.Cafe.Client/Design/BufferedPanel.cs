using System.Windows.Forms;

namespace BNet.Cafe.Client.Design
{
    public class BufferedPanel : Panel
    {
        public BufferedPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);
            UpdateStyles();
        }
    }
}