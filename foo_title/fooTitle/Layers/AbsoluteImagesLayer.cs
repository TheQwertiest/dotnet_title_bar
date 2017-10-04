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
using System.Xml.Linq;
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
        private readonly bool _hasGif = false;

		public AbsoluteImagesLayer(Rectangle parentRect, XElement node) : base(parentRect, node)
		{
			// load all images
			XPathNavigator nav = node.CreateNavigator();
			XPathNodeIterator xi = (XPathNodeIterator)nav.Evaluate("contents/image");
			
			while (xi.MoveNext()) {
				AddImage(xi.Current);
			}

            foreach (Bitmap b in images)
            {
                if (ImageAnimator.CanAnimate(b))
                {
                    _hasGif = true;
                    break;
                }
            }
            if (Enabled)
		        OnLayerEnable();
		}

		private void AddImage(XPathNavigator node) {
			string src = node.GetAttribute("src", "");
			Bitmap b = Main.GetInstance().CurrentSkin.GetSkinImage(src);
			images.Add(b);
		}

		protected override void DrawImpl() {
		    if (_hasGif)
		    {
		        ImageAnimator.UpdateFrames();
            }
		    foreach (Bitmap b in images){ 
				Display.Canvas.DrawImage(b, ClientRect.X, ClientRect.Y, ClientRect.Width, ClientRect.Height);
			}
		}

        protected override void OnLayerEnable()
        {
            if (_hasGif)
            {
                foreach (Bitmap b in images)
                {
                    if (ImageAnimator.CanAnimate(b))
                        ImageAnimator.Animate(b, OnFrameChanged);
                }
            }
        }

        protected override void OnLayerDisable()
        {
            if (_hasGif)
            {
                foreach (Bitmap b in images)
                {
                    if (ImageAnimator.CanAnimate(b))
                        ImageAnimator.StopAnimate(b, OnFrameChanged);
                }
            }
        }

        private void OnFrameChanged(object o, EventArgs e)
        {
            Main.GetInstance().RequestRedraw();
        }
    }
}
