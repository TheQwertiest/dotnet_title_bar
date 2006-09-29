using System;
using System.Xml;
using System.Xml.XPath;
using System.Collections;
using System.Drawing;
using fooTitle.Geometries;


namespace fooTitle.Layers
{
	/// <summary>
	/// Layers exist in a hiaerchical structure
	/// </summary>
    [LayerTypeAttribute("empty")]
	public class Layer : fooTitle.Extending.Element
	{
		private string myName;
		public string Name {
			get {
				return myName;
			}
		}

		private string myType;
		public string Type {
			get {
				return myType;
			}
		}

        public Point Position {
            get {
                return geometry.GetPosition();
            }
        }

		public virtual System.Drawing.Rectangle ClientRect {
			get {
                return geometry.ClientRect;
			}
		}
        protected Display display;
        protected Display Display {
            get {
                return display;
            }
            set {
                display = value;
            }
        }

        protected Geometry geometry;
		
		protected ArrayList images = new ArrayList();
		protected ArrayList layers = new ArrayList();

		public Layer(Rectangle parentRect, XmlNode node) {
			XPathNavigator nav = node.CreateNavigator();

			// read name and type
			myName = node.Attributes.GetNamedItem("name").Value;
			myType = node.Attributes.GetNamedItem("type").Value;

            // create the geometry
            foreach (XmlNode child in node.ChildNodes) {
                if (child.Name == "geometry") {
                    string geomType = child.Attributes.GetNamedItem("type").Value;
                    geometry = Main.GetInstance().GeometryFactory.CreateGeometry(geomType, parentRect, child);
                }
            }


			UpdateGeometry(parentRect);
		}

		protected Layer() {

		}

		protected virtual void loadLayers(XmlNode node) {
			foreach (XmlNode i in node.ChildNodes) {
				if (i.Name == "layer") 
					addLayer(i);
			}
		}

		private void addLayer(XmlNode node) {
			Layer layer = null;
			string type = node.Attributes.GetNamedItem("type").InnerText;

            layer = Main.GetInstance().LayerFactory.CreateLayer(type, this.ClientRect, node);

            if (layer != null) {
                layer.Display = this.Display;
                layer.loadLayers(node);
                layers.Add(layer);
            }
		}

		public virtual void UpdateGeometry(Rectangle parentRect) {
            geometry.Update(parentRect);

			// now update all sub-layers
			foreach (Layer i in layers) {
				i.UpdateGeometry(ClientRect);
			}
		}

        protected void drawSubLayers() {
            foreach (Layer i in layers) {
                i.Draw();
            }

        }

		public virtual void Draw() {
            drawSubLayers();
		}

        /// <summary>
        /// This function calculates the minimal width for itself and sublayers to fit into.
        /// </summary>
        /// <returns>Returns the minimal width required to fit the sublayers and self's content</returns>
		public virtual Size GetMinimalSize() {
            return geometry.GetMinimalSize(Display, defaultGetMinimalSize());
        }

        /// <summary>
        /// This calculates the size of this's content. Does not take geometry into account.
        /// </summary>
        protected Size defaultGetMinimalSize() {
            Size minSize = new Size(0, 0);
            foreach (Layer i in layers) {
                Size layerSize = i.GetMinimalSize();
                if (i.Position.X + layerSize.Width > minSize.Width)
                    minSize.Width = i.Position.X + layerSize.Width;
                if (i.Position.Y + layerSize.Height > minSize.Height)
                    minSize.Height = i.Position.Y + layerSize.Height;
            }
            // return geometry.GetMinimalSize(Display, minSize);
            return minSize;
        }
	}
}
