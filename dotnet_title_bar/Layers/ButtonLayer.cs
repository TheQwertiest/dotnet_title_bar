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
using fooTitle.Extending;
using Qwr.ComponentInterface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using System.Xml.Linq;

namespace fooTitle.Layers
{
    public enum MouseActionType
    {
        Click,
        DoubleClick,
        Wheel,
    }

    public interface IButtonAction
    {
        void Init(XElement node, Skin skin);
        void Run(MouseButtons button, int clicks, int delta);
        MouseActionType GetMouseActionType();
    };

    public enum ScrollDirection
    {
        Up,
        Down,
        None,
    }

    public abstract class ButtonAction : IButtonAction
    {
        protected Skin ParentSkin;
        protected MouseButtons button;
        protected int clicks;
        protected ScrollDirection scrollDir;

        private static MouseButtons StringToButton(string b)
        {
            return b switch
            {
                "left" or "left_doubleclick" => MouseButtons.Left,
                "right" or "right_doubleclick" => MouseButtons.Right,
                "middle" => MouseButtons.Middle,
                "back" => MouseButtons.XButton1,
                "forward" => MouseButtons.XButton2,
                _ => MouseButtons.None,
            };
        }

        private static ScrollDirection StringToDir(string d)
        {
            switch (d)
            {
                case "up":
                    return ScrollDirection.Up;
                case "down":
                    return ScrollDirection.Down;
                case "none":
                default:
                    return ScrollDirection.None;
            }
        }

