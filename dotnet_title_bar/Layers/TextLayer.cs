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
    [LayerTypeAttribute("text")]
    public class TextLayer : Layer
    {
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

        protected string defaultText;

        protected LabelPart left = new LabelPart();
        protected LabelPart right = new LabelPart();

        protected int space;
        protected int angle;

        public TextLayer(Rectangle parentRect, XElement node) : base(parentRect, node)
        {
            XElement contents = GetFirstChildByName(node, "contents");

            // read the spacing
            space = GetCastedAttributeValue(contents, "spacing", 20);
            angle = GetCastedAttributeValue(contents, "angle", 0);

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

            defaultText = "";
            foreach (XElement n in contents.Elements())
            {
                if (n.Name == "defaultText")
                    defaultText = GetNodeValue(n, false);
            }

            if (Main.GetInstance().CurrentSkin != null)
            {
                Main.GetInstance().CurrentSkin.PlaybackAdvancedToNewTrack += OnPlaybackNewTrack;
                Main.GetInstance().CurrentSkin.TrackPlaybackPositionChanged += OnPlaybackTime;
                Main.GetInstance().CurrentSkin.PlaybackStopped += CurrentSkin_OnPlaybackStopEvent;
                Main.GetInstance().CurrentSkin.PlaybackPausedStateChanged += CurrentSkin_OnPlaybackPauseEvent;
            }
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
                res.fontSize = int.Parse(node.Attribute("size").Value);
            else
                res.fontSize = def.fontSize;

            if (node.Attribute("italic") != null)
                res.isItalic = bool.Parse(node.Attribute("italic").Value);
            else
                res.isItalic = def.isItalic;

            if (node.Attribute("bold") != null)
                res.isBold = bool.Parse(node.Attribute("bold").Value);
            else
                res.isBold = def.isBold;

            if (node.Attribute("font") != null)
                res.fontName = node.Attribute("font").Value;
            else
                res.fontName = def.fontName;

            if (node.Attribute("color") != null)
                res.color = ColorFromCode(node.Attribute("color").Value);
            else
                res.color = def.color;

            FontStyle fontStyle = FontStyle.Regular;
            if (res.isItalic)
                fontStyle |= FontStyle.Italic;
            if (res.isBold)
                fontStyle |= FontStyle.Bold;
            res.font = new Font(res.fontName,
                Main.GetInstance().IsDpiScalable ? res.fontSize : (int)Math.Round((double)res.fontSize * 96 / 72),
                fontStyle,
                Main.GetInstance().IsDpiScalable ? GraphicsUnit.Point : GraphicsUnit.Pixel);
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
                    this.left = label;
                    break;
                case "right":
                    this.right = label;
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
                    int.Parse(b, System.Globalization.NumberStyles.HexNumber)
                );
            }
            catch
            {
                Main.Console.LogWarning($"Error in text layer {Name}, invalid color code {code}.");
                return Color.Black;
            }
        }

        protected override void DrawImpl()
        {
            Matrix oldTransform = Display.Canvas.Transform;

            Rectangle bounds = CalcRotatedBounds();

            Display.Canvas.TranslateTransform(-bounds.X, -bounds.Y);
            Display.Canvas.TranslateTransform(ClientRect.X, ClientRect.Y);

            Display.Canvas.RotateTransform(angle);

            StraightDraw(Display.Canvas);
            Display.Canvas.Transform = oldTransform;
        }

        /// <summary>
        /// Draws at 0,0. Transformation into ClientRect is handled outside.
        /// </summary>
        protected virtual void StraightDraw(Graphics g)
        {
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            //float leftWidth = 0;
            if (!string.IsNullOrEmpty(left.formatted))
            {
                //leftWidth = Display.Canvas.MeasureString(left.formatted, left.font).Width;
                g.DrawString(left.formatted, left.font, new SolidBrush(left.color), 0, 0);
            }

            if (!string.IsNullOrEmpty(right.formatted))
            {
                StringFormat rightFormat = new StringFormat { Alignment = StringAlignment.Far };
                //g.DrawString(right.formatted, right.font, new SolidBrush(right.color), space + leftWidth, 0);

                // the text is right-aligned, so we must take the size of client rect into account. But
                // the text can be also rotated, so we must consider different sizes. This will probably
                // not work very well for arbitrary angles, but is ok for 90*n.
                float farEdge = GetFarPointInClientRect(angle);
                Rectangle drawInto = new Rectangle { Width = (int)farEdge };
                g.DrawString(right.formatted, right.font, new SolidBrush(right.color), drawInto, rightFormat);

            }
        }

        protected virtual void UpdateText()
        {
            LabelPart[] parts = new LabelPart[2];
            parts[0] = left;
            parts[1] = right;


            string[] prevText = { left.formatted, right.formatted };

            left.formatted = defaultText;
            right.formatted = "";

            var isPlaying = Main.GetInstance().Fb2kPlaybackControls.IsPlaying();
            for (int i = 0; i < 2; ++i)
            {
                if (!string.IsNullOrEmpty(parts[i].text) && isPlaying)
                {
                    // Evaluate only when there is a track, otherwise keep default text
                    var tf = Main.GetInstance().Fb2kControls.TitleFormat(parts[i].text);
                    parts[i].formatted = tf.Eval(force: true);
                }
            }

            for (int i = 0; i < 2; ++i)
            {
                if (prevText[i] != parts[i].formatted)
                {
                    Main.GetInstance().RequestRedraw();
                    break;
                }
            }
        }

        protected override Size GetMinimalSizeImpl()
        {
            return geometry.GetMinimalSize(CalcRotatedBounds().Size);
        }

        /// <returns>
        /// Size of text, not rotated.
        /// </returns>
        private Size CalcStraightSize()
        {
            float width = 0;

            StringFormat sf = new StringFormat(StringFormat.GenericDefault)
            { FormatFlags = StringFormatFlags.MeasureTrailingSpaces };

            if (!string.IsNullOrEmpty(left.formatted))
            {
                width += Display.Canvas.MeasureString(left.formatted, left.font, new PointF(0, 0), sf).Width;
            }
            if (!string.IsNullOrEmpty(right.formatted))
            {
                width += Display.Canvas.MeasureString(right.formatted, right.font, new PointF(0, 0), sf).Width;
                width += space;
            }

            float height = 0;
            if (!string.IsNullOrEmpty(left.formatted))
                height = Display.Canvas.MeasureString(left.formatted, left.font).Height;
            if (!string.IsNullOrEmpty(right.formatted))
                height = Math.Max(height, Display.Canvas.MeasureString(right.formatted, right.font).Height);

            return new Size((int)Math.Ceiling(width), (int)Math.Ceiling(height));
        }

        // TODO: remove

        private static void SetLeft(ref System.Drawing.Rectangle r, int val)
        {
            int oldRight = r.Right;
            r.X = val;
            SetRight(ref r, oldRight);
        }

        private static void SetTop(ref System.Drawing.Rectangle r, int val)
        {
            int oldBottom = r.Bottom;
            r.Y = val;
            SetBottom(ref r, oldBottom);
        }

        private static void SetRight(ref System.Drawing.Rectangle r, int val)
        {
            r.Width = val - r.X;
        }

        private static void SetBottom(ref System.Drawing.Rectangle r, int val)
        {
            r.Height = val - r.Y;
        }

        private Rectangle CalcRotatedBounds()
        {
            Size size = CalcStraightSize();
            Matrix transform = new Matrix();
            transform.Rotate(angle);
            Point[] boundPoints = {
                new Point(0,0),
                new Point(size.Width, 0),
                new Point(size.Width, size.Height),
                new Point(0, size.Height)
            };
            transform.TransformPoints(boundPoints);

            Rectangle result = new Rectangle();

            foreach (Point p in boundPoints)
            {
                if (p.X < result.X)
                    SetLeft(ref result, p.X);
                if (p.X > result.Right)
                    SetRight(ref result, p.X);
                if (p.Y < result.Y)
                    SetTop(ref result, p.Y);
                if (p.Y > result.Bottom)
                    SetBottom(ref result, p.Y);
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
    }
}