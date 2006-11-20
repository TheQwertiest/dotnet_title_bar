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
using fooTitle.Extending;
using System.Xml;

namespace fooTitle.Geometries {
    public abstract class Geometry : Element {

        protected Rectangle myClientRect = new Rectangle(0, 0, 0, 0);
        /// <summary>
        /// The rectangle that this geometry defines.
        /// </summary>
        public Rectangle ClientRect {
            get {
                return myClientRect;
            }
        }
        /// <summary>
        /// This structure describes a rectangle using foobar2000's formatting expressions.
        /// </summary>
        public struct ExpressionPadding {
            public string Left;
            public string Top;
            public string Right;
            public string Bottom;
        }

        /// <summary>
        /// This structure describes a point using foobar2000's formatting expressions.
        /// </summary>
        public struct ExpressionPoint {
            public string X;
            public string Y;
        }

        /// <summary>
        /// Stores padding information. Each member stores the size of padding at each side of a rectangle
        /// </summary>
        public struct Padding {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        /// <summary>
        /// Creates a new geometry object.
        /// </summary>
        /// <param name="parentRect">The rectangle that this geometry lives in</param>
        /// <param name="node">The XML node to load geometry's parameters from</param>
        public Geometry(Rectangle parentRect, XmlNode node) {

        }


        protected Geometry() { }

        /// <summary>
        /// Call this method to recalculates it's ClientRect when parentRect changes
        /// </summary>
        /// <param name="parentRect">the new parent rectangle</param>
        public abstract void Update(Rectangle parentRect);

        /// <summary>
        /// Calculates the minimal size needed to display this entire layer.
        /// </summary>
        /// <param name="display">The display this geometry will be shown on 
        /// (different displays may have different metrics)</param>
        /// <param name="size">The size of the contents.</param>
        public abstract Size GetMinimalSize(Display display, Size size);


        /// <summary>
        /// Returns the position of this layer, relative to it's parent rectangle.
        /// </summary>
        public abstract Point GetPosition();
    }

}
