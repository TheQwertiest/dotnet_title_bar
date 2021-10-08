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
    /// A layer for organizing other layers, has no content itself.
    /// </summary>
    [LayerType("no-content")]
    public class NoContentLayer : Layer
    {
        public override bool HasToolTip => false;

        public NoContentLayer(Rectangle parentRect, XElement node, Skin skin)
            : base(parentRect, node, skin)
        {
        }
    }
}
