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
using System.Drawing;
using fooManagedWrapper;
using naid;
using System.Drawing.Drawing2D;

namespace fooTitle.Layers
{
    [LayerTypeAttribute("text")]
	public class TextLayer : Layer
	{
        protected class LabelPart {
            public string text;
            public string formatted;
            public Font font;
            public Color color;

            // Font properties
            public string fontName;
            public int fontSize;
            public bool isBold;
            public bool isItalic;
            public bool isScaleable;
        }
 
        protected string defaultText;

        protected LabelPart left = new LabelPart();
        protected LabelPart right = new LabelPart();

		protected int space;
        protected int angle;

		public TextLayer(Rectangle parentRect, XmlNode node) : base(parentRect, node) {
            XmlNode contents = GetFirstChildByName(node, "contents");
			
			// read the spacing
			space = Int32.Parse(GetAttributeValue(contents, "spacing", "20"));
            angle = Int32.Parse(GetAttributeValue(contents, "angle", "0"));

            // read the default font
            LabelPart empty = new LabelPart
		    {
		        isBold = false,
		        isItalic = false,
		        fontName = "Arial",
		        color = Color.FromArgb(255, 0, 0, 0),
		        fontSize = 9,
		        isScaleable = bool.Parse(GetAttributeValue(contents, "scaleable", "false")),
            };
		    LabelPart def = ReadLabel(contents, empty);
 
			// read contents
            foreach (XmlNode n in contents.SelectNodes("label")) {
                addLabel(n, def);
            }

            defaultText = "";
            foreach (XmlNode n in contents.ChildNodes) {
                if (n.Name == "defaultText")
                    defaultText = GetNodeValue(n);
            }
 
            if (Main.GetInstance().CurrentSkin != null) {
                Main.GetInstance().CurrentSkin.OnPlaybackNewTrackEvent += this.OnPlaybackNewTrack;
                Main.GetInstance().CurrentSkin.OnPlaybackTimeEvent += this.OnPlaybackTime;
                Main.GetInstance().CurrentSkin.OnPlaybackStopEvent += CurrentSkin_OnPlaybackStopEvent;
                Main.GetInstance().CurrentSkin.OnPlaybackPauseEvent += CurrentSkin_OnPlaybackPauseEvent;
            }
		}

        public void CurrentSkin_OnPlaybackPauseEvent(bool state) {
            updateText();
        }

        public void CurrentSkin_OnPlaybackStopEvent(IPlayControl.StopReason reason) {
            if (reason != IPlayControl.StopReason.stop_reason_starting_another) {
                updateText();
            }
        }
        
        protected LabelPart ReadLabel(XmlNode node, LabelPart def) {
            LabelPart res = new LabelPart();

            if (node.Attributes.GetNamedItem("size") != null)
                res.fontSize = int.Parse(node.Attributes.GetNamedItem("size").Value);
            else
                res.fontSize = def.fontSize;
            
            if (node.Attributes.GetNamedItem("italic") != null)
                res.isItalic = bool.Parse(node.Attributes.GetNamedItem("italic").Value);
            else
                res.isItalic = def.isItalic;

            if (node.Attributes.GetNamedItem("bold") != null)
                res.isBold = bool.Parse(node.Attributes.GetNamedItem("bold").Value);
            else
                res.isBold = def.isBold;

            if (node.Attributes.GetNamedItem("font") != null)
                res.fontName = node.Attributes.GetNamedItem("font").Value;
            else
                res.fontName = def.fontName;

            if (node.Attributes.GetNamedItem("color") != null)
                res.color = colorFromCode(node.Attributes.GetNamedItem("color").Value);
            else
                res.color = def.color;

            
            FontStyle fontStyle = FontStyle.Regular;
            if (res.isItalic)
                fontStyle |= FontStyle.Italic;
            if (res.isBold)
                fontStyle |= FontStyle.Bold;
            res.font = new Font(res.fontName,
                res.isScaleable ? res.fontSize : (int)Math.Round((double)res.fontSize * 96 / 72),
                fontStyle,
                res.isScaleable ? GraphicsUnit.Point : GraphicsUnit.Pixel);
            res.text = GetNodeValue(node);

            return res;
        }

