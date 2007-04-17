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
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;
using System.Drawing;
using System.Xml;

namespace fooTitle.Geometries {

    public enum AlignType {
        Left,
        Right
    };

    [GeometryTypeAttribute("absolute")]
    class AbsoluteGeometry : Geometry {

        protected Rectangle myParentRect;
        
        /// <summary>
        /// The expression to calculate width from. May be null if no expression was given.
        /// </summary>
        protected string myExprWidth;
        protected int myWidth;
        public int Width {
            get {
                return myWidth;
            }
            set {
                myWidth = value;
                Update(myParentRect);
            }
        }

        /// <summary>
        /// The expression to calculate height from. May be null if no expression was given.
        /// </summary>
        protected string myExprHeight;
        protected int myHeight;
        public int Height {
            get {
                return myHeight;
            }
            set {
                myHeight = value;
                Update(myParentRect);
            }
        }
        /// <summary>
        /// The expressions to calculate X and Y coordinates of the position from. Either of them may be
        /// null if no expression was given.
        /// </summary>
        protected ExpressionPoint myExprPosition;
        protected Point myPosition;
        public Point Position {
            get {
                return myPosition;
            }
            set {
                myPosition = value;
                Update(myParentRect);
            }
        }

        protected AlignType myAlign;
        public AlignType Align {
            get {
                return myAlign;
            }
            set {
                myAlign = value;
                Update(myParentRect);
            }
        }

        public AbsoluteGeometry(Rectangle parentRect, XmlNode node) : base(parentRect, node) {
            myParentRect = parentRect;

            // read description from the xml
            XmlNode size = GetFirstChildByName(node, "size");

            // read and store expressions
            myExprWidth = GetExpressionFromAttribute(size, "x", "100");
            myExprHeight = GetExpressionFromAttribute(size, "y", "30");

            // get the actual values
            myWidth = (int)GetNumberFromAttribute(size, "x", "100");
            myHeight = (int)GetNumberFromAttribute(size, "y", "30");

            // read position
            XmlNode position = GetFirstChildByName(node, "position");

            // read and store expressions (if any)
            myExprPosition.X = GetExpressionFromAttribute(position, "x", "0");
            myExprPosition.Y = GetExpressionFromAttribute(position, "y", "0");

            // get the actual values
            myPosition.X = (int)GetNumberFromAttribute(position, "x", "0");
            myPosition.Y = (int)GetNumberFromAttribute(position, "y", "0");

            // read align
            if (position.Attributes.GetNamedItem("align").Value == "right")
                myAlign = AlignType.Right;
            else
                myAlign = AlignType.Left;
        }

        public AbsoluteGeometry(Rectangle parentRect, int width, int height, Point position, AlignType align) {
            myParentRect = parentRect;
            myWidth = width;
            myHeight = height;
            myPosition = position;
            myAlign = align;
        }

        public override void Update(Rectangle parentRect) {
            // calculate parameters 
            myPosition.X = GetValueFromExpression(myExprPosition.X, myPosition.X);
            myPosition.Y = GetValueFromExpression(myExprPosition.Y, myPosition.Y);
            myWidth = GetValueFromExpression(myExprWidth, myWidth);
            myHeight = GetValueFromExpression(myExprHeight, myHeight);

            // now calculate client rect from the parameters
            myClientRect.Width = myWidth;
            myClientRect.Height = myHeight;

            if (myAlign == AlignType.Left) {
                myClientRect.X = myPosition.X + parentRect.Left;
                myClientRect.Y = myPosition.Y + parentRect.Top;
            } else if (myAlign == AlignType.Right) {
                myClientRect.X = myPosition.X + parentRect.Right - myClientRect.Width;
                myClientRect.Y = myPosition.Y + parentRect.Top;
            }
        }
        
        public override Size GetMinimalSize(Display display, Size contentSize) {
            return new Size(Math.Max(Width, contentSize.Width), Math.Max(Height, contentSize.Height));
        }

        public override System.Drawing.Point GetPosition() {
            return Position;
        }

        public override bool IsDynamic() {
            return (myExprHeight != null || myExprPosition.X != null || myExprPosition.Y != null || myExprWidth != null);
        }
    }
}
