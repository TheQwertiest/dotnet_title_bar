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
using System.Xml;
using System.Drawing;
using System.Windows.Forms;

using fooManagedWrapper;
using fooTitle.Extending;


namespace fooTitle.Layers {

    interface IButtonAction {
        void Init(XmlNode node);
        void Run(MouseButtons button, int delta);
    };

    enum ScrollDirection {
        UP,
        DOWN,
        NONE,
    }

    abstract class ButtonAction : IButtonAction {
        protected MouseButtons button;
        protected ScrollDirection scrollDir;

        private static MouseButtons stringToButton(string b) {
            switch (b) {
                case "left":
                    return MouseButtons.Left;
                case "right":
                    return MouseButtons.Right;
                case "middle":
                    return MouseButtons.Middle;
                case "back":
                    return MouseButtons.XButton1;
                case "forward":
                    return MouseButtons.XButton2;
                case "all":
                default:
                    return MouseButtons.None;
            }
        }

        private static ScrollDirection stringToDir(string d) {
            switch (d) {
                case "up":
                    return ScrollDirection.UP;
                case "down":
                    return ScrollDirection.DOWN;
                case "none":
                default:
                    return ScrollDirection.NONE;
            }
        }

        public virtual void Init(XmlNode node) {
            button = stringToButton(Element.GetAttributeValue(node, "button", "all").ToLowerInvariant());
            scrollDir = stringToDir(Element.GetAttributeValue(node, "scroll", "none").ToLowerInvariant());
            if (button != MouseButtons.None && scrollDir != ScrollDirection.NONE) {
                throw new ArgumentException("You can't specify both 'button' and 'scroll' attributes on an action tag!");
            }
        }

        public abstract void Run(MouseButtons button, int delta);

        protected bool shouldRun(MouseButtons button, int delta) {
            if (this.scrollDir != ScrollDirection.NONE) {
                return (scrollDir == ScrollDirection.UP && delta > 0) || (scrollDir == ScrollDirection.DOWN && delta < 0);
            }
            if (this.button != MouseButtons.None && this.button != button) {
                return false;
            }
            return true;
        }
    }

    class MainMenuAction : ButtonAction {
        private string originalCmd;
        private CMainMenuCommands cmds;
        private uint commandIndex;

        private void readCommand(string cmd) {
            originalCmd = cmd;

            if (!MainMenuUtils.FindCommandByPath(cmd, out cmds, out commandIndex))
                throw new ArgumentException(String.Format("Command {0} not found.", cmd));
        }

        public override void Init(XmlNode node) {
            base.Init(node);
            string cmd = Element.GetNodeValue(node);
            readCommand(cmd);
        }

        public override void Run(MouseButtons button, int delta) {
            if (!shouldRun(button, delta)) {
                return;
            }
            cmds.Execute(commandIndex);
        }
    };

    class ContextMenuAction : ButtonAction {
        private Context context;
        private string cmdPath;

        public override void Init(XmlNode node) {
            base.Init(node);
            if (Element.GetAttributeValue(node, "context", "nowplaying").ToLowerInvariant() == "nowplaying") {
                context = Context.NowPlaying;
            } else {
                context = Context.Playlist;
            }

            cmdPath = Element.GetNodeValue(node);
        }

        public override void Run(MouseButtons button, int delta) {
            if (!shouldRun(button, delta)) {
                return;
            }
            if (string.IsNullOrEmpty(cmdPath)) {
                return;
            }

            Guid commandGuid;
            CContextMenuItem cmds;
            uint index;
            bool dynamic;

            if (!ContextMenuUtils.FindContextCommandByDefaultPath(cmdPath, context, out cmds, out commandGuid, out index, out dynamic)) {
                CConsole.Warning(String.Format("Contextmenu command {0} not found.", cmdPath));
            }

            if (dynamic) {
                cmds.Execute(index, commandGuid, context);
            } else {
                cmds.Execute(index, context);
            }
        }
    }

    class LegacyMainMenuCommand : ButtonAction {
        private string commandName;

        public override void Init(XmlNode node) {
            base.Init(node);
            commandName = Element.GetNodeValue(node);
        }

        public override void Run(MouseButtons button, int delta) {
            if (!shouldRun(button, delta)) {
                return;
            }
            CManagedWrapper.DoMainMenuCommand(commandName);
        }
    };

    class ToggleAction : ButtonAction {
        private string target;

        private enum Kind {
            Toggle,
            Enable,
            Disable
        }
        private Kind only;

