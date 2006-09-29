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
