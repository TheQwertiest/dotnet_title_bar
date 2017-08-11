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
using System.Windows.Forms;
using fooTitle.Config;

namespace fooTitle {
    public class Display : PerPixelAlphaForm {
        
        private readonly System.ComponentModel.Container _components = null;
        private Bitmap _canvasBitmap;
        public Graphics Canvas;

        public int SnapDist = 10;

        public AnimationManager AnimManager { get; }

        private bool dragging = false;
        private int dragX;
        private int dragY;
        public int MyOpacity;
        private int minOpacity;

        private readonly DockAnchor _dockAnchor;

        /// <summary>
        /// The opacity in normal state
        /// </summary>
        private readonly ConfInt normalOpacity = ConfValuesManager.CreateIntValue("display/normalOpacity", 255, 5, 255);
        /// <summary>
        /// The opacity when the mouse is over foo_title
        /// </summary>
        private readonly ConfInt overOpacity = ConfValuesManager.CreateIntValue("display/overOpacity", 255, 5, 255);
        /// <summary>
        /// The opacity when the foo_title display is triggered
        /// </summary>
        private ConfInt triggerOpacity = ConfValuesManager.CreateIntValue("display/overOpacity", 255, 5, 255);
        /// <summary>
        /// The z position of the window - either always on top or on the bottom.
        /// </summary>
        public ConfEnum<Win32.WindowPosition> WindowPosition = ConfValuesManager.CreateEnumValue("display/windowPosition", Win32.WindowPosition.Topmost);
        /// <summary>
        /// Indicates the need to draw anchor
        /// </summary>
        private readonly ConfBool _shouldDrawAnchor = ConfValuesManager.CreateBoolValue("display/drawAnchor", false);

        public Display(int width, int height) {
            InitializeComponent();

            _canvasBitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Canvas = Graphics.FromImage(_canvasBitmap);
            Canvas.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            this.Size = new Size(width, height);
            this.Left = 250;
            this.Top = 0;

            MyOpacity = normalOpacity.Value;
            minOpacity = normalOpacity.Value;
            AnimManager = new AnimationManager(this);

            normalOpacity.OnChanged += normalOpacity_OnChanged;
            overOpacity.OnChanged += overOpacity_OnChanged;
            WindowPosition.OnChanged += windowPosition_OnChanged;

            _dockAnchor = new DockAnchor(this);
            SetWindowsPos(WindowPosition.Value);
        }

        void windowPosition_OnChanged(string name) {
            SetWindowsPos(WindowPosition.Value);
        }

        private void normalOpacity_OnChanged(string name) {
            MyOpacity = normalOpacity.Value;
            if (minOpacity != 0)
            {
                minOpacity = normalOpacity.Value;
            }
            Display_OnPaint(null,null);
        }

