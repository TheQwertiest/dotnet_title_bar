using System;
using System.Xml;
using System.Xml.XPath;
using System.Drawing;
using System.Drawing.Imaging;

namespace fooTitle.Layers
{
    [LayerTypeAttribute("fill-images")]
	public class FillImagesLayer : Layer
	{
		protected Bitmap leftImage = null;
		protected Bitmap centerImage = null;
		protected Bitmap rightImage = null;
        protected Bitmap centerRepeated;
        protected Graphics centerRepeatedCanvas;
		protected bool repeatCenter = false;

		public FillImagesLayer(Rectangle parentRect, XmlNode node) : base(parentRect, node) {
			// load all images
			XPathNavigator nav = node.CreateNavigator();
			XPathNodeIterator xi = (XPathNodeIterator)nav.Evaluate("contents/image");
			
			while (xi.MoveNext()) {
				addImage(xi.Current);
			}
		}

		protected void addImage(XPathNavigator node) {
			string src = node.GetAttribute("src", "");
			string position = node.GetAttribute("position", "");
			Bitmap b = new Bitmap(Main.GetInstance().CurrentSkin.GetSkinFilePath(src));
			if (position == "left") {
				leftImage = b;
			} else if (position == "center") {
				centerImage = b;
                if (node.GetAttribute("repeat", "") == "true") 
                    repeatCenter = true;
			} else if (position == "right") {
				rightImage = b;
			}
		}

        private void prepareRepeatImage() {
            // can't create 0x0 bitmaps
            centerRepeated = new Bitmap(Math.Max(10, getRightImageStart() - getLeftImageWidth()), Math.Max(10, ClientRect.Height));
            centerRepeatedCanvas = Graphics.FromImage(centerRepeated);

            if (centerImage != null) {
                if (repeatCenter) {
                    ImageAttributes attrs = new ImageAttributes();
                    float count = (getRightImageStart() - getLeftImageWidth()) / (float)centerImage.Width;

                    for (int i = 0; i < (count + 1); i++) {
                        centerRepeatedCanvas.DrawImage(centerImage, new Rectangle(
                                (int)Math.Round((float)(i * centerImage.Width)),
                                0,
                                (int)Math.Round((float)centerImage.Width),
                                ClientRect.Height),
                            0, 0, centerImage.Width, centerImage.Height,
                            GraphicsUnit.Pixel,
                            attrs);
                    }
                } else {
                    // no repeat
                    centerRepeatedCanvas.DrawImage(centerImage, 0, 0, centerRepeated.Width, centerRepeated.Height);
                }
            }
        }

        public override void UpdateGeometry(Rectangle parentRect) {
            base.UpdateGeometry(parentRect);

            prepareRepeatImage();
        }

		public override void Draw() {
			int left = getLeftImageWidth();  // where the center image starts (rel. to layer)
			int right = getRightImageStart(); // where the center image ends (rel. to layer)
			if (leftImage != null) {
				Display.Canvas.DrawImage(leftImage, ClientRect.X, ClientRect.Y, leftImage.Width, ClientRect.Height);
			}

            Display.Canvas.DrawImage(centerRepeated, ClientRect.X + left, ClientRect.Y, centerRepeated.Width, ClientRect.Height);

			if (rightImage != null) {
				Display.Canvas.DrawImage(rightImage, ClientRect.X + right, ClientRect.Y, rightImage.Width, ClientRect.Height);
			}

			base.Draw();

		}

        private int getRightImageStart() {
            if (rightImage != null) {
                return ClientRect.Width - rightImage.Width;
            }
            return ClientRect.Width;
        }

        private int getLeftImageWidth() {
            if (leftImage != null) {
                return leftImage.Width;
            }
            return 0;
        }
	}
}
