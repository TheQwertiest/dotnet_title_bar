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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace fooTitle.Layers
{
    [LayerTypeAttribute("fill-images")]
	public class FillImagesLayer : Layer
	{
		private Bitmap _leftImage = null;
		private Bitmap _centerImage = null;
		private Bitmap _rightImage = null;
        private Bitmap _centerRepeated;
        private Graphics _centerRepeatedCanvas;
		private bool _repeatCenter = false;

		public FillImagesLayer(Rectangle parentRect, XElement node) : base(parentRect, node) {
			// load all images
			XPathNavigator nav = node.CreateNavigator();
			XPathNodeIterator xi = (XPathNodeIterator)nav.Evaluate("contents/image");
			
			while (xi.MoveNext()) {
				AddImage(xi.Current);
			}
		}

		protected void AddImage(XPathNavigator node) {
			string src = node.GetAttribute("src", "");
			string position = node.GetAttribute("position", "");
            // Image need to be DPI scaled, since this layer uses real image sizes
			Bitmap b = ScaleImage(Main.GetInstance().CurrentSkin.GetSkinImage(src));

            if (position == "left") {
				_leftImage = b;
			} else if (position == "center") {
				_centerImage = b;
                if (node.GetAttribute("repeat", "") == "true") 
                    _repeatCenter = true;
			} else if (position == "right") {
				_rightImage = b;
			}
		}

        private void PrepareRepeatImage() {
            // can't create 0x0 bitmaps
            _centerRepeated = new Bitmap(Math.Max(10, GetRightImageStart() - GetLeftImageWidth()), Math.Max(10, ClientRect.Height));
            _centerRepeatedCanvas = Graphics.FromImage(_centerRepeated);

            if (_centerImage == null)
                return;

            if (_repeatCenter) {
                ImageAttributes attrs = new ImageAttributes();
                float count = (GetRightImageStart() - GetLeftImageWidth()) / (float)_centerImage.Width;

                for (int i = 0; i < (count + 1); i++) {
                    _centerRepeatedCanvas.DrawImage(_centerImage, new Rectangle(
                            (int)Math.Round((float)(i * _centerImage.Width)),
                            0,
                            (int)Math.Round((float)_centerImage.Width),
                            ClientRect.Height),
                        0, 0, _centerImage.Width, _centerImage.Height,
                        GraphicsUnit.Pixel,
                        attrs);
                }
            } else {
                // no repeat
                _centerRepeatedCanvas.DrawImage(_centerImage, 0, 0, _centerRepeated.Width, _centerRepeated.Height);
            }
        }

        public override void UpdateGeometry(Rectangle parentRect) {
            base.UpdateGeometry(parentRect);

            PrepareRepeatImage();
        }

		protected override void DrawImpl() {
			int left = GetLeftImageWidth();  // where the center image starts (rel. to layer)
			int right = GetRightImageStart(); // where the center image ends (rel. to layer)
			if (_leftImage != null) {
				Display.Canvas.DrawImage(_leftImage, ClientRect.X, ClientRect.Y, _leftImage.Width, ClientRect.Height);
			}

            Display.Canvas.DrawImage(_centerRepeated, ClientRect.X + left, ClientRect.Y, _centerRepeated.Width, ClientRect.Height);

			if (_rightImage != null) {
				Display.Canvas.DrawImage(_rightImage, ClientRect.X + right, ClientRect.Y, _rightImage.Width, ClientRect.Height);
			}
		}

        private int GetRightImageStart() {
            if (_rightImage != null) {
                return ClientRect.Width - _rightImage.Width;
            }
            return ClientRect.Width;
        }

        private int GetLeftImageWidth()
        {
            return _leftImage?.Width ?? 0;
        }

        private static Bitmap ScaleImage(Image image)
        {
            Rectangle destRect = new Rectangle(0, 0, Main.GetInstance().ScaleValue(image.Width), Main.GetInstance().ScaleValue(image.Height));
            Bitmap destImage = new Bitmap(Main.GetInstance().ScaleValue(image.Width), Main.GetInstance().ScaleValue(image.Height));

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            Graphics graphics = Graphics.FromImage(destImage);
            ImageAttributes wrapMode = new ImageAttributes();
            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
            graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);

            return destImage;
        }
    }
}