        private void overOpacity_OnChanged(string name)
        {
            Display_OnPaint(null, null);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing) {
            if (disposing) {
                _components?.Dispose();
                AnimManager.Dispose();

                // need to remove this from the events on the configuration values
                normalOpacity.OnChanged -= normalOpacity_OnChanged;
                overOpacity.OnChanged -= overOpacity_OnChanged;
                WindowPosition.OnChanged -= windowPosition_OnChanged;
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            // 
            // Display
            // 
            this.AutoScaleBaseSize = new Size(5, 13);
            this.ClientSize = new Size(480, 96);
            this.ControlBox = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Icon = fooManagedWrapper.CManagedWrapper.getInstance().GetMainIcon();
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Display";
            this.ShowInTaskbar = false;
            this.Text = "foo_title";
            this.MouseDown += Display_MouseDown;
            this.MouseUp += Display_MouseUp;
            this.MouseMove += Display_MouseMove;
            this.Activated += Display_Activated;
            this.Paint += Display_OnPaint;
        }
        #endregion

        public void FrameRedraw() {
            Canvas.ResetClip();
            if (_shouldDrawAnchor.Value)
            {
                _dockAnchor.Draw();
            }
            //AnimManager.UpdateOpacity();
            SetBitmap(_canvasBitmap, (byte)MyOpacity);
            Canvas.Clear(Color.Transparent);
        }

        #region Bottom

        private void Display_Activated(object sender, EventArgs e) {
            if (WindowPosition.Value == Win32.WindowPosition.Bottom) {
                SetWindowsPos(Win32.WindowPosition.Bottom);
            }
        }
#pragma warning disable 0168, 219, 67
        protected override void WndProc(ref Message m) {
            if (WindowPosition.Value == Win32.WindowPosition.Bottom) {
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

            const int WM_CLOSE = 0x0010;
            if (m.Msg == WM_CLOSE) {
                return;
            }

            base.WndProc(ref m);
        }
#pragma warning restore 0168, 219, 67
        #endregion

        #region Dragging
        private void Display_MouseDown(object sender, MouseEventArgs e) {
            if (Main.GetInstance().CanDragDisplay) {
                dragging = true;
                dragX = e.X;
                dragY = e.Y;
            }
        }

        private void Display_MouseUp(object sender, MouseEventArgs e) {
            if (dragging) {
                dragging = false;

                // save position
                Main.GetInstance().SavePosition();
            }
        }

        private void Display_MouseMove(object sender, MouseEventArgs e) {
            if (dragging)
            {
                Point mouse = this.PointToScreen(new Point(e.X, e.Y));
                Screen screen = Screen.FromPoint(mouse);

                this.Left = DoSnapping(mouse.X - dragX, this.Width, screen.WorkingArea.Left, screen.WorkingArea.Right);
                this.Top = DoSnapping(mouse.Y - dragY, this.Height, screen.WorkingArea.Top, screen.WorkingArea.Bottom);
            }
        }

        // snapping
        private int DoSnapping(int pos, int size, int border, int oppositeBorder) {
            if (Main.GetInstance().edgeSnapEnabled && (Control.ModifierKeys & Keys.Control) == 0) {
                int borderDist = Math.Abs(pos - border);
                int oppositeBorderDist = Math.Abs(pos + size - oppositeBorder);
                if (borderDist < SnapDist || oppositeBorderDist < SnapDist) {
                    if (borderDist < oppositeBorderDist) {
                        return border;
                    } else {
                        return oppositeBorder - size;
                    }
                }
            }
            return pos;
        }
        #endregion

        private void Display_OnPaint(object sender, EventArgs e)
        {
            Main.GetInstance().DrawForm();
            if (this.Visible)
            {
                FrameRedraw();
            }
        }

        internal void SetSize(int width, int height) {
            this.Width = width;
            this.Height = height;

            _canvasBitmap = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Canvas = Graphics.FromImage(_canvasBitmap);
            FrameRedraw();

            AdjustPositionByAnchor();
        }

        internal void InitializeAnchor(DockAnchor.Type anchorType, float anchorDx, float anchorDy)
        {
            _dockAnchor.Initialize(anchorType, anchorDx, anchorDy);
        }

        internal Win32.Point GetAnchorPosition()
        {
            return _dockAnchor.GetPosition();
        }

        internal void SetAnchorPosition(int anchorX, int anchorY)
        {
            _dockAnchor.SetPosition(anchorX, anchorY);
            Win32.Point windowPos = _dockAnchor.GetWindowPosition();
            if (this.Top != windowPos.y)
            {
                this.Top = windowPos.y;
            }
            if (this.Left != windowPos.x)
            {
                this.Left = windowPos.x;
            }
        }

        private void AdjustPositionByAnchor()
        {
            Win32.Point windowPos = _dockAnchor.GetWindowPosition();
            if (this.Top != windowPos.y)
            {
                this.Top = windowPos.y;
            }
            if (this.Left != windowPos.x)
            {
                this.Left = windowPos.x;
            }
        }

        internal void ReadjustPosition()
        {
            Screen screen = Screen.FromControl(this);
            if (this.Top < 0)
            {
                this.Top = 0;
            }
            else if (this.Top > screen.WorkingArea.Bottom)
            {
                this.Top = screen.WorkingArea.Bottom - this.Height;
            }

            if (this.Left < 0)
            {
                this.Left = 0;
            }
            else if (this.Top > screen.WorkingArea.Bottom)
            {
                this.Left = screen.WorkingArea.Right - this.Width;
            }
            Refresh();
        }
    }
}
