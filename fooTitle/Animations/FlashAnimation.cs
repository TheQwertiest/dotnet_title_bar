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
