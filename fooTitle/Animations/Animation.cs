using System;

namespace fooTitle
{
	public class Animation
	{
		protected Display display;

		protected int myFrame;
		public int Frame {
			get {
				return myFrame;
			}
			set {
				myFrame = value;
			}
		}

		protected int myMaxFrame;
		public int MaxFrame {
			get {
				return myMaxFrame;
			}
		}

		public Animation(Display display)
		{
			myFrame = 0;
			this.display = display;
		}

		virtual public void Draw() {
			if (Frame <= 0) return;
			myFrame --;
		}

		public void Start() {
			Frame = MaxFrame;
		}
	}
}
