using System;
using System.Xml;
using System.Xml.XPath;
using System.Drawing;

namespace fooTitle.Layers
{
	/// <summary>
	/// A simple layer that contains several images that are stretched across it's entire clientRect and drawn over.
	/// </summary>
    [LayerTypeAttribute("absolute-images")]
	public class AbsoluteImagesLayer : Layer
	{
		public AbsoluteImagesLayer(Rectangle parentRect, XmlNode node) : base(parentRect, node)
		{
			// load all images
			XPathNavigator nav = node.CreateNavigator();
			XPathNodeIterator xi = (XPathNodeIterator)nav.Evaluate("contents/image");
			
			while (xi.MoveNext()) {
				addImage(xi.Current);
			}
		}

		protected void addImage(XPathNavigator node) {
			string src = node.GetAttribute("src", "");
			Bitmap b = new Bitmap(Main.GetInstance().CurrentSkin.GetSkinFilePath(src));
			images.Add(b);
		}

		public override void Draw() {
			foreach (Bitmap b in images){ 
				Display.Canvas.DrawImage(b, ClientRect.X, ClientRect.Y, ClientRect.Width, ClientRect.Height);
			}

			base.Draw();
		}

	}
}
