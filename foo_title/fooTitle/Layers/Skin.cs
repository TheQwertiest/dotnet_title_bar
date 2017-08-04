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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Drawing;
using System.Linq;
using fooManagedWrapper;
using fooTitle.Geometries;
using System.Windows.Forms;


namespace fooTitle.Layers {
    /// <summary>
    /// Loads itself from an xml file and handles all drawing.
    /// </summary>
    public class Skin : Layer, IPlayCallbackSender {
        private readonly XmlDocument document = new XmlDocument();
        private readonly XmlNode skin;
        public List<Layer> DynamicLayers = new List<Layer>();

        /// <summary>
        /// Returns the directory of the skin. Can be used for loading images and other data files.
        /// </summary>
        private string SkinDirectory { get; }

        public fooTitle.ToolTip ToolTip { get; private set; }

        /// <summary>
        /// Loads the skin from the specified xml file
        /// </summary>
        /// <param name="path">Path to the skin directory.</param>
        public Skin(string path)
        {
            SkinDirectory = path;

            // load the skin xml file
            document.Load(GetSkinFilePath("skin.xml"));

            // read the xml document for the basic properties
            skin = document.GetElementsByTagName("skin").Item(0);

            int width = int.Parse(skin.Attributes.GetNamedItem("width").Value);
            int height = int.Parse(skin.Attributes.GetNamedItem("height").Value);
            geometry = new AbsoluteGeometry(new Rectangle(0, 0, width, height), width, height, new Point(0, 0), AlignType.Left);

            // register to main for playback events
            Main.GetInstance().OnPlaybackNewTrackEvent += OnPlaybackNewTrack;
            Main.GetInstance().OnPlaybackTimeEvent += OnPlaybackTime;
            Main.GetInstance().OnPlaybackPauseEvent += OnPlaybackPause;
            Main.GetInstance().OnPlaybackStopEvent += OnPlaybackStop;
        }

        /// <summary>
        /// Call to free this skin (unregistering events, unregistering layer events,...)
        /// </summary>
        public void Free() {
            Main.GetInstance().OnPlaybackNewTrackEvent -= OnPlaybackNewTrack;
            Main.GetInstance().OnPlaybackTimeEvent -= OnPlaybackTime;
            Main.GetInstance().OnPlaybackStopEvent -= OnPlaybackStop;
            Main.GetInstance().OnPlaybackPauseEvent -= OnPlaybackPause;

            display.MouseMove -= display_MouseMove;
            display.MouseDown -= display_MouseDown;
            display.MouseUp -= display_MouseUp;
            display.MouseLeave -= display_MouseLeave;
            display.MouseWheel -= display_MouseWheel;
            display.MouseDoubleClick -= display_MouseDoubleClick;
        }

        /// <summary>
        /// Resizes the skin and all layers, but not the form.
        /// </summary>
        /// <param name="newSize">The new size the skin should have</param>
        public void Resize(Size newSize) {
            ((AbsoluteGeometry)geometry).Width = newSize.Width;
            ((AbsoluteGeometry)geometry).Height = newSize.Height;
            foreach (Layer l in layers) {
                l.UpdateGeometry(ClientRect);
            }
        }

        protected override Size getMinimalSizeImpl() {
            // don't ask geometry..
            return getContentSize();
        }

        /// <summary>
        /// Asks layers for optimal size and resizes itself and the display in case it's needed.
        /// </summary>
        public void CheckSize() {
            Size size = GetMinimalSize();
            if ((size.Width != ((AbsoluteGeometry)geometry).Width) || (size.Height != ((AbsoluteGeometry)geometry).Height)) {
                Resize(size);
                Main.GetInstance().Display.SetSize(ClientRect.Width, ClientRect.Height);
            }
        }

        /// <summary>
        /// Asks layers for optimal size and resizes itself and the display in case it's needed.
        /// Called when the skin is loaded - does full size checking
        /// </summary>
        public void FirstCheckSize() {
            Size size = GetMinimalSize();
            Resize(size);
            Main.GetInstance().Display.SetSize(ClientRect.Width, ClientRect.Height);
        }

        /// <summary>
        /// Optimised version of updateGeometry - processes only the dynamic layers
        /// </summary>
        /// <param name="parentRect">parent rectangle - the geometry should fit in it</param>
        public void FrameUpdateGeometry(Rectangle parentRect) {
            foreach (Layer l in DynamicLayers) {
                l.UpdateThisLayerGeometry(l.ParentLayer.ClientRect);
            }
        }

        /// <summary>
        /// Used for easily finding out what is the path to a skin's file (image, xml, extension,...)
        /// </summary>
        /// <param name="fileName">Name of the file which is searched for.</param>
        /// <returns>Path composed of app's data directory and skin's directory.</returns>
        public string GetSkinFilePath(string fileName) {
            return Path.Combine(SkinDirectory, fileName);
        }

        /// <summary>
        /// Creates a Bitmap from a file located in the skin folder. Use this to prevent the
        /// file from being locked all the time.
        /// </summary>
        public Bitmap GetSkinImage(string fileName) {
            using (FileStream stream = new FileStream(GetSkinFilePath(fileName), FileMode.Open, FileAccess.Read)) {
                Bitmap tmp = new Bitmap(stream);
                return tmp.Clone() as Bitmap;
            }
        }

