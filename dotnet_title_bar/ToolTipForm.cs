using System.Drawing;
using System.Windows.Forms;

namespace fooTitle
{
    public class ToolTipForm : PerPixelAlphaForm
    {
        protected override bool ShowWithoutActivation => true;

        private static readonly Color _backgroundColor = Color.White;
        private static readonly Color _textColor = Color.FromArgb(87, 87, 87);
        private static readonly Font _textFont = SystemFonts.CaptionFont;
        private static readonly Pen _borderPen = new(Color.FromArgb(118, 118, 118));

        private string _text;
        private Rectangle _borderRectangle;

        public ToolTipForm()
        {
            this.InitializeComponent();
        }
        public void SetText(string newText)
        {
            if (newText != null && _text != newText)
            {
                _text = newText;
                SizeF textSize = TextRenderer.MeasureText(_text, _textFont);
                Width = (int)textSize.Width + 6;
                Height = (int)textSize.Height + 5;
                _borderRectangle.Width = Width - 1;
                _borderRectangle.Height = Height - 1;
                UpdateBitmap();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            //
            // ToolTipDisplay
            //
            this.ClientSize = new Size(10, 10);
            this.ControlBox = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Name = "ToolTipDisplay";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "ToolTipDisplay";
            this.TopMost = false;

            // Passthrough hack
            int initialStyle = Win32.GetWindowLong(this.Handle, -20);
            _ = Win32.SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);

            this.ResumeLayout(false);
        }

        private void UpdateBitmap()
        {
            Bitmap b = new Bitmap(Width, Height);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.Clear(_backgroundColor);
                g.DrawRectangle(_borderPen, _borderRectangle);
                TextRenderer.DrawText(g, _text, _textFont, new Point(3, 2), _textColor, _backgroundColor);
            }
            SetBitmap(b);
        }
    }
}
