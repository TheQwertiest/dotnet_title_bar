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
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using fooTitle;
using System.Xml;

namespace fooTitle.Layers {
    [LayerTypeAttribute("scrolling-text")]
    class ScrollingTextLayer : TextLayer {

        protected int direction = 1;
        /// <summary>
        /// Number of pixels to move the text per second.
        /// </summary>
        protected float speed = 25F;
        /// <summary>
        /// Length of pause in miliseconds when the text arrives to either of its edges. 
        /// </summary>
        protected int pause = 1000;
        protected float xpos = 0;
        protected Bitmap textImage;
        protected Graphics textCanvas;
        protected long lastUpdate = System.DateTime.Now.Ticks;
        protected bool paused = false;
        protected enum Align {
            Left,
            Center,
            Right
        }

        protected Align align;

        public ScrollingTextLayer(Rectangle parentRect, XmlNode node)
            : base(parentRect, node) {

            XmlNode contents = GetFirstChildByName(node, "contents");
            
            speed = float.Parse(GetAttributeValue(contents, "speed", "25"));
            pause = Int32.Parse(GetAttributeValue(contents, "pause", "1000"));
        }

        protected override void addLabel(XmlNode node, TextLayer.LabelPart def) {
            string position = GetAttributeValue(node, "position", "left");
            left = readLabelFromElement(node, def);

            if (position == "left") {
                align = Align.Left;
            } else if (position == "right") {
                align = Align.Right;
            } else if (position == "center") {
                align = Align.Center;
            }
        }

        protected override void straightDraw(Graphics g) {
 	
            // first move the text
            long now = System.DateTime.Now.Ticks;
            long deltaTime = now - lastUpdate;
            float textWidth = g.MeasureString(left.formatted, left.font).Width;
            float farPoint = this.getFarPointInClientRect(this.angle);
            float drawAt = 0;
            
            if (textWidth <= farPoint) {  // no need to move, it's too short
                xpos = 0;
                if (align == Align.Left) {
                    drawAt = 0;
                } else if (align == Align.Center) {
                    drawAt = (farPoint - textWidth) / 2;
                } else if (align == Align.Right) {
                    drawAt = farPoint - textWidth;
                }
            } else if (!paused) {
                xpos += direction * speed * ((float)deltaTime / 10000000F);
                if (xpos >= textWidth - farPoint) {
                    // reached the right end
                    direction = -1;
                    xpos = textWidth - farPoint;
                    paused = true;
                } else if (xpos <= 0) {
                    // reached the left end
                    direction = 1;
                    xpos = 0;
                    paused = true;
                }
            }

            // then draw it
            if (textImage != null) {
                g.DrawImage(textImage, (int)(drawAt), 0, new RectangleF((int)xpos, 0, (int)farPoint, textImage.Height), GraphicsUnit.Pixel);
            }

            if (!paused) {
                lastUpdate = now;
            } else {
                if ((deltaTime / 10000) >= pause) {
                    lastUpdate = now;
                    paused = false;
                }
            }
        }

        /// <summary>
        /// This layer doesn't have any fixed size content, like TextLayer, so just use the default
        /// GetMinimalSize implementation
        /// </summary>
        /// <returns>the same as Layer.GetMinimialSize()</returns>
        protected override System.Drawing.Size getMinimalSizeImpl() {
            return geometry.GetMinimalSize(Display, getContentSize());
        }

        /// <summary>
        /// This draws the entire text to a backbuffer. When drawing, the appropriate part of backbuffer is used.
        /// </summary>
        protected override void updateText() {
            base.updateText();
            if (left.formatted != null) {
                SizeF textSize = Main.GetInstance().Display.Canvas.MeasureString(left.formatted, left.font);
                textImage = new Bitmap(Math.Max(10, (int)Math.Ceiling(textSize.Width)), Math.Max(10, (int)Math.Ceiling(textSize.Height)), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                textCanvas = Graphics.FromImage(textImage);
                textCanvas.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                textCanvas.DrawString(left.formatted, left.font, new SolidBrush(left.color), new PointF(0, 0));
            }
        }
    }
}