        public class SkinInfo
        {
            public string Name;
            public string Author;
        };        
        public static SkinInfo GetSkinInfo(string skinPath)
        {
            string skinFullPath = Path.Combine(skinPath, "skin.xml");
            if (!File.Exists(skinFullPath))
            {
                return null;
            }

            XmlDocument document = new XmlDocument();
            document.Load(skinFullPath);            

            XmlNode skin = document.GetElementsByTagName("skin").Item(0);

            return new SkinInfo{
                Name = GetAttributeValue(skin, "name", null),
                Author = GetAttributeValue(skin, "author", null)
            };
        }

        public void OnPlaybackTime(double time) {
            // pass it on
            sendEvent(OnPlaybackTimeEvent, time);
        }

        public void OnPlaybackNewTrack(CMetaDBHandle song) {
            // pass it on
            sendEvent(OnPlaybackNewTrackEvent, song);
            CheckSize();
        }

        public void OnPlaybackStop(IPlayControl.StopReason reason) {
            // pass it on
            sendEvent(OnPlaybackStopEvent, reason);
            if (reason != IPlayControl.StopReason.stop_reason_starting_another)
                CheckSize();
        }

        public void OnPlaybackPause(bool state) {
            // pass it on
            sendEvent(OnPlaybackPauseEvent, state);
        }

        /// <summary>
        /// Does not check for exceptions
        /// </summary>
        /// <param name="_event">This must be a delegate</param>
        /// <param name="p">Parameters for the delegate</param>
        protected static void sendEvent(Object _event, params Object[] p) {
            if (_event != null) {
                System.Delegate d = (System.Delegate)_event;
                d.DynamicInvoke(p);
            }
        }

        protected static void sendEventCatch(Object _event, params Object[] p) {
            try {
                sendEvent(_event, p);
            } catch (Exception e) {
                MessageBox.Show(e.ToString(), "Error");
            }
        }

        #region IPlayCallbackSender Members
#pragma warning disable 0168, 219, 67
        public event OnPlaybackTimeDelegate OnPlaybackTimeEvent;
        public event OnPlaybackNewTrackDelegate OnPlaybackNewTrackEvent;
        public event OnQuitDelegate OnQuitEvent;
        public event OnInitDelegate OnInitEvent;
        public event OnPlaybackStopDelegate OnPlaybackStopEvent;
        public event OnPlaybackPauseDelegate OnPlaybackPauseEvent;
        public event OnPlaybackDynamicInfoTrackDelegate OnPlaybackDynamicInfoTrackEvent;
#pragma warning restore 0168, 219, 67
        #endregion

        public event MouseEventHandler OnMouseMove;
        public event MouseEventHandler OnMouseDown;
        public event MouseEventHandler OnMouseDoubleClick;
        public event MouseEventHandler OnMouseUp;
        public event EventHandler OnMouseLeave;
        public event MouseEventHandler OnMouseWheel;

        public void Init(Display display_) {
            display = display_;

            // register to mouse events
            display.MouseMove += display_MouseMove;
            display.MouseDown += display_MouseDown;
            display.MouseUp += display_MouseUp;
            display.MouseLeave += display_MouseLeave;
            display.MouseWheel += display_MouseWheel;
            display.MouseDoubleClick += display_MouseDoubleClick;

            InitAnchor();

            LoadLayers(skin);

            if (HasToolTipLayer(this))
            {
                ToolTip = new fooTitle.ToolTip(display_, this);                
            }

            geometry.Update(new Rectangle(0, 0, ((AbsoluteGeometry)geometry).Width, ((AbsoluteGeometry)geometry).Height));
        }

        private static bool HasToolTipLayer(Layer layer)
        {
            return layer.SubLayers.Any(i => i.HasToolTip || HasToolTipLayer(i));
        }

        private void InitAnchor()
        {
            string anchorTypeStr = GetAttributeValue(skin, "anchor", "top,left");
            float anchorDx = GetNumberFromAttribute(skin, "anchor_dx", "0");
            float anchorDy = GetNumberFromAttribute(skin, "anchor_dy", "0");

            DockAnchor.Type anchorType = DockAnchor.Type.None;
            foreach (string i in anchorTypeStr.ToLower().Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                switch (i)
                {
                    case "top":
                        anchorType |= DockAnchor.Type.Top;
                        break;
                    case "bottom":
                        anchorType |= DockAnchor.Type.Bottom;
                        break;
                    case "right":
                        anchorType |= DockAnchor.Type.Right;
                        break;
                    case "left":
                        anchorType |= DockAnchor.Type.Left;
                        break;
                    case "center":
                        anchorType |= DockAnchor.Type.Center;
                        break;
                }
            }

            display.InitializeAnchor(anchorType, anchorDx, anchorDy);
        }

        void display_MouseUp(object sender, MouseEventArgs e) {
            sendEventCatch(OnMouseUp, sender, e);
        }

        void display_MouseDown(object sender, MouseEventArgs e) {
            sendEventCatch(OnMouseDown, sender, e);
        }

        void display_MouseMove(object sender, MouseEventArgs e) {
            ToolTip?.OnMouseMove(sender, e);
            sendEventCatch(OnMouseMove, sender, e);
        }

        void display_MouseLeave(object sender, EventArgs e) {
            sendEventCatch(OnMouseLeave, sender, e);
        }

        void display_MouseWheel(object sender, EventArgs e) {
            sendEventCatch(OnMouseWheel, sender, e);
        }

        void display_MouseDoubleClick(object sender, EventArgs e)
        {
            sendEventCatch(OnMouseDoubleClick, sender, e);
        }

    }
}