		protected virtual void addLabel(XmlNode node, LabelPart def) {
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

        protected Color colorFromCode(string code) {
            try {
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
            } catch {
                fooManagedWrapper.CConsole.Warning($"Error in text layer {this.Name}, invalid color code {code}.");
                return Color.Black;
            }
        }

		protected override void DrawImpl() {
            Matrix oldTransform = Display.Canvas.Transform;

            Rectangle bounds = calcRotatedBounds();

            Display.Canvas.TranslateTransform(-bounds.X, -bounds.Y);
            Display.Canvas.TranslateTransform(ClientRect.X, ClientRect.Y);

            Display.Canvas.RotateTransform(angle);
            
            straightDraw(Display.Canvas);
            Display.Canvas.Transform = oldTransform;
		}

        /// <summary>
        /// Draws at 0,0. Transformation into ClientRect is handled outside.
        /// </summary>
        protected virtual void straightDraw(Graphics g) {
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            float leftWidth = 0;
            if (!string.IsNullOrEmpty(left.formatted)) {
                leftWidth = Display.Canvas.MeasureString(left.formatted, left.font).Width;
                g.DrawString(left.formatted, left.font, new SolidBrush(left.color), 0, 0);
            }

            if (!string.IsNullOrEmpty(right.formatted)) {
                StringFormat rightFormat = new StringFormat {Alignment = StringAlignment.Far};
                //g.DrawString(right.formatted, right.font, new SolidBrush(right.color), space + leftWidth, 0);
                
                // the text is right-aligned, so we must take the size of client rect into account. But
                // the text can be also rotated, so we must consider different sizes. This will probably
                // not work very well for arbitrary angles, but is ok for 90*n.
                float farEdge = getFarPointInClientRect(angle);
                Rectangle drawInto = new Rectangle {Width = (int) farEdge};
                g.DrawString(right.formatted, right.font, new SolidBrush(right.color), drawInto, rightFormat);

            }
        }

		protected virtual void updateText() {
            LabelPart[] parts = new LabelPart[2];
            parts[0] = left;
            parts[1] = right;


		    string[] prevText = {left.formatted, right.formatted};		    

            left.formatted = defaultText;
            right.formatted = "";

            for (int i = 0; i < 2; ++i ) {
                if (!string.IsNullOrEmpty(parts[i].text) && Main.PlayControl.IsPlaying()) {
                    // Evaluate only when there is a track, otherwise keep default text
                    parts[i].formatted = Main.PlayControl.FormatTitle(Main.PlayControl.GetNowPlaying(), parts[i].text);
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

		protected override Size GetMinimalSizeImpl() {
            return geometry.GetMinimalSize(calcRotatedBounds().Size);
		}

        /// <returns>
        /// Size of text, not rotated.
        /// </returns>
        private Size calcStraightSize() {
            float width = 0;
            if (!string.IsNullOrEmpty(left.formatted))
                width += Display.Canvas.MeasureString(left.formatted, left.font).Width;
            if (!string.IsNullOrEmpty(right.formatted)) {
                width += Display.Canvas.MeasureString(right.formatted, right.font).Width;
                width += space;
            }

            int height = 0;
            if (!string.IsNullOrEmpty(left.formatted))
                height = (int)Display.Canvas.MeasureString(left.formatted, left.font).Height;
            if (!string.IsNullOrEmpty(right.formatted))
                height = Math.Max(height, (int)Display.Canvas.MeasureString(right.formatted, right.font).Height);

            return new Size((int)width, (int)height);
        }

        private Rectangle calcRotatedBounds() {
            Size size = calcStraightSize();
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

            foreach (Point p in boundPoints) {
                if (p.X < result.X)
                    RectUtils.SetLeft(ref result, p.X);
                if (p.X > result.Right)
                    RectUtils.SetRight(ref result, p.X);
                if (p.Y < result.Y)
                    RectUtils.SetTop(ref result, p.Y);
                if (p.Y > result.Bottom)
                    RectUtils.SetBottom(ref result, p.Y);
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
        protected float getFarPointInClientRect(int _angle) {
            if ((_angle < 45 || _angle > (360 - 45)) || (_angle > (180 - 45) && _angle < (180 + 45))) {
                return ClientRect.Width;
            } else {
                return ClientRect.Height;
            }
        }

		public void OnPlaybackNewTrack(fooManagedWrapper.CMetaDBHandle song) {
			updateText();
		}

		public void OnPlaybackTime(double time) {
			updateText();
		}

	}
}
