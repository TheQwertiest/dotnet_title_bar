using System;
using System.Drawing;
using System.Xml;

namespace fooTitle.Geometries {
    [GeometryTypeAttribute("minimal")]
    internal class MinimalGeometry : Geometry{
        [Flags]
        public enum AlignType
        {
            None = 0,
            Left = 1,
            Right = 1 << 1,
            Top = 1 << 2,
            Bottom = 1 << 4,
            Center = 1 << 5
        };

        private AlignType _align;

        /// <summary>
        /// Current values of padding
        /// </summary>
        protected Padding MyPadding;

        /// <summary>
        /// Stored expressions to calculate padding from
        /// </summary>
        protected ExpressionPadding MyExprPadding;

        private Size _curContentSize;

        private Point _relPosition;
        public Point Position => _relPosition;

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

            // read alignment
            XmlNode positionNode = GetFirstChildByNameOrNull(node, "position");
            string alignStr = "left,top";
            if (positionNode != null )
                alignStr = GetAttributeValue(positionNode, "align", "left,top");

            _align = AlignType.None;
            foreach (string i in alignStr.ToLower().Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                switch (i)
                {
                    case "top":
                        _align |= AlignType.Top;
                        break;
                    case "bottom":
                        _align |= AlignType.Bottom;
                        break;
                    case "right":
                        _align |= AlignType.Right;
                        break;
                    case "left":
                        _align |= AlignType.Left;
                        break;
                    case "center":
                        _align |= AlignType.Center;
                        break;
                }
            }
        }

        public override void Update(Rectangle parentRect) {
            MyPadding.Left = GetValueFromExpression(MyExprPadding.Left, MyPadding.Left);
            MyPadding.Top = GetValueFromExpression(MyExprPadding.Top, MyPadding.Top);
            MyPadding.Right = GetValueFromExpression(MyExprPadding.Right, MyPadding.Right);
            MyPadding.Bottom = GetValueFromExpression(MyExprPadding.Bottom, MyPadding.Bottom);

            int myWidth = Math.Min(parentRect.Width - (MyPadding.Left + MyPadding.Right), _curContentSize.Width);
            int myHeight = Math.Min(parentRect.Height - (MyPadding.Top + MyPadding.Bottom), _curContentSize.Height);
            myClientRect.Width = Math.Max(0, myWidth);
            myClientRect.Height = Math.Max(0, myHeight);

            if ((_align & AlignType.Left) != 0)
            {
                _relPosition.X = 0;
            }
            else if ((_align & AlignType.Right) != 0)
            {
                _relPosition.X = parentRect.Width - (myClientRect.Width + MyPadding.Right + MyPadding.Left);
            }
            else
            {
                _relPosition.X = (parentRect.Width - (myClientRect.Width + MyPadding.Right - MyPadding.Left)) / 2;
            }

            if ((_align & AlignType.Top) != 0)
            {
                _relPosition.Y = 0;
            }
            else if ((_align & AlignType.Bottom) != 0)
            {
                _relPosition.Y = parentRect.Height - (myClientRect.Height + MyPadding.Bottom + MyPadding.Top);
            }
            else
            {
                _relPosition.Y = (parentRect.Height - (myClientRect.Height + MyPadding.Bottom - MyPadding.Top))/2;
            }

            myClientRect.X = parentRect.Left + MyPadding.Left + _relPosition.X;
            myClientRect.Y = parentRect.Top + MyPadding.Top + _relPosition.Y;
        }

        public override Size GetMinimalSize(Size contentSize)
        {
            _curContentSize = contentSize;
            return new Size(MyPadding.Left + MyPadding.Right + contentSize.Width,
                MyPadding.Top + MyPadding.Bottom + contentSize.Height);
        }

        public override Point GetPosition()
        {
            return new Point(0,0);
        }

        public override bool IsDynamic() {
            return (MyExprPadding.Bottom != null || MyExprPadding.Left != null || MyExprPadding.Right != null || MyExprPadding.Top != null);
        }
    }
}
