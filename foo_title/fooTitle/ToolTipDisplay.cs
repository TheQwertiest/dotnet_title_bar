using System;
using System.Drawing;
using System.Windows.Forms;

namespace fooTitle {
    public class ToolTipDisplay : PerPixelAlphaForm {

        private static Color backgroundColor = Color.White;
        private static Color textColor = Color.FromArgb(87, 87, 87);
        private static Font textFont = SystemFonts.CaptionFont;
        private static Pen borderPen = new Pen(Color.FromArgb(118, 118, 118));

        private string text;
        private Rectangle borderRectangle;

        public ToolTipDisplay() {
            this.InitializeComponent();
        }

        private void InitializeComponent() {
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
            this.ResumeLayout(false);
        }

        private void updateBitmap() {
            Bitmap b = new Bitmap(Width, Height);
            using (Graphics g = Graphics.FromImage(b)) {
                g.Clear(backgroundColor);
                g.DrawRectangle(borderPen, borderRectangle);
                TextRenderer.DrawText(g, text, textFont, new Point(3, 2), textColor, backgroundColor);
            }
            SetBitmap(b);
        }

        public void SetText(string newText) {
            if (newText != null && text != newText) {
                text = newText;
                SizeF textSize = TextRenderer.MeasureText(text, textFont);
                Width = (int)textSize.Width + 6;
                Height = (int)textSize.Height + 5;
                borderRectangle.Width = Width - 1;
                borderRectangle.Height = Height - 1;
                updateBitmap();
            }
        }

        protected override bool ShowWithoutActivation {
            get {
                return true;
            }
        }
    }
}
