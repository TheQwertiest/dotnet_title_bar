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
using fooManagedWrapper;
using fooTitle.Geometries;
using System.Windows.Forms;


namespace fooTitle.Layers
{
	/// <summary>
	/// Loads itself from an xml file and handles all drawing.
	/// </summary>
	public class Skin : Layer,  IPlayCallbackSender
	{
		XmlDocument document = new XmlDocument();
        XmlNode skin;
        string path;
        public List<Layer> DynamicLayers = new List<Layer>();
        

        /// <summary>
        /// Returns the directory of the skin. Can be used for loading images and other data files.
        /// </summary>
        public string SkinDirectory {
            get {
                return path;
            }
        }

        private OnPlaybackNewTrackDelegate onNewTrackRegistered;
        private OnPlaybackTimeDelegate onTimeRegistered;
        private OnPlaybackPauseDelegate onPauseRegistered;
        private OnPlaybackStopDelegate onStopRegistered;

		/// <summary>
		/// Loads the skin from the specified xml file
		/// </summary>
		/// <param name="path">Path to the skin directory.</param>
		public Skin(string _path) : base()
		{
            path = _path;

			// load the skin xml file
			document.Load(GetSkinFilePath("skin.xml"));

			// read the xml document for the basic properties
			skin = document.GetElementsByTagName("skin").Item(0);

            int width = Int32.Parse(skin.Attributes.GetNamedItem("width").Value);
            int height = Int32.Parse(skin.Attributes.GetNamedItem("height").Value);
            geometry = new AbsoluteGeometry(new Rectangle(0, 0, width, height), width, height, new Point(0,0), AlignType.Left);
			
            // register to main for playback events
            onNewTrackRegistered = new OnPlaybackNewTrackDelegate(OnPlaybackNewTrack);
            onTimeRegistered = new OnPlaybackTimeDelegate(OnPlaybackTime);
            onStopRegistered = new OnPlaybackStopDelegate(OnPlaybackStop);
            onPauseRegistered = new OnPlaybackPauseDelegate(OnPlaybackPause);
            Main.GetInstance().OnPlaybackNewTrackEvent += onNewTrackRegistered;
            Main.GetInstance().OnPlaybackTimeEvent += onTimeRegistered;
            Main.GetInstance().OnPlaybackPauseEvent += onPauseRegistered;
            Main.GetInstance().OnPlaybackStopEvent += onStopRegistered;
		}

        /// <summary>
        /// Call to free this skin (unregistering events, unregistering layer events,...)
        /// </summary>
        public void Free() {
            Main.GetInstance().OnPlaybackNewTrackEvent -= onNewTrackRegistered;
            Main.GetInstance().OnPlaybackTimeEvent -= onTimeRegistered;
            Main.GetInstance().OnPlaybackStopEvent -= onStopRegistered;
            Main.GetInstance().OnPlaybackPauseEvent -= onPauseRegistered;

            display.MouseUp -= mouseUpReg;
            display.MouseDown -= mouseDownReg;
            display.MouseMove -= mouseMoveReg;
        }

		/// <summary>
		/// Resizes the skin and all layers, but not the form.
		/// </summary>
		/// <param name="newSize">The new size the skin should have</param>
		public void Resize(Size newSize) {
			((AbsoluteGeometry)geometry).Width = newSize.Width;
            ((AbsoluteGeometry)geometry).Height = newSize.Height;
			foreach (Layer l in layers ) {
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
        public event MouseEventHandler OnMouseUp;
        public event EventHandler OnMouseLeave;

        protected MouseEventHandler mouseMoveReg, mouseDownReg, mouseUpReg;
        protected EventHandler mouseLeaveReg;

        public void Init(Display _display) {
            display = _display;

            // register to mouse events
            mouseMoveReg = new MouseEventHandler(display_MouseMove);
            display.MouseMove += mouseMoveReg;
            mouseDownReg = new MouseEventHandler(display_MouseDown);
            display.MouseDown += mouseDownReg;
            mouseUpReg = new MouseEventHandler(display_MouseUp);
            display.MouseUp += mouseUpReg;
            mouseLeaveReg = new EventHandler(display_MouseLeave);
            display.MouseLeave += mouseLeaveReg;

            loadLayers(skin);
            geometry.Update(new Rectangle(0, 0, ((AbsoluteGeometry)geometry).Width, ((AbsoluteGeometry)geometry).Height));
        }
        
        void display_MouseUp(object sender, MouseEventArgs e) {
            sendEventCatch(OnMouseUp, sender, e);
        }

        void display_MouseDown(object sender, MouseEventArgs e) {
            sendEventCatch(OnMouseDown, sender, e);
        }

        void display_MouseMove(object sender, MouseEventArgs e) {
            sendEventCatch(OnMouseMove, sender, e);
        }

        void display_MouseLeave(object sender, EventArgs e) {
            sendEventCatch(OnMouseLeave, sender, e);
        }


    }
}
