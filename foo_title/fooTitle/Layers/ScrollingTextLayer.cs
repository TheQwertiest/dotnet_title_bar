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
using System.Windows.Forms;
using System.Xml.Linq;

namespace fooTitle.Layers {
    [LayerTypeAttribute("scrolling-text")]
    internal class ScrollingTextLayer : TextLayer, IContiniousRedraw
    {
        private int _direction = 1;
        /// <summary>
        /// Number of pixels to move the text per second.
        /// </summary>
        private readonly float _speed = 25F;
        /// <summary>
        /// Length of pause in milliseconds when the text arrives to either of its edges. 
        /// </summary>
        private readonly int _pause = 1000;
        private float _xpos = 0;
        private Bitmap _textImage;
        private Graphics _textCanvas;
        private long _lastUpdate = DateTime.Now.Ticks;
        private bool _paused = false;
        private enum Align
        {
            Left,
            Center,
            Right
        }

        private Align _align;

        public ScrollingTextLayer(Rectangle parentRect, XElement node)
            : base(parentRect, node)
        {

            XNode contents = GetFirstChildByName(node, "contents");

            _speed = GetCastedAttributeValue<float>(contents, "speed", "25");
            _pause = GetCastedAttributeValue<int>(contents, "pause", "1000");
        }

        protected override void AddLabel(XElement node, TextLayer.LabelPart def)
        {
            string position = GetAttributeValue(node, "position", "left");
            left = ReadLabel(node, def);

            switch (position)
            {
                case "left":
                    _align = Align.Left;
                    break;
                case "right":
                    _align = Align.Right;
                    break;
                case "center":
                    _align = Align.Center;
                    break;
            }
        }

        protected override void StraightDraw(Graphics g)
        {

            // first move the text
            long now = DateTime.Now.Ticks;
            long deltaTime = now - _lastUpdate;
            float textWidth = g.MeasureString(left.formatted, left.font).Width;
            float farPoint = this.GetFarPointInClientRect(this.angle);
            float drawAt = 0;

            if (textWidth <= farPoint)
            {
                // no need to move, it's too short
                _xpos = 0;
                switch (_align)
                {
                    case Align.Left:
                        drawAt = 0;
                        break;
                    case Align.Center:
                        drawAt = (farPoint - textWidth) / 2;
                        break;
                    case Align.Right:
                        drawAt = farPoint - textWidth;
                        break;
                }
            }
            else if (!_paused)
            {
                _xpos += _direction * _speed * (deltaTime / 10000000F);
                if (_xpos >= textWidth - farPoint)
                {
                    // reached the right end
                    _direction = -1;
                    _xpos = textWidth - farPoint;
                    _paused = true;
                }
                else if (_xpos <= 0)
                {
                    // reached the left end
                    _direction = 1;
                    _xpos = 0;
                    _paused = true;
                }
            }

            // then draw it
            if (_textImage != null)
            {
                g.DrawImage(_textImage, (int)(drawAt), 0, new RectangleF((int)_xpos, 0, (int)farPoint, _textImage.Height), GraphicsUnit.Pixel);
            }

            if (!_paused)
            {
                _lastUpdate = now;
            }
            else if ((deltaTime / 10000) >= _pause)
            {
                _lastUpdate = now;
                _paused = false;
            }
        }

        /// <summary>
        /// This layer doesn't have any fixed size content, like TextLayer, so just use the default
        /// GetMinimalSize implementation
        /// </summary>
        /// <returns>the same as Layer.GetMinimialSize()</returns>
        protected override Size GetMinimalSizeImpl()
        {
            return geometry.GetMinimalSize(GetContentSize());
        }

        /// <summary>
        /// This draws the entire text to a backbuffer. When drawing, the appropriate part of backbuffer is used.
        /// </summary>
        protected override void UpdateText()
        {
            string prevString = left.formatted;
   
            left.formatted = defaultText;
            right.formatted = "";

            if (!string.IsNullOrEmpty(left.text) && Main.PlayControl.IsPlaying())
            {
                // Evaluate only when there is a track, otherwise keep default text
                left.formatted = Main.PlayControl.FormatTitle(Main.PlayControl.GetNowPlaying(), left.text);
            }
  
            if (left.formatted != null)
            {
                SizeF textSize = Main.GetInstance().Display.Canvas.MeasureString(left.formatted, left.font);
                _textImage = new Bitmap(
                    Math.Max(10, (int)Math.Ceiling(textSize.Width)), 
                    Math.Max(10, (int)Math.Ceiling(textSize.Height)), 
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                _textCanvas = Graphics.FromImage(_textImage);
                _textCanvas.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                _textCanvas.DrawString(left.formatted, left.font, new SolidBrush(left.color), new PointF(0, 0));
            }

            if (prevString != left.formatted)
            {
                Main.GetInstance().RequestRedraw();
                if (IsScrollingNeeded())
                {
                    Main.GetInstance().AddRedrawRequester(this);
                }
                else
                {
                    Main.GetInstance().RemoveRedrawRequester(this);
                }
            }
        }

        protected override void OnLayerEnable()
        {
            if (IsScrollingNeeded())
                Main.GetInstance().AddRedrawRequester(this);
        }

        protected override void OnLayerDisable()
        {
            Main.GetInstance().RemoveRedrawRequester(this);
        }

        private bool IsScrollingNeeded()
        {
            float textWidth = TextRenderer.MeasureText(left.formatted, left.font).Width;
            float farPoint = GetFarPointInClientRect(this.angle);

            return textWidth > farPoint;
        }

        bool IContiniousRedraw.IsRedrawNeeded()
        {
            int deltaTime = (int)((DateTime.Now.Ticks - _lastUpdate) / 10000);

            if (_paused && deltaTime < _pause)
                return false;

            return true;
        }
    }
}
