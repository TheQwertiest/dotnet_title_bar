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
using Qwr.ComponentInterface;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml.Linq;

namespace fooTitle.Layers
{
    [LayerType("text")]
    public class TextLayer : Layer
    {
        protected string _defaultText;

        protected LabelPart _left = new();
        protected LabelPart _right = new();

        protected int _space;
        protected int _angle;

        public TextLayer(Rectangle parentRect, XElement node, Skin skin)
            : base(parentRect, node, skin)
        {
            XElement contents = GetFirstChildByName(node, "contents");

            // read the spacing
            _space = GetCastedAttributeValue(contents, "spacing", 20);
            _angle = GetCastedAttributeValue(contents, "angle", 0);

            // read the default font
            LabelPart empty = new LabelPart
            {
                isBold = false,
                isItalic = false,
                fontName = "Arial",
                color = Color.FromArgb(255, 0, 0, 0),
                fontSize = 9,
            };
            LabelPart def = ReadLabel(contents, empty);

            // read contents
            foreach (XElement n in contents.Elements("label"))
            {
                AddLabel(n, def);
            }

            _defaultText = "";
            foreach (XElement n in contents.Elements())
            {
                if (n.Name == "defaultText")
                {
                    _defaultText = GetNodeValue(n, false);
                }
            }

            ParentSkin.PlaybackAdvancedToNewTrack += OnPlaybackNewTrack;
            ParentSkin.TrackPlaybackPositionChanged += OnPlaybackTime;
            ParentSkin.PlaybackStopped += CurrentSkin_OnPlaybackStopEvent;
            ParentSkin.PlaybackPausedStateChanged += CurrentSkin_OnPlaybackPauseEvent;
        }

        public void CurrentSkin_OnPlaybackPauseEvent(bool state)
        {
            UpdateText();
        }

        public void CurrentSkin_OnPlaybackStopEvent(PlaybackStopReason reason)
        {
            if (reason != PlaybackStopReason.StartingAnother)
            {
                UpdateText();
            }
        }

        protected LabelPart ReadLabel(XElement node, LabelPart def)
        {
            LabelPart res = new LabelPart();

            if (node.Attribute("size") != null)
            {
                res.fontSize = int.Parse(node.Attribute("size").Value);
            }
            else
            {
                res.fontSize = def.fontSize;
            }

            if (node.Attribute("italic") != null)
            {
                res.isItalic = bool.Parse(node.Attribute("italic").Value);
            }
            else
            {
                res.isItalic = def.isItalic;
            }

            if (node.Attribute("bold") != null)
            {
                res.isBold = bool.Parse(node.Attribute("bold").Value);
            }
            else
            {
                res.isBold = def.isBold;
            }

            if (node.Attribute("font") != null)
            {
                res.fontName = node.Attribute("font").Value;
            }
            else
            {
                res.fontName = def.fontName;
            }

            if (node.Attribute("color") != null)
            {
                res.color = ColorFromCode(node.Attribute("color").Value);
            }
            else
            {
                res.color = def.color;
            }

            FontStyle fontStyle = FontStyle.Regular;
            if (res.isItalic)
            {
                fontStyle |= FontStyle.Italic;
            }

            if (res.isBold)
            {
                fontStyle |= FontStyle.Bold;
            }

            var isDpiScalable = Configs.Display_IsDpiScalingEnabled.Value;
            res.font = new Font(res.fontName,
                                isDpiScalable ? res.fontSize : (int)Math.Round((double)res.fontSize * 96 / 72),
                                fontStyle,
                                isDpiScalable ? GraphicsUnit.Point : GraphicsUnit.Pixel);
            res.text = GetNodeValue(node, false);

            return res;
        }

        protected virtual void AddLabel(XElement node, LabelPart def)
        {
            string position = GetAttributeValue(node, "position", "left");
            LabelPart label = ReadLabel(node, def);
            switch (position)
            {
                case "left":
                    _left = label;
                    break;
                case "right":
                    _right = label;
                    break;
            }
        }

        protected Color ColorFromCode(string code)
        {
            try
            {
                string a = code.Substring(0, 2).ToLower();
                string r = code.Substring(2, 2).ToLower();
                string g = code.Substring(4, 2).ToLower();
                string b = code.Substring(6, 2).ToLower();

                return Color.FromArgb(
                    int.Parse(a, System.Globalization.NumberStyles.HexNumber),
                    int.Parse(r, System.Globalization.NumberStyles.HexNumber),
                    int.Parse(g, System.Globalization.NumberStyles.HexNumber),
                    int.Parse(b, System.Globalization.NumberStyles.HexNumber));
            }
            catch
            {
                Console.Get().LogWarning($"Error in text layer {Name}, invalid color code {code}.");
                return Color.Black;
            }
        }

        protected override void DrawImpl(Graphics canvas)
        {
            Matrix oldTransform = canvas.Transform;

            Rectangle bounds = CalcRotatedBounds();

            canvas.TranslateTransform(-bounds.X, -bounds.Y);
            canvas.TranslateTransform(ClientRect.X, ClientRect.Y);
            canvas.RotateTransform(_angle);

            StraightDraw(canvas);
            canvas.Transform = oldTransform;
        }

