/*
    Copyright 2005 - 2006 Roman Plasil
	http://foo-title.sourceforge.net
    This file is part of foo_title.

    foo_title is free software; you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation; either version 2.1 of the License, or
    (at your option) any later version.

    foo_title is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with foo_title; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace fooTitle
{
    public class Display : PerPixelAlphaForm
	{
		private System.ComponentModel.Container components = null;
		private System.Drawing.Bitmap canvasBitmap = null;
		public System.Drawing.Graphics Canvas = null;

        public int SnapDist = 10;

		private bool dragging = false;
		private int dragX;
        private int dragY;
        private int opacity;

        public class Fade {
            protected int startVal;
            protected int stopVal;
            protected int length;
            protected long startTime;

            protected float phase;

            public Fade(int _startVal, int _stopVal, int _length) {
                startVal = _startVal;
                stopVal = _stopVal;
                startTime = System.DateTime.Now.Ticks / 10000;
                length = _length;
            }

            public int GetValue() {
                long now = System.DateTime.Now.Ticks / 10000;
                
                // special cases
                if (now == startTime)
                    return startVal;
                if (length == 0) {
                    phase = 1;  // end it now
                }

                // normal processing
                phase = (float)(now - startTime) / (float)length;
                if (phase > 1)
                    phase = 1;
                return (int)(startVal + phase * (stopVal - startVal));
            }

            public bool Done() {
                return phase >= 1;
            }
        }

        protected Fade opacityFade;
        
		public Display(int width, int height)
		{
			InitializeComponent();

			canvasBitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Canvas = Graphics.FromImage(canvasBitmap);
            Canvas.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

			this.Size = new Size(width, height);
			this.Left = 250;
			this.Top = 0;

            opacity = Main.GetInstance().NormalOpacity;

            SetWindowsPos(Main.GetInstance().WindowPosition);

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// Display
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(480, 96);
			this.ControlBox = false;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Display";
			this.ShowInTaskbar = false;
			this.Text = "foo_title";
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Display_MouseDown);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Display_MouseUp);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Display_MouseMove);
            this.MouseEnter += new EventHandler(Display_MouseEnter);
            this.MouseLeave += new EventHandler(Display_MouseLeave);
            this.Activated += new EventHandler(Display_Activated);
		}


		#endregion

		public void FrameRedraw() {
            frameUpdateOpacity();
            SetBitmap(canvasBitmap, (byte)opacity);
            Canvas.Clear(Color.Transparent);
        }
        
#region Bottom
        void Display_Activated(object sender, EventArgs e) {
            if (Main.GetInstance().WindowPosition == Win32.WindowPosition.Bottom) {
                SetWindowsPos(Win32.WindowPosition.Bottom);
            }
        }

        protected override void WndProc(ref Message m) {
            if (Main.GetInstance().WindowPosition == Win32.WindowPosition.Bottom) {
                const int WM_MOUSEACTIVATE = 0x21;
                const int MA_ACTIVATE = 1;
                const int MA_ACTIVATEANDEAT = 2;
                const int MA_NOACTIVATE = 3;
                const int MA_NOACTIVATEANDEAT = 4;

                if (m.Msg == WM_MOUSEACTIVATE) {
                    m.Result = (IntPtr)MA_NOACTIVATE;
                    return;
                }
            }

            base.WndProc(ref m);
        }
#endregion


        #region Dragging
        private void Display_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
			dragging = true;
			dragX = e.X;
            dragY = e.Y;
		}

		private void Display_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
			dragging = false;

            // save position
            Main.GetInstance().SavePosition();
		}

		private void Display_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (dragging) {
                Point mouse = this.PointToScreen(new Point(e.X, e.Y));

                Left = doSnapping(mouse.X - dragX, 0);
                Top = doSnapping(mouse.Y - dragY, 0);
            }
		}

        // snapping
        private int doSnapping(int pos, int border) {
            if (pos - border < SnapDist) {
                return border;
            }
            return pos;
        }
		#endregion

        #region Opacity

        protected void frameUpdateOpacity() {
            if (opacityFade != null) {
                opacity = opacityFade.GetValue();
                if (opacityFade.Done())
                    opacityFade = null;
            }
        }

        void Display_MouseLeave(object sender, EventArgs e) {
            opacityFade = new Fade(Main.GetInstance().OverOpacity, Main.GetInstance().NormalOpacity, Main.GetInstance().FadeLength);
        }

        void Display_MouseEnter(object sender, EventArgs e) {
            opacityFade = new Fade(Main.GetInstance().NormalOpacity, Main.GetInstance().OverOpacity, Main.GetInstance().FadeLength);
        }

        public void SetNormalOpacity(int value) {
            opacity = value;
            opacityFade = null;
        }
        #endregion


        internal void SetSize(int width, int height) {
            this.Width = width;
            this.Height = height;

            canvasBitmap = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Canvas = Graphics.FromImage(canvasBitmap);
        }
    }
}
