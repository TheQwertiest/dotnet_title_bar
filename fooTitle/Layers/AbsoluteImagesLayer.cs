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
			Bitmap b = Main.GetInstance().CurrentSkin.GetSkinImage(src);
			images.Add(b);
		}

		protected override void drawImpl() {
			foreach (Bitmap b in images){ 
				Display.Canvas.DrawImage(b, ClientRect.X, ClientRect.Y, ClientRect.Width, ClientRect.Height);
			}

		}

	}
}
