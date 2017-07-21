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

namespace fooTitle
{
	public class FlashAnimation : Animation
	{
		Color Color;

		public FlashAnimation(Display display) : base(display)
		{
			Color = Color.FromArgb(255, 255, 255);
			myMaxFrame = 4;
		}

		override public void Draw() {
			if (Frame <= 0) return;
			base.Draw();

			// sets the drawing color to some alpha-blend of this.Color
			// the first frame is fully solid, the last frame is fully transparent
			Color drawColor = Color.FromArgb((int)(Frame / (float)MaxFrame * 255.0), this.Color);
			
			const int MAX_EMPTY_WIDTH = 30;
			int emptyWidth = (int) ((Frame / MaxFrame) * MAX_EMPTY_WIDTH);
			//display.DrawRectangle(emptyWidth, 0, display.GetWidth() - emptyWidth, display.GetHeight(), drawColor);
			display.Canvas.FillRectangle(new SolidBrush(drawColor), emptyWidth, 0, display.Width - 2*emptyWidth, display.Height);
		}
	}
}
