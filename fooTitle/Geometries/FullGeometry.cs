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
using System.Drawing;
using System.Xml;

namespace fooTitle.Geometries {
    [GeometryTypeAttribute("full")]
    class FullGeometry : Geometry{

        protected Rectangle myPadding;

        public FullGeometry(Rectangle parentRect, XmlNode node) : base(parentRect, node) {
            XmlNode padding = GetFirstChildByName(node, "padding");
            myPadding.X = Int32.Parse(padding.Attributes.GetNamedItem("left").Value);
            myPadding.Y = Int32.Parse(padding.Attributes.GetNamedItem("top").Value);
            myPadding.Width = Int32.Parse(padding.Attributes.GetNamedItem("right").Value) - myPadding.Left;
            myPadding.Height = Int32.Parse(padding.Attributes.GetNamedItem("bottom").Value) - myPadding.Top;
            
        }

        public override void Update(Rectangle parentRect) {
            myClientRect.X = myPadding.Left + parentRect.Left;
            myClientRect.Y = myPadding.Top + parentRect.Top;

            myClientRect.Width = parentRect.Width - (myPadding.Left + myPadding.Right);
            myClientRect.Height = parentRect.Height - (myPadding.Top + myPadding.Bottom);
            
        }

        public override Size GetMinimalSize(Display display, Size contentSize) {
            return new Size(myPadding.Left + myPadding.Right + contentSize.Width,
                            myPadding.Top + myPadding.Bottom + contentSize.Height);
        }

        public override System.Drawing.Point GetPosition() {
            return new Point(0, 0);
        }
    }
}