        public virtual void Init(XElement node, Skin skin)
        {
            ParentSkin = skin;

            string buttonStr = Element.GetAttributeValue(node, "button", "left").ToLowerInvariant();
            button = StringToButton(buttonStr);
            clicks = "left_doubleclick" == buttonStr ? 2 : 1;
            scrollDir = StringToDir(Element.GetAttributeValue(node, "scroll", "none").ToLowerInvariant());

            if (button != MouseButtons.None && scrollDir != ScrollDirection.None)
            {
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

        protected bool ShouldRun(MouseButtons button, int clicks, int delta)
        {
            if (scrollDir != ScrollDirection.None)
            {
                return (scrollDir == ScrollDirection.Up && delta > 0) || (scrollDir == ScrollDirection.Down && delta < 0);
            }
            return this.button == MouseButtons.None || (this.button == button && this.clicks == clicks);
        }
    }

    public class MainMenuAction : ButtonAction
    {
        private string _originalCmd;

        public override void Init(XElement node, Skin skin)
        {
            base.Init(node, skin);
            _originalCmd = Element.GetNodeValue(node);
        }

        public override void Run(MouseButtons button, int clicks, int delta)
        {
            if (!ShouldRun(button, clicks, delta))
            {
                return;
            }

            Main.Get().Fb2kControls.ExecuteMainMenuCommand(_originalCmd);
        }
    };

    public class ContextMenuAction : ButtonAction
    {
        private bool _useNowPlaying;
        private string _cmdPath;

        public override void Init(XElement node, Skin skin)
        {
            base.Init(node, skin);
            _useNowPlaying = (Element.GetAttributeValue(node, "context", "nowplaying").ToLowerInvariant() == "nowplaying");
            _cmdPath = Element.GetNodeValue(node);
        }

        public override void Run(MouseButtons button, int clicks, int delta)
        {
            if (!ShouldRun(button, clicks, delta))
            {
                return;
            }
            if (string.IsNullOrEmpty(_cmdPath))
            {
                return;
            }

            // TODO: fix the old scenario - it used active item selection for context menu when _useNowPlaying is false
            try
            {
                Main.Get().Fb2kControls.ExecuteContextMenuCommand(_cmdPath);
            }
            catch (Exception e)
            {
                Console.Get().LogWarning($"Failed to execute command `{_cmdPath}`", e);
            }
        }
    }

    public class LegacyMainMenuCommand : ButtonAction
    {
        private string _commandName;

        public override void Init(XElement node, Skin skin)
        {
            base.Init(node, skin);
            _commandName = Element.GetNodeValue(node);
        }

        public override void Run(MouseButtons button, int clicks, int delta)
        {
            if (!ShouldRun(button, clicks, delta))
            {
                return;
            }
            Main.Get().Fb2kControls.ExecuteMainMenuCommand(_commandName);
        }
    };

    public class ToggleAction : ButtonAction
    {
        private Skin _parentSkin;
        private string _target;

        private enum Kind
        {
            Toggle,
            Enable,
            Disable
        }
        private Kind _only;

        public override void Init(XElement node, Skin skin)
        {
            base.Init(node, skin);

            _parentSkin = skin;
            _target = Element.GetAttributeValue(node, "target", "");

            string only = Element.GetAttributeValue(node, "only", "toggle").ToLowerInvariant();
            switch (only)
            {
                case "enable":
                    _only = Kind.Enable;
                    break;
                case "toggle":
                    _only = Kind.Toggle;
                    break;
                case "disable":
                    _only = Kind.Disable;
                    break;
            }
        }

        public override void Run(MouseButtons button, int clicks, int delta)
        {
            if (!ShouldRun(button, clicks, delta))
            {
                return;
            }
            Layer root = LayerTools.FindLayerByName(ParentSkin, _target);
            if (root == null)
            {
                Console.Get().LogWarning($"Enable action couldn't find layer {_target}.");
                return;
            }

            bool enable = true;
            switch (_only)
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

    [LayerType("button")]
    public class ButtonLayer : Layer
    {
        // action register
        public static Dictionary<string, Type> Actions = new Dictionary<string, Type>();

        static ButtonLayer()
        {
            Actions.Add("menu", typeof(MainMenuAction));
            Actions.Add("contextmenu", typeof(ContextMenuAction));
            Actions.Add("toggle", typeof(ToggleAction));
            Actions.Add("legacy", typeof(LegacyMainMenuCommand));
        }

        protected Bitmap myNormalImage;
        protected Bitmap myOverImage;
        protected Bitmap myDownImage;

        protected bool mouseOn;
        protected bool mouseDown;

        private ICollection<IButtonAction> _actions;

        public ButtonLayer(Rectangle parentRect, XElement node, Skin skin)
            : base(parentRect, node, skin)
        {
            XElement contents = GetFirstChildByName(node, "contents");
            ReadActions(contents);

            XElement img = GetFirstChildByNameOrNull(contents, "normalImg");
            if (img != null)
            {
                myNormalImage = ParentSkin.GetSkinImage(img.Attribute("src").Value);
            }

            img = GetFirstChildByNameOrNull(contents, "overImg");
            if (img != null)
            {
                myOverImage = ParentSkin.GetSkinImage(img.Attribute("src").Value);
            }

            img = GetFirstChildByNameOrNull(contents, "downImg");
            if (img != null)
            {
                myDownImage = ParentSkin.GetSkinImage(img.Attribute("src").Value);
            }

            RegisterMouseEvents();
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            mouseOn = false;
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (!Enabled || !mouseOn || !mouseDown)
            {
                return;
            }

            mouseDown = false;
            Main.Get().RedrawTitleBar();

            if (e.Clicks == (e.Clicks >> 1) << 1)
            { // double clicks
                return;
            }

            RunActions(sender, e);
            Main.Get().RedrawTitleBar(true);
        }

        private void OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!Enabled || !mouseOn)
            {
                return;
            }

            RunActions(sender, e);
            Main.Get().RedrawTitleBar(true);
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (!Enabled || !mouseOn)
            {
                return;
            }

            RunActions(sender, e);
            Main.Get().RedrawTitleBar(true);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            bool wasMouseOne = mouseOn;
            mouseOn = ClientRect.Contains(e.X, e.Y);
            if (!mouseOn)
            {
                mouseDown = false;
            }

            if (wasMouseOne != mouseOn)
            {
                Main.Get().RedrawTitleBar();
            }
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (!Enabled || !mouseOn)
            {
                return;
            }

            mouseDown = true;
            Main.Get().RedrawTitleBar();
        }

        private void RunActions(object sender, MouseEventArgs e)
        {
            bool hasToggle = false;
            foreach (IButtonAction action in _actions)
            {
                if (action.GetType() == typeof(ToggleAction))
                {
                    hasToggle = true;
                }

                action.Run(e.Button, e.Clicks, e.Delta);
            }

            if (hasToggle)
            {
                SkinState.SaveState(ParentSkin);
            }
        }

        protected override void DrawImpl(Graphics canvas)
        {
            Bitmap toDraw;
            if (mouseDown)
            {
                toDraw = myDownImage;
            }
            else if (mouseOn)
            {
                toDraw = myOverImage;
            }
            else
            {
                toDraw = myNormalImage;
            }

            if (toDraw != null)
            {
                canvas.DrawImage(toDraw, ClientRect.X, ClientRect.Y, ClientRect.Width, ClientRect.Height);
            }
        }

        // TODO: remove

        private static T ConstructParameterless<T>(Type type)
        {
            foreach (ConstructorInfo cons in type.GetConstructors())
            {
                if (cons.GetParameters().Length == 0)
                {
                    try
                    {
                        return (T)cons.Invoke(Array.Empty<object>());
                    }
                    catch (TargetInvocationException ex) when (ex.InnerException != null)
                    {
                        ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                        throw;
                    }
                }
            }

            throw new InvalidOperationException("No parameterless constructor found for class " + type.ToString());
        }

        private void ReadActions(XElement node)
        {
            _actions = new List<IButtonAction>();

            foreach (XElement child in node.Elements())
            {
                if (child.Name != "action")
                {
                    continue;
                }

                string type = GetAttributeValue(child, "type", "legacy");
                Type actionClass;
                if (!Actions.TryGetValue(type, out actionClass))
                {
                    throw new ArgumentException($"No button action type {type} is registered.");
                }

                IButtonAction newAction = ConstructParameterless<IButtonAction>(actionClass);
                newAction.Init(child, ParentSkin);
                _actions.Add(newAction);
            }
        }

        private void RegisterMouseEvents()
        {
            bool clickIsSet = false;
            bool doubleClickIsSet = false;
            bool wheelIsSet = false;
            foreach (IButtonAction child in _actions)
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

            ParentSkin.MouseMove += OnMouseMove;
            ParentSkin.MouseLeave += OnMouseLeave;

            if (clickIsSet)
            {
                ParentSkin.MouseDown += OnMouseDown;
                ParentSkin.MouseUp += OnMouseUp;
            }
            if (wheelIsSet)
            {
                ParentSkin.MouseWheel += OnMouseWheel;
            }
            if (doubleClickIsSet)
            {
                ParentSkin.MouseDoubleClick += OnMouseDoubleClick;
            }
        }
    }
}
