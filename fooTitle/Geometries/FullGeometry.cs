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
