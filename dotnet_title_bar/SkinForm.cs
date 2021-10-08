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
using fooTitle.Config;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace fooTitle
{
    public class SkinForm : PerPixelAlphaForm
    {
        static public Graphics CanvasForMeasure;
        public Graphics Canvas;

        public int CurrentOpacity;

        private static readonly int _snapDistance = 10;
        private readonly ConfInt _normalOpacity = Configs.Display_NormalOpacity;
        private readonly ConfInt _overOpacity = Configs.Display_MouseOverOpacity;
        private readonly ConfBool _shouldDrawAnchor = Configs.Display_ShouldDrawAnchor;
        private readonly ConfEnum<Win32.WindowPosition> _windowPosition = Configs.Display_WindowPosition;
        private readonly DockAnchor _dockAnchor;
        /// <summary>
        /// Automatically handles reshowing dotnet_title_bar if it's supposed to be always on top.
        /// </summary>
        private readonly RepeatedShowing _reshower;

        private Bitmap _canvasBitmap;
        private bool _isDragging = false;
        private int _dragX;
        private int _dragY;
        private int _minOpacity;

#if DEBUG
        private int _perf_frames = 0;
        private readonly Stopwatch _perf_watch = new();
        private long _perf_lastTime = 0;
#endif

        public SkinForm(int width, int height)
        {
            this.AutoScaleMode = AutoScaleMode.None;
            InitializeComponent();

            _canvasBitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Canvas = Graphics.FromImage(_canvasBitmap);
            Canvas.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            if (CanvasForMeasure == null)
            {
                var canvasForMeasureBitmap = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                CanvasForMeasure = Graphics.FromImage(canvasForMeasureBitmap);
                CanvasForMeasure.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            }

            this.Size = new Size(width, height);
            this.Left = 250;
            this.Top = 0;

            AnimManager = new AnimationManager(this);
            CurrentOpacity = _normalOpacity.Value;
            _minOpacity = _normalOpacity.Value;

            _normalOpacity.Changed += NormalOpacity_Changed_EventHandler;
            _overOpacity.Changed += OverOpacity_Changed_EventHandler;
            _windowPosition.Changed += WindowPosition_Changed_EventHandler;
            _shouldDrawAnchor.Changed += ShouldDrawAnchor_Changed_EventHandler;

            _reshower = new RepeatedShowing(this);
            _dockAnchor = new DockAnchor(this);
            SetWindowsPos(_windowPosition.Value);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                AnimManager.Dispose();

                _normalOpacity.Changed -= NormalOpacity_Changed_EventHandler;
                _overOpacity.Changed -= OverOpacity_Changed_EventHandler;
                _windowPosition.Changed -= WindowPosition_Changed_EventHandler;
                _shouldDrawAnchor.Changed -= ShouldDrawAnchor_Changed_EventHandler;
            }
            base.Dispose(disposing);
        }

        public AnimationManager AnimManager { get; }

        public void FrameRedraw()
        {
#if DEBUG
            ++_perf_frames;
            if (_perf_watch.ElapsedMilliseconds - _perf_lastTime >= 1000)
            {
                Debug.Write(_perf_frames + " frames\n");
                _perf_frames = 0;
                _perf_lastTime = _perf_watch.ElapsedMilliseconds;
            }
#endif

            Canvas.ResetClip();
            if (_shouldDrawAnchor.Value)
            {
                _dockAnchor.Draw();
            }
            SetBitmap(_canvasBitmap, (byte)CurrentOpacity);
            Canvas.Clear(Color.Transparent);
        }

        public void SetSize(int width, int height)
        {
            this.Width = width;
            this.Height = height;

            _canvasBitmap = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Canvas = Graphics.FromImage(_canvasBitmap);
            // Redraw immediately to avoid flickering because of the size change
            FrameRedraw();

            AdjustPositionByAnchor();
        }

        public void InitializeAnchor(DockAnchorType anchorType, float anchorDx, float anchorDy)
        {
            _dockAnchor.Initialize(anchorType, anchorDx, anchorDy);
        }

        public Win32.Point GetAnchorPosition()
        {
            return _dockAnchor.GetPosition();
        }

        public void SetAnchorPosition(int anchorX, int anchorY)
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

        public void ReadjustPosition()
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

            Main.Get().RedrawTitleBar(true);
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
            this.AutoScaleBaseSize = new Size(5, 13);
            this.ClientSize = new Size(480, 96);
            this.ControlBox = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Icon = Main.Get().Fb2kUtils.Fb2kIcon();
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Display";
            this.ShowInTaskbar = false;
            this.Text = "dotnet_title_bar";
            this.MouseDown += Display_MouseDown_EventHandler;
            this.MouseUp += Display_MouseUp_EventHandler;
            this.MouseMove += Display_MouseMove_EventHandler;
            this.Activated += Display_Activated_EventHandler;
            this.Paint += Display_Paint_EventHandler;
#if DEBUG
            _perf_watch.Start();
#endif
        }
        #endregion

