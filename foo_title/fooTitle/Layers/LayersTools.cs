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
using System.Reflection;
using System.Collections;
using fooTitle;
using System.Xml;
using fooTitle.Extending;
using System.Collections.Generic;

namespace fooTitle.Layers {
    /// <summary>
    /// This attribute should be used on classes that represent layers accessible from the
    /// skin's XML file. Type coresponds to the type="..." attribute in the skin's <layer> tag.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class LayerTypeAttribute : ElementTypeAttribute {
        public LayerTypeAttribute(String type):base(type) {
        }
    }

    
    /// <summary>
    /// Keeps track of layer available in this and extension assemblies
    /// </summary>
    public class LayerFactory : ElementFactory {
        
        public LayerFactory() {
            elementType = typeof(Layer);
            elementTypeAttributeType = typeof(LayerTypeAttribute);
        }

        public Layer CreateLayer(String type, System.Drawing.Rectangle parentRect, XmlNode node) {
            return(Layer)CreateElement(type, new object[] { parentRect, node });
        }

    }

    public static class LayerTools {
        public static void DepthFirstVisit(Layer initial, Predicate<Layer> visitor) {
            Stack<Layer> stack = new Stack<Layer>();
            stack.Push(initial);

            while (stack.Count > 0) {
                Layer current = stack.Pop();
                if (current == null)
                    return;

                if (!visitor(current))
                    return;

                foreach (Layer sub in current.SubLayers) {
                    stack.Push(sub);
                }
            }
        }

        // I think it's better to put these utility functions here than into Layer itself,
        // because they can work using just public interface and don't clutter the class.

        /// <summary>
        /// Enables or disables a layer and all its children.
        /// </summary>
        public static void EnableLayer(Layer layer, bool enable) {
            DepthFirstVisit(layer, delegate(Layer l) {
                l.Enabled = enable;
                return true;
            });
        }

        public static Layer FindLayerByName(Layer initial, string name) {
            Layer result = null;

            DepthFirstVisit(initial, delegate(Layer l) {
                if (l.Name == name) {
                    result = l;
                    return false;
                } else {
                    return true;
                }
            });

            return result;
        }
    }
}
