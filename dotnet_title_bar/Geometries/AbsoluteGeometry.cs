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
using System.Xml.Linq;

namespace fooTitle.Geometries
{

    [GeometryType("absolute")]
    class AbsoluteGeometry : Geometry
    {

        public enum AlignType
        {
            Left,
            Right
        }
        ;

        private readonly Rectangle _myParentRect;

        /// <summary>
        /// The expression to calculate width from. May be null if no expression was given.
        /// </summary>
        private readonly string _myExprWidth;
        private int _myWidth;
        public int Width
        {
            get => _myWidth;
            set
            {
                _myWidth = value;
                Update(_myParentRect);
            }
        }

        /// <summary>
        /// The expression to calculate height from. May be null if no expression was given.
        /// </summary>
        private readonly string _myExprHeight;
        private int _myHeight;
        public int Height
        {
            get => _myHeight;
            set
            {
                _myHeight = value;
                Update(_myParentRect);
            }
        }
        /// <summary>
        /// The expressions to calculate X and Y coordinates of the position from. Either of them may be
        /// null if no expression was given.
        /// </summary>
        private ExpressionPoint _myExprPosition;
        private Point _myPosition;
        public Point Position
        {
            get => _myPosition;
            set
            {
                _myPosition = value;
                Update(_myParentRect);
            }
        }

        private AlignType _myAlign;
        public AlignType Align
        {
            get => _myAlign;
            set
            {
                _myAlign = value;
                Update(_myParentRect);
            }
        }

        public AbsoluteGeometry(Rectangle parentRect, XElement node)
            : base(parentRect, node)
        {
            _myParentRect = parentRect;

            // read description from the xml
            XElement size = GetFirstChildByName(node, "size");

            // read and store expressions
            _myExprWidth = GetExpressionFromAttribute(size, "x");
            _myExprHeight = GetExpressionFromAttribute(size, "y");

            // get the actual values
            _myWidth = DpiHandler.ScaleValueByDpi((int)GetNumberFromAttribute(size, "x", 100));
            _myHeight = DpiHandler.ScaleValueByDpi((int)GetNumberFromAttribute(size, "y", 30));

            // read position
            XElement position = GetFirstChildByName(node, "position");

            // read and store expressions (if any)
            _myExprPosition.X = GetExpressionFromAttribute(position, "x");
            _myExprPosition.Y = GetExpressionFromAttribute(position, "y");

            // TODO: myPosition (i.e. relative to parent) calculation should take align into account
            // get the actual values
            _myPosition.X = DpiHandler.ScaleValueByDpi((int)GetNumberFromAttribute(position, "x", 0));
            _myPosition.Y = DpiHandler.ScaleValueByDpi((int)GetNumberFromAttribute(position, "y", 0));

            // read align
            if (GetAttributeValue(position, "align", "left") == "right")
                _myAlign = AlignType.Right;
            else
                _myAlign = AlignType.Left;
        }

        public AbsoluteGeometry(Rectangle parentRect, int width, int height, Point position)
        {
            _myParentRect = parentRect;
            _myWidth = width;
            _myHeight = height;
            _myPosition = position;
            _myAlign = AlignType.Left;
        }

        public override void Update(Rectangle parentRect)
        {
            // calculate parameters
            _myPosition.X = GetScaledValueFromExpression(_myExprPosition.X, _myPosition.X);
            _myPosition.Y = GetScaledValueFromExpression(_myExprPosition.Y, _myPosition.Y);
            _myWidth = GetScaledValueFromExpression(_myExprWidth, _myWidth);
            _myHeight = GetScaledValueFromExpression(_myExprHeight, _myHeight);

            // now calculate client rect from the parameters
            myClientRect.Width = _myWidth;
            myClientRect.Height = _myHeight;

            switch (_myAlign)
            {
                case AlignType.Left:
                    myClientRect.X = _myPosition.X + parentRect.Left;
                    myClientRect.Y = _myPosition.Y + parentRect.Top;
                    break;
                case AlignType.Right:
                    myClientRect.X = _myPosition.X + parentRect.Right - myClientRect.Width;
                    myClientRect.Y = _myPosition.Y + parentRect.Top;
                    break;
            }
        }

        public override Size GetMinimalSize(Size contentSize)
        {
            return new Size(Math.Max(Width, contentSize.Width), Math.Max(Height, contentSize.Height));
        }

        public override Point GetPosition()
        {
            return Position;
        }

        public override bool IsDynamic()
        {
            return (_myExprHeight != null || _myExprPosition.X != null || _myExprPosition.Y != null || _myExprWidth != null);
        }
    }
}
