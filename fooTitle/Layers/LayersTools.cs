using System;
using System.Reflection;
using System.Collections;
using fooTitle;
using System.Xml;
using fooTitle.Extending;

namespace fooTitle.Layers {

    /// This attribute should be used on classes that represent layers accessible from the
    /// skin's XML file. Type coresponds to the type="..." attribute in the skin's <layer> tag.
    [AttributeUsage(AttributeTargets.Class)]
    public class LayerTypeAttribute : ElementTypeAttribute {
        public LayerTypeAttribute(String type):base(type) {
        }
    }

    /// Keeps track of layer available in this and extension assemblies
    public class LayerFactory : ElementFactory {
        
        public LayerFactory() {
            elementType = typeof(Layer);
            elementTypeAttributeType = typeof(LayerTypeAttribute);
        }

        public Layer CreateLayer(String type, System.Drawing.Rectangle parentRect, XmlNode node) {
            return(Layer)CreateElement(type, new object[] { parentRect, node });
        }

    }
}
