using System;
using System.Drawing;
using System.Xml;
using System.Xml.XPath;

namespace fooTitle.Layers
{
	/// <summary>
	/// Summary description for AnimationLayer.
	/// </summary>
    [LayerTypeAttribute("animation")]
    public class AnimationLayer : Layer
	{
		protected int curFrame = 0;

		public AnimationLayer(Rectangle parentRect, XmlNode node) : base(parentRect, node) {
			// load all images
			XPathNavigator nav = node.CreateNavigator();
			XPathNodeIterator xi = (XPathNodeIterator)nav.Evaluate("contents/frame");
			
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
			Display.Canvas.DrawImage((Bitmap)images[curFrame], ClientRect.X, ClientRect.Y, ClientRect.Width, ClientRect.Height);

			curFrame ++;
			if (curFrame >= images.Count) 
				curFrame = 0;
			base.Draw();
		}
	}
}
