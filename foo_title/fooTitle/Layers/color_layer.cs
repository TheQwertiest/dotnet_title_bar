/*
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

namespace fooTitle.Layers
{
    /// <summary>
    /// A simple layer that is filled with specified color.
    /// </summary>
    [LayerTypeAttribute("color")]
    public class ColorLayer : Layer
    {
        private readonly Color _color;

        public ColorLayer(Rectangle parentRect, XmlNode node) : base(parentRect, node)
        {
            XmlNode contents = GetFirstChildByName(node, "contents");
            _color = ColorFromCode(contents.Attributes.GetNamedItem("color").Value);
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
                fooManagedWrapper.CConsole.Warning(String.Format("Error in text layer {0}, invalid color code {1}.", this.Name, code));
                return Color.Black;
            }
        }

        protected override void drawImpl()
        {
            Display.Canvas.FillRectangle(new SolidBrush(_color), ClientRect.X, ClientRect.Y, ClientRect.Width, ClientRect.Height);
        }

    }
}
