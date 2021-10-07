/*
*  This file is part of foo_title.
*  Copyright 2017 TheQwertiest (https://github.com/TheQwertiest/foo_title)
*  
*  This library is free software; you can redistribute it and/or
*  modify it under the terms of the GNU Lesser General Public
*  License as published by the Free Software Foundation; either
*  version 2.1 of the License, or (at your option) any later version.
*  
*  This library is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  
*  
*  See the file COPYING included with this distribution for more
*  information.
*/

using System.Drawing;
using System.Windows.Forms;

namespace fooTitle
{
    public class ToolTipDisplay : PerPixelAlphaForm
    {

        protected override bool ShowWithoutActivation => true;

        private static readonly Color backgroundColor = Color.White;
        private static readonly Color textColor = Color.FromArgb(87, 87, 87);
        private static readonly Font textFont = SystemFonts.CaptionFont;
        private static readonly Pen borderPen = new Pen(Color.FromArgb(118, 118, 118));

        private string _text;
        private Rectangle _borderRectangle;

        public ToolTipDisplay()
        {
            this.InitializeComponent();
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
            Win32.SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);

            this.ResumeLayout(false);
        }

        private void UpdateBitmap()
        {
            Bitmap b = new Bitmap(Width, Height);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.Clear(backgroundColor);
                g.DrawRectangle(borderPen, _borderRectangle);
                TextRenderer.DrawText(g, _text, textFont, new Point(3, 2), textColor, backgroundColor);
            }
            SetBitmap(b);
        }

        public void SetText(string newText)
        {
            if (newText != null && _text != newText)
            {
                _text = newText;
                SizeF textSize = TextRenderer.MeasureText(_text, textFont);
                Width = (int)textSize.Width + 6;
                Height = (int)textSize.Height + 5;
                _borderRectangle.Width = Width - 1;
                _borderRectangle.Height = Height - 1;
                UpdateBitmap();
            }
        }
    }
}
