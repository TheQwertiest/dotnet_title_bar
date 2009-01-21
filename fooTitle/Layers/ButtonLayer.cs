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
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Drawing;
using System.Windows.Forms;

using naid;

using fooManagedWrapper;
using fooTitle.Extending;


namespace fooTitle.Layers {

    interface IButtonAction {
        void Init(XmlNode node);
        void Run();
    };

    class MainMenuAction : IButtonAction {
        private string commandName;
        private List<String> path;
        private string originalCmd;

        public MainMenuAction(string cmd) {
            readCommand(cmd);
        }

        private void readCommand(string cmd) {
            originalCmd = cmd;
            path = new List<String>(cmd.Split('/'));
            commandName = path[path.Count - 1];
            path.RemoveAt(path.Count - 1);
        }

        public void Init(XmlNode node) {
            string cmd = Element.GetNodeValue(node);

            readCommand(cmd);
        }

        public void Run() {
            CMainMenuGroupPopup currentMenu = null;

            foreach (string pathPart in path) {
                bool found = false;

                foreach (CMainMenuGroupPopup menu in new CMainMenuGroupPopupEnumerator()) {
                    CConsole.Write(String.Format("{0}: {1} <- {2}", menu.Name, menu.MyGuid, menu.Parent));

                    bool parentMatch = (currentMenu == null) || (menu.Parent == currentMenu.MyGuid);
                    if ((menu.Name.ToLowerInvariant() == pathPart.ToLowerInvariant()) && parentMatch) {
                        currentMenu = menu;
                        found = true;
                    }
                }

                if (!found)
                    throw new ArgumentException(String.Format("Path to command \"{0}\" is invalid.", originalCmd));
            }

            // now find the command
            foreach (CMainMenuCommands cmds in new CMainMenuCommandsEnumerator()) {
                if (cmds.Parent == currentMenu.MyGuid) {
                    for (uint i = 0; i < cmds.CommandCount; i++) {
                        if (cmds.GetName(i) == commandName) {
                            cmds.Execute(i);
                            return;

                        }
                    }

                }
            }

            throw new ArgumentException(String.Format("Path to command \"{0}\" is invalid.", originalCmd));
            
            
            
        }

    };


    [LayerTypeAttribute("button")]
    class ButtonLayer : Layer{
        // action register
        public static Dictionary<string, Type> Actions = new Dictionary<string,Type>();

        static ButtonLayer() {
            Actions.Add("menu", typeof(MainMenuAction));
        }

        

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

            if (mouseOn) {
                CManagedWrapper.DoMainMenuCommand(myAction);
                MainMenuAction x = new MainMenuAction("Playback/Order/Random");
                x.Run();
                
            }
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
