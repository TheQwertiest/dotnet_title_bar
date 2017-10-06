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
using System.Xml.Linq;
using System.Xml.XPath;

namespace fooTitle.Layers
{
	/// <summary>
	/// Summary description for AnimationLayer.
	/// </summary>
    [LayerTypeAttribute("animation")]
    public class AnimationLayer : Layer, IContiniousRedraw
	{
		protected int curFrame = 0;
        /// <summary>
        /// FPS rate of animation.
        /// </summary>
        protected int refreshRate = 15;
        protected long lastUpdate = DateTime.Now.Ticks;

        public AnimationLayer(Rectangle parentRect, XElement node) : base(parentRect, node) {
            XElement contents = GetFirstChildByName(node, "contents");
            refreshRate = Math.Max(1, GetCastedAttributeValue<int>(contents, "speed", 15));

            // load all images
            XPathNavigator nav = node.CreateNavigator();
			XPathNodeIterator xi = (XPathNodeIterator)nav.Evaluate("contents/frame");

            while (xi.MoveNext()) {
				AddImage(xi.Current);
			}

		    Main.GetInstance().AddRedrawRequester(this);
        }

		protected void AddImage(XPathNavigator node) {
			string src = node.GetAttribute("src", "");
			Bitmap b = Main.GetInstance().CurrentSkin.GetSkinImage(src);
			images.Add(b);
		}

        protected override void DrawImpl() {
            long now = DateTime.Now.Ticks;
            int deltaTime = (int)((now - lastUpdate) / 10000);

            if (deltaTime >= 1000 / refreshRate)
            {
                ++curFrame;
                if (curFrame >= images.Count)
                    curFrame = 0;

                lastUpdate = now;
            }

            Display.Canvas.DrawImage((Bitmap)images[curFrame], ClientRect.X, ClientRect.Y, ClientRect.Width, ClientRect.Height);		
        }

        protected override void OnLayerEnable()
        {
            Main.GetInstance().AddRedrawRequester(this);
        }

        protected override void OnLayerDisable()
        {
            Main.GetInstance().RemoveRedrawRequester(this);
        }

	    public bool IsRedrawNeeded()
	    {
	        int deltaTime = (int)((DateTime.Now.Ticks - lastUpdate) / 10000);
	        return (deltaTime >= 1000 / refreshRate);
	    }
    }
}
