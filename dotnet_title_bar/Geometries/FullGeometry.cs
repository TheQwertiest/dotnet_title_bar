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

using System.Drawing;
using System.Xml.Linq;

namespace fooTitle.Geometries
{
    [GeometryType("full")]
    public class FullGeometry : Geometry
    {

        /// <summary>
        /// Current values of padding
        /// </summary>
        private Padding _myPadding;

        /// <summary>
        /// Stored expressions to calculate padding from
        /// </summary>
        private ExpressionPadding _myExprPadding;

        public FullGeometry(Rectangle parentRect, XElement node)
            : base(parentRect, node)
        {
            XElement padding = GetFirstChildByName(node, "padding");

            // read and store expressions
            _myExprPadding.Left = GetExpressionFromAttribute(padding, "left");
            _myExprPadding.Top = GetExpressionFromAttribute(padding, "top");
            _myExprPadding.Right = GetExpressionFromAttribute(padding, "right");
            _myExprPadding.Bottom = GetExpressionFromAttribute(padding, "bottom");

            // get the actual values
            _myPadding.Left = DpiHandler.ScaleValueByDpi((int)GetNumberFromAttribute(padding, "left", 0));
            _myPadding.Top = DpiHandler.ScaleValueByDpi((int)GetNumberFromAttribute(padding, "top", 0));
            _myPadding.Right = DpiHandler.ScaleValueByDpi((int)GetNumberFromAttribute(padding, "right", 0));
            _myPadding.Bottom = DpiHandler.ScaleValueByDpi((int)GetNumberFromAttribute(padding, "bottom", 0));
        }

        public override void Update(Rectangle parentRect)
        {
            // get the actual values
            _myPadding.Left = GetScaledValueFromExpression(_myExprPadding.Left, _myPadding.Left);
            _myPadding.Top = GetScaledValueFromExpression(_myExprPadding.Top, _myPadding.Top);
            _myPadding.Right = GetScaledValueFromExpression(_myExprPadding.Right, _myPadding.Right);
            _myPadding.Bottom = GetScaledValueFromExpression(_myExprPadding.Bottom, _myPadding.Bottom);

            myClientRect.X = _myPadding.Left + parentRect.Left;
            myClientRect.Y = _myPadding.Top + parentRect.Top;

            myClientRect.Width = parentRect.Width - (_myPadding.Left + _myPadding.Right);
            myClientRect.Height = parentRect.Height - (_myPadding.Top + _myPadding.Bottom);
        }

        public override Size GetMinimalSize(Size contentSize)
        {
            return new Size(_myPadding.Left + _myPadding.Right + contentSize.Width,
                            _myPadding.Top + _myPadding.Bottom + contentSize.Height);
        }

        public override Point GetPosition()
        {
            return new Point(0, 0);
        }

        public override bool IsDynamic()
        {
            return (_myExprPadding.Bottom != null || _myExprPadding.Left != null || _myExprPadding.Right != null || _myExprPadding.Top != null);
        }
    }
}