        public override void Init(XmlNode node) {
            base.Init(node);
            target = Element.GetAttributeValue(node, "target", "");

            string _only = Element.GetAttributeValue(node, "only", "toggle").ToLowerInvariant();
            if (_only == "enable")
                only = Kind.Enable;
            else if (_only == "toggle")
                only = Kind.Toggle;
            else if (_only == "disable")
                only = Kind.Disable;
        }

        public override void Run(MouseButtons button, int delta) {
            if (!shouldRun(button, delta)) {
                return;
            }
            Layer root = LayerTools.FindLayerByName(Main.GetInstance().CurrentSkin, target);
            if (root == null) {
                CConsole.Write(string.Format("Enable action couldn't find layer {0}.", target));
                return;
            }

            bool enable = true;
            if (only == Kind.Disable) {
                enable = false;
            } else if (only == Kind.Toggle) {
                enable = !root.Enabled;
            }

            LayerTools.EnableLayer(root, enable);
        }
    };

    [LayerTypeAttribute("button")]
    class ButtonLayer : Layer {
        // action register
        public static Dictionary<string, Type> Actions = new Dictionary<string, Type>();

        static ButtonLayer() {
            Actions.Add("menu", typeof(MainMenuAction));
            Actions.Add("contextmenu", typeof(ContextMenuAction));
            Actions.Add("toggle", typeof(ToggleAction));
        }

        protected Bitmap myNormalImage;
        protected Bitmap myOverImage;
        protected Bitmap myDownImage;

        protected bool mouseOn;
        protected bool mouseDown;

        private ICollection<IButtonAction> actions;

        public ButtonLayer(Rectangle parentRect, XmlNode node) : base(parentRect, node) {
            XmlNode contents = GetFirstChildByName(node, "contents");
            readActions(contents);

            XmlNode img;
            img = GetFirstChildByNameOrNull(contents, "normalImg");
            if (img != null) {
                myNormalImage = Main.GetInstance().CurrentSkin.GetSkinImage(img.Attributes.GetNamedItem("src").Value);
            }

            img = GetFirstChildByNameOrNull(contents, "overImg");
            if (img != null) {
                myOverImage = Main.GetInstance().CurrentSkin.GetSkinImage(img.Attributes.GetNamedItem("src").Value);
            }

            img = GetFirstChildByNameOrNull(contents, "downImg");
            if (img != null) {
                myDownImage = Main.GetInstance().CurrentSkin.GetSkinImage(img.Attributes.GetNamedItem("src").Value);
            }

            // register mouse events
            Main.GetInstance().CurrentSkin.OnMouseMove += new MouseEventHandler(OnMouseMove);
            Main.GetInstance().CurrentSkin.OnMouseDown += new MouseEventHandler(OnMouseDown);
            Main.GetInstance().CurrentSkin.OnMouseUp += new MouseEventHandler(OnMouseUpOrWheel);
            Main.GetInstance().CurrentSkin.OnMouseLeave += new EventHandler(OnMouseLeave);
            Main.GetInstance().CurrentSkin.OnMouseWheel += new MouseEventHandler(OnMouseUpOrWheel);
        }

        void OnMouseLeave(object sender, EventArgs e) {
            mouseOn = false;
        }

        void OnMouseUpOrWheel(object sender, MouseEventArgs e) {
            if (e.Delta == 0) {
                mouseDown = false;
            }

            if (!Enabled) {
                return;
            }

            if (mouseOn) {
                // run all actions
                foreach (IButtonAction action in actions) {
                    action.Run(e.Button, e.Delta);
                }
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


        protected override void drawImpl() {
            Bitmap toDraw;
            if (mouseDown)
                toDraw = myDownImage;
            else if (mouseOn)
                toDraw = myOverImage;
            else
                toDraw = myNormalImage;

            if (toDraw != null) {
                Display.Canvas.DrawImage(toDraw, ClientRect.X, ClientRect.Y, ClientRect.Width, ClientRect.Height);
            }
        }

        private void readActions(XmlNode node) {
            actions = new List<IButtonAction>();

            foreach (XmlNode child in node.ChildNodes) {
                if (child.Name != "action")
                    continue;

                string type = GetAttributeValue(child, "type", null);
                if (type == null) {
                    IButtonAction newAction = new LegacyMainMenuCommand();
                    newAction.Init(child);
                    actions.Add(newAction);
                } else {
                    Type actionClass;
                    if (!Actions.TryGetValue(type, out actionClass)) {
                        throw new ArgumentException(String.Format("No button action type {0} is registered.", type));
                    } else {
                        IButtonAction newAction = naid.ReflectionUtils.ConstructParameterless<IButtonAction>(actionClass);
                        newAction.Init(child);
                        actions.Add(newAction);
                    }
                }
            }
        }
    }
}
