/*
*  This file is part of foo_title.
*  Copyright 2005 - 2006 Roman Plasil (http://foo-title.sourceforge.net)
*  Copyright 2017 TheQwertiest (https://github.com/TheQwertiest/foo_title)
*  
*  This library is free software; you can redistribute it and/or
*  modify it under the terms of the GNU Lesser General Public
*  License as published by the Free Software Foundation; either
*  version 2.1 of the License, or (at your option) any later version.
*  
*  This library is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  
*  
*  See the file COPYING included with this distribution for more
*  information.
*/

using System.Drawing;
using System.Xml.Linq;

namespace fooTitle.Layers
{
    /// <summary>
    /// A simple layer that is filled with specified color.
    /// </summary>
    [LayerType("color")]
    public class ColorLayer : Layer
    {
        private readonly Color _color;

        public ColorLayer(Rectangle parentRect, XElement node, Skin skin)
            : base(parentRect, node, skin)
        {
            XElement contents = GetFirstChildByName(node, "contents");
            _color = ColorFromCode(contents.Attribute("color").Value);
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
            canvas.FillRectangle(new SolidBrush(_color), ClientRect.X, ClientRect.Y, ClientRect.Width, ClientRect.Height);
        }
    }
}