        /// <summary>
        /// Draws at 0,0. Transformation into ClientRect is handled outside.
        /// </summary>
        protected virtual void StraightDraw(Graphics g)
        {
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            //float leftWidth = 0;
            if (!string.IsNullOrEmpty(_left.formatted))
            {
                //leftWidth = Display.Canvas.MeasureString(left.formatted, left.font).Width;
                g.DrawString(_left.formatted, _left.font, new SolidBrush(_left.color), 0, 0);
            }

            if (!string.IsNullOrEmpty(_right.formatted))
            {
                StringFormat rightFormat = new StringFormat { Alignment = StringAlignment.Far };
                //g.DrawString(right.formatted, right.font, new SolidBrush(right.color), space + leftWidth, 0);

                // the text is right-aligned, so we must take the size of client rect into account. But
                // the text can be also rotated, so we must consider different sizes. This will probably
                // not work very well for arbitrary angles, but is ok for 90*n.
                float farEdge = GetFarPointInClientRect(_angle);
                Rectangle drawInto = new Rectangle { Width = (int)farEdge };
                g.DrawString(_right.formatted, _right.font, new SolidBrush(_right.color), drawInto, rightFormat);
            }
        }

        protected virtual void UpdateText()
        {
            var prevLeft = _left.formatted;
            var prevRight = _right.formatted;

            _left.formatted = _defaultText;
            _right.formatted = "";

            var isPlaying = Main.Get().Fb2kPlaybackControls.IsPlaying();
            foreach (var part in new LabelPart[] { _left, _right })
            {
                if (!string.IsNullOrEmpty(part.text) && isPlaying)
                {
                    // Evaluate only when there is a track, otherwise keep default text
                    var tf = Main.Get().Fb2kControls.TitleFormat(part.text);
                    part.formatted = tf.Eval(force: true);
                }
            }

            if (_left.formatted != prevLeft || _right.formatted != prevRight)
            {
                Main.Get().RedrawTitleBar();
            }
        }

        protected override Size GetMinimalSizeImpl()
        {
            return ContainedGeometry.GetMinimalSize(CalcRotatedBounds().Size);
        }

        /// <returns>
        /// Size of text, not rotated.
        /// </returns>
        private Size CalcStraightSize()
        {
            float width = 0;

            StringFormat sf = new StringFormat(StringFormat.GenericDefault) { FormatFlags = StringFormatFlags.MeasureTrailingSpaces };
            var canvas = SkinForm.CanvasForMeasure;

            if (!string.IsNullOrEmpty(_left.formatted))
            {
                width += canvas.MeasureString(_left.formatted, _left.font, new PointF(0, 0), sf).Width;
            }
            if (!string.IsNullOrEmpty(_right.formatted))
            {
                width += canvas.MeasureString(_right.formatted, _right.font, new PointF(0, 0), sf).Width;
                width += _space;
            }

            float height = 0;
            if (!string.IsNullOrEmpty(_left.formatted))
            {
                height = canvas.MeasureString(_left.formatted, _left.font).Height;
            }

            if (!string.IsNullOrEmpty(_right.formatted))
            {
                height = Math.Max(height, canvas.MeasureString(_right.formatted, _right.font).Height);
            }

            return new Size((int)Math.Ceiling(width), (int)Math.Ceiling(height));
        }

        // TODO: remove

        private static void SetLeft(ref Rectangle r, int val)
        {
            int oldRight = r.Right;
            r.X = val;
            SetRight(ref r, oldRight);
        }

        private static void SetTop(ref Rectangle r, int val)
        {
            int oldBottom = r.Bottom;
            r.Y = val;
            SetBottom(ref r, oldBottom);
        }

        private static void SetRight(ref Rectangle r, int val)
        {
            r.Width = val - r.X;
        }

        private static void SetBottom(ref Rectangle r, int val)
        {
            r.Height = val - r.Y;
        }

        private Rectangle CalcRotatedBounds()
        {
            Size size = CalcStraightSize();
            Matrix transform = new Matrix();
            transform.Rotate(_angle);
            Point[] boundPoints = {
                new Point(0, 0),
                new Point(size.Width, 0),
                new Point(size.Width, size.Height),
                new Point(0, size.Height)
            };
            transform.TransformPoints(boundPoints);

            Rectangle result = new Rectangle();

            foreach (Point p in boundPoints)
            {
                if (p.X < result.X)
                {
                    SetLeft(ref result, p.X);
                }

                if (p.X > result.Right)
                {
                    SetRight(ref result, p.X);
                }

                if (p.Y < result.Y)
                {
                    SetTop(ref result, p.Y);
                }

                if (p.Y > result.Bottom)
                {
                    SetBottom(ref result, p.Y);
                }
            }

            return result;
        }

        /// <returns>
        /// Returns how much space is there in the client rect when we draw the text
        /// under given angle.
        /// </returns>
        /// <remarks>
        /// Only works correctly for 90*n.
        /// </remarks>
        protected float GetFarPointInClientRect(int _angle)
        {
            if ((_angle < 45 || _angle > (360 - 45)) || (_angle > (180 - 45) && _angle < (180 + 45)))
            {
                return ClientRect.Width;
            }
            return ClientRect.Height;
        }

        public void OnPlaybackNewTrack(IMetadbHandle song)
        {
            UpdateText();
        }

        public void OnPlaybackTime(double time)
        {
            UpdateText();
        }
        protected class LabelPart
        {
            public string text;
            public string formatted;
            public Font font;
            public Color color;

            // Font properties
            public string fontName;
            public int fontSize;
            public bool isBold;
            public bool isItalic;
        }
    }
}
