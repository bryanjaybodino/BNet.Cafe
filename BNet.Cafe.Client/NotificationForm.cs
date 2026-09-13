using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BNet.Cafe.Client.Design
{
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public partial class NotificationForm : Form
    {
        // Win32 Extended Window Styles
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_EX_TRANSPARENT = 0x00000020;

        private Timer autoCloseTimer;
        private Timer slideTimer;
        private int targetY;

        // Prevents stealing focus when shown
        protected override bool ShowWithoutActivation => true;

        // Makes window click-through and completely prevents focus activation
        protected override CreateParams CreateParams
        {
            get;
        }

        public NotificationForm(string title, string message, NotificationType type = NotificationType.Info, int durationMs = 3500)
        {
            InitializeComponent();
            ApplyNotificationDetails(title, message, type);
            AdjustLayoutForTextLength();
            ConfigureAnimation(durationMs);
        }

        private void ApplyNotificationDetails(string title, string message, NotificationType type)
        {
            Label_Title.Text = title;
            Label_Message.Text = message;

            // Color status bar palette matching application palette
            switch (type)
            {
                case NotificationType.Success:
                    Panel_BottomColorBar.BackColor = Color.FromArgb(16, 185, 129); // Emerald
                    break;
                case NotificationType.Warning:
                    Panel_BottomColorBar.BackColor = Color.FromArgb(245, 158, 11); // Amber
                    break;
                case NotificationType.Error:
                    Panel_BottomColorBar.BackColor = Color.FromArgb(239, 68, 68); // Red
                    break;
                case NotificationType.Info:
                default:
                    Panel_BottomColorBar.BackColor = Color.FromArgb(88, 80, 236); // Primary Indigo Header Color
                    break;
            }

            // Click-to-dismiss handlers removed to avoid taking mouse focus away from active games
        }

        private void AdjustLayoutForTextLength()
        {
            // Calculate height for multi-line text with wider max bounds (402px)
            Size maxTextSize = new Size(402, 1000);
            Size titleMeasured = TextRenderer.MeasureText(Label_Title.Text, Label_Title.Font, maxTextSize, TextFormatFlags.WordBreak);
            Size messageMeasured = TextRenderer.MeasureText(Label_Message.Text, Label_Message.Font, maxTextSize, TextFormatFlags.WordBreak);

            Label_Title.Height = titleMeasured.Height;
            Label_Message.Top = Label_Title.Bottom + 4;
            Label_Message.Height = messageMeasured.Height;

            // Dynamic bottom extension so long text never truncates
            int requiredHeight = Label_Message.Bottom + 20;
            this.Height = Math.Max(75, requiredHeight);
        }

        private GraphicsPath CreateNotchPath()
        {
            GraphicsPath path = new GraphicsPath();
            int bottomIndent = 50; // Scaled inset distance for wider notification slants
            int notchHeight = 100;   // Vertical height of the slant slope

            path.StartFigure();
            path.AddLine(0, 0, this.Width, 0); // Flat Top edge flush with screen top
            path.AddLine(this.Width, 0, this.Width, this.Height - notchHeight); // Right edge down to slant start
            path.AddLine(this.Width, this.Height - notchHeight, this.Width - bottomIndent, this.Height); // Slanted bottom-right corner
            path.AddLine(this.Width - bottomIndent, this.Height, bottomIndent, this.Height); // Bottom flat edge \___________________/
            path.AddLine(bottomIndent, this.Height, 0, this.Height - notchHeight); // Slanted bottom-left corner
            path.AddLine(0, this.Height - notchHeight, 0, 0); // Left edge back up to top
            path.CloseFigure();

            return path;
        }

        private void UpdateNotchShapeRegion()
        {
            using (GraphicsPath path = CreateNotchPath())
            {
                this.Region = new Region(path);
            }
        }

        private void ConfigureAnimation(int durationMs)
        {
            Rectangle screenArea = Screen.PrimaryScreen.WorkingArea;

            int centerX = screenArea.Left + (screenArea.Width - this.Width) / 2;
            targetY = screenArea.Top; // Flush to top screen edge
            int startY = screenArea.Top - this.Height;

            this.Location = new Point(centerX, startY);

            // Smooth drop-down animation
            slideTimer = new Timer { Interval = 10 };
            slideTimer.Tick += (s, e) =>
            {
                if (this.Top < targetY)
                {
                    this.Top += Math.Max(2, (targetY - this.Top) / 3);
                }
                else
                {
                    this.Top = targetY;
                    slideTimer.Stop();
                }
            };

            // Auto-dismiss timer
            autoCloseTimer = new Timer { Interval = durationMs };
            autoCloseTimer.Tick += (s, e) => CloseNotification();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            UpdateNotchShapeRegion();
            slideTimer?.Start();
            autoCloseTimer?.Start();
        }

        private void NotificationForm_Resize(object sender, EventArgs e)
        {
            UpdateNotchShapeRegion();
        }

        private void NotificationForm_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw clean border following the bottom-notched shape
            using (Pen borderPen = new Pen(Color.FromArgb(203, 213, 225), 1.5f))
            using (GraphicsPath path = CreateNotchPath())
            {
                e.Graphics.DrawPath(borderPen, path);
            }
        }

        private void CloseNotification()
        {
            autoCloseTimer?.Stop();
            slideTimer?.Stop();
            this.Close();
            this.Dispose();
        }

        public static void Show(string title, string message, NotificationType type = NotificationType.Info, int durationMs = 3500)
        {
            var alert = new NotificationForm(title, message, type, durationMs);
            alert.Show();
        }
    }
}