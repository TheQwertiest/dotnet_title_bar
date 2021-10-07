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

using System;
using System.Drawing;

namespace fooTitle
{
    internal class DockAnchor
    {
        private readonly Display _display;
        private double _anchorDx = 0;
        private double _anchorDy = 0;
        private int _anchorX = 0;
        private int _anchorY = 0;
        private DockAnchor.Type _anchorType = DockAnchor.Type.Left | DockAnchor.Type.Top;

        internal DockAnchor(Display display)
        {
            _display = display;
        }

        [Flags]
        public enum Type
        {
            None = 0,
            Left = 1,
            Right = 1 << 1,
            Top = 1 << 2,
            Bottom = 1 << 3,
            Center = 1 << 4,
        }

        internal void Initialize(DockAnchor.Type type, double dx, double dy)
        {
            _anchorType = type;

            _anchorDx = 0.5 /*dx*/;
            _anchorDy = 0.5 /*dy*/;

            if ((_anchorType & DockAnchor.Type.Left) != 0)
            {
                _anchorDx = 0;
            }
            else if ((_anchorType & DockAnchor.Type.Right) != 0)
            {
                _anchorDx = 1;
            }
            else
            {
                _anchorDx = 0.5;
            }

            if ((_anchorType & DockAnchor.Type.Top) != 0)
            {
                _anchorDy = 0;
            }
            else if ((_anchorType & DockAnchor.Type.Bottom) != 0)
            {
                _anchorDy = 1;
            }
            else
            {
                _anchorDy = 0.5;
            }

            Win32.Point anchorPos = CalculatePositionFromWindow();
            _anchorX = anchorPos.x;
            _anchorY = anchorPos.y;
        }
        internal Win32.Point GetPosition()
        {
            return CalculatePositionFromWindow();
        }

        internal void SetPosition(int anchorX, int anchorY)
        {
            _anchorX = anchorX;
            _anchorY = anchorY;
        }

        internal Win32.Point GetWindowPosition()
        {
            int x = _anchorX - (int)(_display.Width * _anchorDx);
            int y = _anchorY - (int)(_display.Height * _anchorDy);

            return new Win32.Point(x, y);
        }

        internal void Draw()
        {
            int anchorRelativeX = Math.Min((int)(_anchorDx * _display.Width), _display.Width - 1);
            int anchorRelativeY = Math.Min((int)(_anchorDy * _display.Height), _display.Height - 1);

            Color anchorCol = Color.FromArgb(0xFF, 0xFF, 0xFF, 0x70);

            var prevClip = _display.Canvas.ClipBounds;

            _display.Canvas.ResetClip();
            _display.Canvas.FillRectangle(new SolidBrush(anchorCol), anchorRelativeX - 1, anchorRelativeY - 1, 3, 3);
            using (var pen = new Pen(anchorCol))
            {
                _display.Canvas.DrawLine(pen, anchorRelativeX - 25, anchorRelativeY, anchorRelativeX + 25, anchorRelativeY);
                _display.Canvas.DrawLine(pen, anchorRelativeX, anchorRelativeY - 25, anchorRelativeX, anchorRelativeY + 25);
            }
            _display.Canvas.SetClip(prevClip);
        }

        private Win32.Point CalculatePositionFromWindow()
        {
            int x = _display.Left + (int)(_display.Width * _anchorDx);
            int y = _display.Top + (int)(_display.Height * _anchorDy);

            return new Win32.Point(x, y);
        }
    }
}
