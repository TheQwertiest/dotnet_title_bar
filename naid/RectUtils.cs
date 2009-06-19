using System;
using System.Collections.Generic;
using System.Text;


namespace naid {
    public static class RectUtils  {

        public static void SetLeft(ref System.Drawing.Rectangle r, int val) {
            int oldRight = r.Right;
            r.X = val;
            SetRight(ref r, oldRight);
        }

        public static void SetTop(ref System.Drawing.Rectangle r, int val) {
            int oldBottom = r.Bottom;
            r.Y = val;
            SetBottom(ref r, oldBottom);
        }

        public static void SetRight(ref System.Drawing.Rectangle r, int val) {
            r.Width = val - r.X;
        }

        public static void SetBottom(ref System.Drawing.Rectangle r, int val) {
            r.Height = val - r.Y;
        }


    }
}
