using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Linq;

namespace fooTitle.Layers
{
    [LayerType("scrolling-text")]
    public class ScrollingTextLayer : TextLayer, IContiniousRedraw
    {
        private int _direction = 1;
        /// <summary>
        /// Number of pixels to move the text per second.
        /// </summary>
        private readonly int _scrollSpeedPxPerSec = 25;
        /// <summary>
        /// Length of pause in milliseconds when the text arrives to either of its edges.
        /// </summary>
        private readonly int _pauseDurationInMs = 1000;
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

        public ScrollingTextLayer(Rectangle parentRect, XElement node, Skin skin)
            : base(parentRect, node, skin)
        {
            XElement contents = GetFirstChildByName(node, "contents");

            _scrollSpeedPxPerSec = DpiHandler.ScaleValueByDpi(GetCastedAttributeValue(contents, "speed", 25));
            _pauseDurationInMs = GetCastedAttributeValue(contents, "pause", 1000);
        }

        protected override void AddLabel(XElement node, LabelPart def)
        {
            string position = GetAttributeValue(node, "position", "left");
            _left = ReadLabel(node, def);

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
            StringFormat sf = new StringFormat(StringFormat.GenericDefault) { FormatFlags = StringFormatFlags.MeasureTrailingSpaces };

            // first move the text
            long now = DateTime.Now.Ticks;
            long deltaTime = now - _lastUpdate;
            float textWidth = g.MeasureString(_left.formatted, _left.font, new PointF(0, 0), sf).Width;
            float farPoint = this.GetFarPointInClientRect(this._angle);
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
                _xpos += _direction * _scrollSpeedPxPerSec * (deltaTime / 10000000F);
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
            else if ((deltaTime / 10000) >= _pauseDurationInMs)
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
            return ContainedGeometry.GetMinimalSize(GetContentSize());
        }

        /// <summary>
        /// This draws the entire text to a backbuffer. When drawing, the appropriate part of backbuffer is used.
        /// </summary>
        protected override void UpdateText()
        {
            string prevString = _left.formatted;

            _left.formatted = _defaultText;
            _right.formatted = "";

            var isPlaying = Main.Get().Fb2kPlaybackControls.IsPlaying();

            if (!string.IsNullOrEmpty(_left.text) && isPlaying)
            {// Evaluate only when there is a track, otherwise keep default text
                var tf = Main.Get().Fb2kControls.TitleFormat(_left.text);
                _left.formatted = tf.Eval(force: true);
            }

            if (_left.formatted != null)
            {
                StringFormat sf = new StringFormat(StringFormat.GenericDefault) { FormatFlags = StringFormatFlags.MeasureTrailingSpaces };

                SizeF textSize = SkinForm.CanvasForMeasure.MeasureString(_left.formatted, _left.font, new PointF(0, 0), sf);
                _textImage = new Bitmap(Math.Max(10, (int)Math.Ceiling(textSize.Width)),
                                        Math.Max(10, (int)Math.Ceiling(textSize.Height)),
                                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                _textCanvas = Graphics.FromImage(_textImage);
                _textCanvas.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                _textCanvas.DrawString(_left.formatted, _left.font, new SolidBrush(_left.color), new PointF(0, 0));
            }

            if (prevString != _left.formatted)
            {
                Main.Get().RedrawTitleBar();
                if (IsScrollingNeeded())
                {
                    Main.Get().AddRedrawRequester(this);
                }
                else
                {
                    Main.Get().RemoveRedrawRequester(this);
                }
            }
        }

        protected override void OnLayerEnable()
        {
            if (IsScrollingNeeded())
            {
                Main.Get().AddRedrawRequester(this);
            }
        }

        protected override void OnLayerDisable()
        {
            Main.Get().RemoveRedrawRequester(this);
        }

        private bool IsScrollingNeeded()
        {
            float textWidth = TextRenderer.MeasureText(_left.formatted, _left.font).Width;
            float farPoint = GetFarPointInClientRect(this._angle);

            return textWidth > farPoint;
        }

        bool IContiniousRedraw.IsRedrawNeeded()
        {
            int deltaTime = (int)((DateTime.Now.Ticks - _lastUpdate) / 10000);
            return (!_paused || deltaTime >= _pauseDurationInMs);
        }
    }
}
