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
    internal enum MouseActionType
    {
        Click,
        DoubleClick,
        Wheel,
    }

    internal interface IButtonAction {
        void Init(XmlNode node);
        void Run(MouseButtons button, int clicks, int delta);
        MouseActionType GetMouseActionType();
    };

    internal enum ScrollDirection {
        Up,
        Down,
        None,
    }

    internal abstract class ButtonAction : IButtonAction {
        protected MouseButtons button;
        protected int clicks;
        protected ScrollDirection scrollDir;

        private static MouseButtons stringToButton(string b) {
            switch (b) {
                case "left":
                case "left_doubleclick":
                    return MouseButtons.Left;
                case "right":
                case "right_doubleclick":
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
                    return ScrollDirection.Up;
                case "down":
                    return ScrollDirection.Down;
                case "none":
                default:
                    return ScrollDirection.None;
            }
        }

        public virtual void Init(XmlNode node)
        {
            string buttonStr = Element.GetAttributeValue(node, "button", "left").ToLowerInvariant();
            button = stringToButton(buttonStr);
            clicks = "left_doubleclick" == buttonStr ? 2 : 1;
            scrollDir = stringToDir(Element.GetAttributeValue(node, "scroll", "none").ToLowerInvariant());

            if (button != MouseButtons.None && scrollDir != ScrollDirection.None) {
                throw new ArgumentException("You can't specify both 'button' and 'scroll' attributes on an action tag!");
            }
        }

        public abstract void Run(MouseButtons button, int clicks, int delta);

        public virtual MouseActionType GetMouseActionType()
        {
            if (clicks == 2)
            {
                return MouseActionType.DoubleClick;
            }

            if (scrollDir != ScrollDirection.None)
            {
                return MouseActionType.Wheel;
            }

            return MouseActionType.Click;
        }

        protected bool shouldRun(MouseButtons button, int clicks, int delta) {
            if (this.scrollDir != ScrollDirection.None) {
                return (scrollDir == ScrollDirection.Up && delta > 0) || (scrollDir == ScrollDirection.Down && delta < 0);
            }
            return this.button == MouseButtons.None || ( this.button == button && this.clicks == clicks );
        }
    }

    internal class MainMenuAction : ButtonAction {
        private string originalCmd;
        private CMainMenuCommands cmds;
        private uint commandIndex;

        private void readCommand(string cmd) {
            originalCmd = cmd;

            if (!MainMenuUtils.FindCommandByPath(cmd, out cmds, out commandIndex))
                throw new ArgumentException($"Command {cmd} not found.");
        }

        public override void Init(XmlNode node) {
            base.Init(node);
            string cmd = Element.GetNodeValue(node);
            readCommand(cmd);
        }

        public override void Run(MouseButtons button, int clicks, int delta) {
            if (!shouldRun(button, clicks, delta)) {
                return;
            }
            cmds.Execute(commandIndex);
        }
    };

    internal class ContextMenuAction : ButtonAction {
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

        public override void Run(MouseButtons button, int clicks, int delta) {
            if (!shouldRun(button, clicks, delta)) {
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
                CConsole.Warning($"Contextmenu command {cmdPath} not found.");
            }

            if (dynamic) {
                cmds.Execute(index, commandGuid, context);
            } else {
                cmds.Execute(index, context);
            }
        }
    }

    internal class LegacyMainMenuCommand : ButtonAction {
        private string commandName;

        public override void Init(XmlNode node) {
            base.Init(node);
            commandName = Element.GetNodeValue(node);
        }

        public override void Run(MouseButtons button, int clicks, int delta) {
            if (!shouldRun(button, clicks, delta)) {
                return;
            }
            CManagedWrapper.DoMainMenuCommand(commandName);
        }
    };

    internal class ToggleAction : ButtonAction {
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
            switch (_only)
            {
                case "enable":
                    only = Kind.Enable;
                    break;
                case "toggle":
                    only = Kind.Toggle;
                    break;
                case "disable":
                    only = Kind.Disable;
                    break;
            }
        }

        public override void Run(MouseButtons button, int clicks, int delta) {
            if (!shouldRun(button, clicks, delta)) {
                return;
            }
            Layer root = LayerTools.FindLayerByName(Main.GetInstance().CurrentSkin, target);
            if (root == null) {
                CConsole.Write($"Enable action couldn't find layer {target}.");
                return;
            }

            bool enable = true;
            switch (only)
            {
                case Kind.Disable:
                    enable = false;
                    break;
                case Kind.Toggle:
                    enable = !root.Enabled;
                    break;
            }

            LayerTools.EnableLayer(root, enable);
        }
    };

    [LayerTypeAttribute("button")]
    internal class ButtonLayer : Layer {
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
            ReadActions(contents);

            XmlNode img = GetFirstChildByNameOrNull(contents, "normalImg");
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

            RegisterMouseEvents();
        }

        private void OnMouseLeave(object sender, EventArgs e) {
            mouseOn = false;
        }

        private void OnMouseUp(object sender, MouseEventArgs e) {         
            mouseDown = false;

            if (!Enabled || !mouseOn ) {
                return;
            }

            if (e.Clicks == (e.Clicks >> 1) << 1 )
            {// double clicks
                return;
            }

            // run all actions
            foreach (IButtonAction action in actions) {
                action.Run(e.Button, e.Clicks ,e.Delta);
            }
        }

        private void OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!Enabled || !mouseOn)
            {
                return;
            }

            // run all actions
            foreach (IButtonAction action in actions)
            {
                action.Run(e.Button, e.Clicks, e.Delta);
            }
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (!Enabled || !mouseOn)
            {
                return;
            }

            // run all actions
            foreach (IButtonAction action in actions)
            {
                action.Run(e.Button, e.Clicks, e.Delta);
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            mouseOn = ClientRect.Contains(e.X, e.Y);
            if (!mouseOn)
                mouseDown = false;
        }

        private void OnMouseDown(object sender, MouseEventArgs e) {
            if (!mouseOn)
            {
                return;
            }

            mouseDown = true;
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

        private void ReadActions(XmlNode node) {
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
                        throw new ArgumentException($"No button action type {type} is registered.");
                    }

                    IButtonAction newAction = naid.ReflectionUtils.ConstructParameterless<IButtonAction>(actionClass);
                    newAction.Init(child);
                    actions.Add(newAction);
                }
            }
        }

        private void RegisterMouseEvents()
        {
            bool clickIsSet = false;
            bool doubleClickIsSet = false;
            bool wheelIsSet = false;
            foreach (IButtonAction child in actions)
            {
                switch (child.GetMouseActionType())
                {
                    case MouseActionType.Click:
                        clickIsSet = true;
                        break;
                    case MouseActionType.DoubleClick:
                        doubleClickIsSet = true;
                        break;
                    case MouseActionType.Wheel:
                        wheelIsSet = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            }

            Main.GetInstance().CurrentSkin.OnMouseMove += OnMouseMove;
            Main.GetInstance().CurrentSkin.OnMouseLeave += OnMouseLeave;

            if (clickIsSet)
            {
                Main.GetInstance().CurrentSkin.OnMouseDown += OnMouseDown;
                Main.GetInstance().CurrentSkin.OnMouseUp += OnMouseUp;
            }
            if (wheelIsSet)
            {
                Main.GetInstance().CurrentSkin.OnMouseWheel += OnMouseWheel;
            }
            if (doubleClickIsSet)
            {
                Main.GetInstance().CurrentSkin.OnMouseDoubleClick += OnMouseDoubleClick;
            }
        }

    }
}
