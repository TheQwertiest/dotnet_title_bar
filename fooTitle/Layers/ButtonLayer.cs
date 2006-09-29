using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Drawing;
using System.Windows.Forms;
using fooManagedWrapper;

namespace fooTitle.Layers {
    [LayerTypeAttribute("button")]
    class ButtonLayer : Layer{

        protected string myAction;
        protected Bitmap myNormalImage;
        protected Bitmap myOverImage;
        protected Bitmap myDownImage;

        protected bool mouseOn;
        protected bool mouseDown;

        public ButtonLayer(Rectangle parentRect, XmlNode node) : base(parentRect, node) {
            XmlNode contents = GetFirstChildByName(node, "contents");
            XmlNode action = GetFirstChildByName(contents, "action");
            myAction = GetNodeValue(action);
            
            XmlNode img;
            img = GetFirstChildByName(contents, "normalImg");
            myNormalImage = new Bitmap(Main.GetInstance().CurrentSkin.GetSkinFilePath(img.Attributes.GetNamedItem("src").Value));

            img = GetFirstChildByName(contents, "overImg");
            myOverImage = new Bitmap(Main.GetInstance().CurrentSkin.GetSkinFilePath(img.Attributes.GetNamedItem("src").Value));

            img = GetFirstChildByName(contents, "downImg");
            myDownImage = new Bitmap(Main.GetInstance().CurrentSkin.GetSkinFilePath(img.Attributes.GetNamedItem("src").Value));

            // register mouse events
            Main.GetInstance().CurrentSkin.OnMouseMove += new MouseEventHandler(OnMouseMove);
            Main.GetInstance().CurrentSkin.OnMouseDown += new MouseEventHandler(OnMouseDown);
            Main.GetInstance().CurrentSkin.OnMouseUp += new MouseEventHandler(OnMouseUp);
            Main.GetInstance().CurrentSkin.OnMouseLeave += new EventHandler(OnMouseLeave);

        }

        void OnMouseLeave(object sender, EventArgs e) {
            mouseOn = false;            
        }

        void OnMouseUp(object sender, MouseEventArgs e) {
            mouseDown = false;

            if (mouseOn)
                CManagedWrapper.DoMainMenuCommand(myAction);
        }

        void OnMouseDown(object sender, MouseEventArgs e) {
            if (mouseOn)
                mouseDown = true;
        }

        void OnMouseMove(object sender, MouseEventArgs e) {
            mouseOn = (e.X >= ClientRect.Left) && (e.X <= ClientRect.Right) &&
                      (e.Y >= ClientRect.Top) && (e.Y <= ClientRect.Bottom);
            if (!mouseOn)
                mouseDown = false;
        }


        public override void Draw() {
            Bitmap toDraw;
            if (mouseDown)
                toDraw = myDownImage;
            else if (mouseOn)
                toDraw = myOverImage;
            else
                toDraw = myNormalImage;
            
            Display.Canvas.DrawImage(toDraw, ClientRect.X, ClientRect.Y, ClientRect.Width, ClientRect.Height);
            base.Draw();
        }


    }
}
