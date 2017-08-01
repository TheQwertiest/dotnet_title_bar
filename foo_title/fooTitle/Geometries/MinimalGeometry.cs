using System;
using System.Drawing;
using System.Xml;

namespace fooTitle.Geometries {
    [GeometryTypeAttribute("minimal")]
    internal class MinimalGeometry : Geometry{

        /// <summary>
        /// Current values of padding
        /// </summary>
        protected Padding MyPadding;

        /// <summary>
        /// Stored expressions to calculate padding from
        /// </summary>
        protected ExpressionPadding MyExprPadding;

        private Size _curContentSize;

        public MinimalGeometry(Rectangle parentRect, XmlNode node) : base(parentRect, node) {
            XmlNode padding = GetFirstChildByName(node, "padding");

            // read and store expressions
            MyExprPadding.Left = GetExpressionFromAttribute(padding, "left", "0");
            MyExprPadding.Top = GetExpressionFromAttribute(padding, "top", "0");
            MyExprPadding.Right = GetExpressionFromAttribute(padding, "right", "0");
            MyExprPadding.Bottom = GetExpressionFromAttribute(padding, "bottom", "0");

            // get the actual values
            MyPadding.Left = (int)GetNumberFromAttribute(padding, "left", "0");
            MyPadding.Top = (int)GetNumberFromAttribute(padding, "top", "0");
            MyPadding.Right = (int)GetNumberFromAttribute(padding, "right", "0");
            MyPadding.Bottom = (int)GetNumberFromAttribute(padding, "bottom", "0");
            
        }

        public override void Update(Rectangle parentRect) {
            MyPadding.Left = GetValueFromExpression(MyExprPadding.Left, MyPadding.Left);
            MyPadding.Top = GetValueFromExpression(MyExprPadding.Top, MyPadding.Top);
            MyPadding.Right = GetValueFromExpression(MyExprPadding.Right, MyPadding.Right);
            MyPadding.Bottom = GetValueFromExpression(MyExprPadding.Bottom, MyPadding.Bottom);

            myClientRect.X = MyPadding.Left + parentRect.Left;
            myClientRect.Y = MyPadding.Top + parentRect.Top;

            myClientRect.Width = Math.Min(_curContentSize.Width, parentRect.Width) - (MyPadding.Left + MyPadding.Right);
            myClientRect.Height = Math.Min(_curContentSize.Height, parentRect.Height) - (MyPadding.Top + MyPadding.Bottom);
        }

        public override Size GetMinimalSize(Size contentSize)
        {
            _curContentSize = new Size(MyPadding.Left + MyPadding.Right + contentSize.Width,
                MyPadding.Top + MyPadding.Bottom + contentSize.Height);
            return _curContentSize;
        }

        public override Point GetPosition() {
            return new Point(0, 0);
        }

        public override bool IsDynamic() {
            return (MyExprPadding.Bottom != null || MyExprPadding.Left != null || MyExprPadding.Right != null || MyExprPadding.Top != null);
        }
    }
}
