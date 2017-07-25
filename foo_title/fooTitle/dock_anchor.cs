/*
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
using System.Windows.Forms;
using System.Drawing;

namespace fooTitle
{
    internal class DockAnchor
    {
        public enum Type
        {
            None = 0,
            Left = 1,
            Right = 1 << 1,
            Top = 1 << 2,
            Bottom = 1 << 3,
            Center = 1 << 4,            
        }

        private Display display_ = null;
        private double anchor_dx_ = 0;
        private double anchor_dy_ = 0;
        private int anchor_x_ = 0;
        private int anchor_y_ = 0;
        private DockAnchor.Type anchorType_ = DockAnchor.Type.Left | DockAnchor.Type.Top;

        internal DockAnchor(Display display)
        {
            display_ = display;
        }

        internal void Initialize(DockAnchor.Type type, double dx, double dy)
        {
            anchorType_ = type;

            anchor_dx_ = 0.5/*dx*/;
            anchor_dy_ = 0.5/*dy*/;

            if ((anchorType_ & DockAnchor.Type.Left) != 0)
            {
                anchor_dx_ = 0;
            }
            else if ((anchorType_ & DockAnchor.Type.Right) != 0)
            {
                anchor_dx_ = 1;
            }
            else if ((anchorType_ & DockAnchor.Type.Center) != 0)
            {
                anchor_dx_ = 0.5;
            }

            if ((anchorType_ & DockAnchor.Type.Top) != 0)
            {
                anchor_dy_ = 0;
            }
            else if ((anchorType_ & DockAnchor.Type.Bottom) != 0)
            {
                anchor_dy_ = 1;
            }
            else if ((anchorType_ & DockAnchor.Type.Center) != 0)
            {
                anchor_dy_ = 0.5;
            }

            Win32.Point anchorPos = CalculatePositionFromWindow();
            anchor_x_ = anchorPos.x;
            anchor_y_ = anchorPos.y;
        }
        internal Win32.Point GetPosition()
        {
            return CalculatePositionFromWindow();
        }

        internal void SetPosition(int anchor_x, int anchor_y)
        {
            anchor_x_ = anchor_x;
            anchor_y_ = anchor_y;
        }

        internal Win32.Point GetWindowPosition()
        {
            int x = anchor_x_ - (int)(display_.Width * anchor_dx_);
            int y = anchor_y_ - (int)(display_.Height * anchor_dy_);

            return new Win32.Point(x, y);
        }

        internal void Draw()
        {
            int anchorRelativeX = Math.Min((int)(anchor_dx_ * display_.Width), display_.Width - 1);
            int anchorRelativeY = Math.Min((int)(anchor_dy_ * display_.Height), display_.Height - 1);

            Color anchorCol = Color.FromArgb(0xFF, 0xFF, 0xFF, 0x70);

            display_.Canvas.FillRectangle(new SolidBrush(anchorCol), anchorRelativeX - 1, anchorRelativeY - 1, 3, 3);
            display_.Canvas.DrawLine(new Pen(anchorCol), anchorRelativeX - 25, anchorRelativeY, anchorRelativeX + 25, anchorRelativeY);
            display_.Canvas.DrawLine(new Pen(anchorCol), anchorRelativeX, anchorRelativeY - 25, anchorRelativeX, anchorRelativeY + 25);
        }

        private Win32.Point CalculatePositionFromWindow()
        {
            int x = display_.Left + (int)(display_.Width * anchor_dx_);
            int y = display_.Top + (int)(display_.Height * anchor_dy_);

            return new Win32.Point(x, y);
        }
    }
}
