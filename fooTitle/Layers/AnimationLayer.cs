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
