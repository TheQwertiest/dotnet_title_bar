using System;
using System.Reflection;
using System.Collections;
using fooTitle.Extending;

namespace fooTitle.Geometries {

    [AttributeUsage(AttributeTargets.Class)]
    public class GeometryTypeAttribute : ElementTypeAttribute {

        public GeometryTypeAttribute(String _type) : base(_type) {
        }

    }

    public class GeometryFactory : ElementFactory {
        public GeometryFactory() {
            elementType = typeof(Geometry);
            elementTypeAttributeType = typeof(ElementTypeAttribute);
        }

        public Geometry CreateGeometry(string type, System.Drawing.Rectangle parentRect, System.Xml.XmlNode child) {
            return (Geometry)CreateElement(type, new object[] { parentRect, child });
        }
    }

}