#pragma warning disable 0168, 219, 67
        protected override void WndProc(ref Message m)
        {
            if (_windowPosition.Value == Win32.WindowPosition.Bottom)
            {
                const int WM_MOUSEACTIVATE = 0x21;
                const int MA_ACTIVATE = 1;
                const int MA_ACTIVATEANDEAT = 2;
                const int MA_NOACTIVATE = 3;
                const int MA_NOACTIVATEANDEAT = 4;

                if (m.Msg == WM_MOUSEACTIVATE)
                {
                    m.Result = (IntPtr)MA_NOACTIVATE;
                    return;
                }
            }

            const int WM_CLOSE = 0x0010;
            if (m.Msg == WM_CLOSE)
            {
                return;
            }

            base.WndProc(ref m);
        }
#pragma warning restore 0168, 219, 67
        void WindowPosition_Changed_EventHandler(string name)
        {
            SetWindowsPos(_windowPosition.Value);
        }

        private void NormalOpacity_Changed_EventHandler(string name)
        {
            CurrentOpacity = _normalOpacity.Value;
            if (_minOpacity != 0)
            {
                _minOpacity = _normalOpacity.Value;
            }
            Display_Paint_EventHandler(null, null);
        }

        private void OverOpacity_Changed_EventHandler(string name)
        {
            Display_Paint_EventHandler(null, null);
        }

        private void ShouldDrawAnchor_Changed_EventHandler(string name)
        {
            Main.Get().RedrawTitleBar(true);
        }

        private void Display_Activated_EventHandler(object sender, EventArgs e)
        {
            if (_windowPosition.Value == Win32.WindowPosition.Bottom)
            {
                SetWindowsPos(Win32.WindowPosition.Bottom);
            }
        }
        private void Display_MouseDown_EventHandler(object sender, MouseEventArgs e)
        {
            if (Main.Get().CanDragDisplay)
            {
                _isDragging = true;
                _dragX = e.X;
                _dragY = e.Y;
            }
        }

        private void Display_MouseUp_EventHandler(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;

                // save position
                Main.Get().SavePosition();
            }
        }

        private void Display_MouseMove_EventHandler(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point mouse = this.PointToScreen(new Point(e.X, e.Y));
                Screen screen = Screen.FromPoint(mouse);

                this.Left = DoSnapping(mouse.X - _dragX, this.Width, screen.WorkingArea.Left, screen.WorkingArea.Right);
                this.Top = DoSnapping(mouse.Y - _dragY, this.Height, screen.WorkingArea.Top, screen.WorkingArea.Bottom);
            }
        }

        private void Display_Paint_EventHandler(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                Main.Get().DrawTitleBar(Canvas);
                FrameRedraw();
            }
        }

        private int DoSnapping(int pos, int size, int border, int oppositeBorder)
        {
            if (Configs.Display_ShouldEdgeSnap.Value && (Control.ModifierKeys & Keys.Control) == 0)
            {
                int borderDist = Math.Abs(pos - border);
                int oppositeBorderDist = Math.Abs(pos + size - oppositeBorder);
                if (borderDist < _snapDistance || oppositeBorderDist < _snapDistance)
                {
                    if (borderDist < oppositeBorderDist)
                    {
                        return border;
                    }
                    else
                    {
                        return oppositeBorder - size;
                    }
                }
            }
            return pos;
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
    }
}
