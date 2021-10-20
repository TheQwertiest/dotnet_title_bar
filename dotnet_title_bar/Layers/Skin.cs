using fooTitle.Geometries;
using Qwr.ComponentInterface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace fooTitle.Layers
{
    public class SkinInfo
    {
        public string Author;
        public string Name;
    }

    /// <summary>
    ///     Loads itself from an xml file and handles all drawing.
    /// </summary>
    public class Skin : Layer, ICallbackSender, IDisposable
    {
        private readonly XElement _skinXml;
        private readonly string _skinDirectory;
        private readonly SkinForm _skinForm;
        private readonly ToolTipForm _tooltipForm;
        public List<Layer> DynamicLayers = new();

        /// <summary>
        ///     Loads the skin from the specified xml file
        /// </summary>
        public Skin(string skinDirectory, SkinForm skinForm, ToolTipForm tooltipForm)
        {
            _skinForm = skinForm;
            _tooltipForm = tooltipForm;
            _skinDirectory = skinDirectory;
            if (!Directory.Exists(_skinDirectory))
            {
                throw new Exception($"Can't find the skin in the following path: {_skinDirectory}");
            }

            // load the skin xml file
            XDocument document = XDocument.Load(GetSkinFilePath("skin.xml"));

            // read the xml document for the basic properties
            _skinXml = document.Elements("skin").First();

            int width = int.Parse(_skinXml.Attribute("width").Value);
            int height = int.Parse(_skinXml.Attribute("height").Value);
            ContainedGeometry = new AbsoluteGeometry(new Rectangle(0, 0, width, height), width, height, new Point(0, 0));

            // register to main for playback events
            Main.Get().PlaybackAdvancedToNewTrack += OnPlaybackNewTrack;
            Main.Get().TrackPlaybackPositionChanged += OnPlaybackTime;
            Main.Get().PlaybackPausedStateChanged += OnPlaybackPause;
            Main.Get().PlaybackStopped += OnPlaybackStop;


            // register to mouse events
            skinForm.MouseMove += Display_MouseMove;
            skinForm.MouseDown += Display_MouseDown;
            skinForm.MouseUp += Display_MouseUp;
            skinForm.MouseLeave += Display_MouseLeave;
            skinForm.MouseWheel += Display_MouseWheel;
            skinForm.MouseDoubleClick += Display_MouseDoubleClick;

            // Load skin data
            InitializeAnchor();
            LoadLayers(_skinXml);

            if (!SkinState.IsStateValid(this))
            {
                SkinState.ResetState();
                SkinState.SaveState(this);
            }
            else
            {
                SkinState.LoadState(this);
            }

            if (HasToolTipLayer(this))
            {
                ToolTip = new ToolTip(_tooltipForm, _skinForm, this);
            }

            ContainedGeometry.Update(new Rectangle(0, 0, ((AbsoluteGeometry)ContainedGeometry).Width, ((AbsoluteGeometry)ContainedGeometry).Height));
        }

        public void Dispose()
        {
            Main.Get().PlaybackAdvancedToNewTrack -= OnPlaybackNewTrack;
            Main.Get().TrackPlaybackPositionChanged -= OnPlaybackTime;
            Main.Get().PlaybackStopped -= OnPlaybackStop;
            Main.Get().PlaybackPausedStateChanged -= OnPlaybackPause;

            _skinForm.MouseMove -= Display_MouseMove;
            _skinForm.MouseDown -= Display_MouseDown;
            _skinForm.MouseUp -= Display_MouseUp;
            _skinForm.MouseLeave -= Display_MouseLeave;
            _skinForm.MouseWheel -= Display_MouseWheel;
            _skinForm.MouseDoubleClick -= Display_MouseDoubleClick;

            ToolTip?.Dispose();
            ToolTip = null;
        }

        public ToolTip ToolTip { get; private set; }

        public static SkinInfo GetSkinInfo(string skinDirectory)
        {
            string skinFullPath = Path.Combine(skinDirectory, "skin.xml");
            if (!File.Exists(skinFullPath))
            {
                return null;
            }

            try
            {
                XDocument document = XDocument.Load(skinFullPath);
                XElement skinXml = document.Elements("skin").First();

                return new SkinInfo
                {
                    Name = GetAttributeValue(skinXml, "name", null),
                    Author = GetAttributeValue(skinXml, "author", null)
                };
            }
            catch (XmlException e)
            {
                Console.Get().LogError($"Failed to parse skin from {skinFullPath}:\n\n"
                                       + $"{e}");
                return null;
            }
        }

        /// <summary>
        ///     Resizes the skin and all layers, but not the form.
        /// </summary>
        /// <param name="newSize">The new size the skin should have</param>
        public void Resize(Size newSize)
        {
            ((AbsoluteGeometry)ContainedGeometry).Width = newSize.Width;
            ((AbsoluteGeometry)ContainedGeometry).Height = newSize.Height;
            foreach (Layer l in ContainedLayers)
            {
                l.UpdateGeometry(ClientRect);
            }
        }

        /// <summary>
        ///     Asks layers for optimal size and resizes itself and the display in case it's needed.
        /// </summary>
        public void CheckSize()
        {
            Size size = GetMinimalSize();
            if (size.Width != ((AbsoluteGeometry)ContainedGeometry).Width || size.Height != ((AbsoluteGeometry)ContainedGeometry).Height)
            {
                Resize(size);
                _skinForm.SetSize(ClientRect.Width, ClientRect.Height);
                Main.Get().RedrawTitleBar(true);
            }
        }

        /// <summary>
        ///     Asks layers for optimal size and resizes itself and the display in case it's needed.
        ///     Called when the skin is loaded - does full size checking
        /// </summary>
        public void FirstCheckSize()
        {
            Size size = GetMinimalSize();
            Resize(size);
            _skinForm.SetSize(ClientRect.Width, ClientRect.Height);
        }

        /// <summary>
        ///     Optimized version of updateGeometry - processes only the dynamic layers
        /// </summary>
        /// <param name="parentRect">parent rectangle - the geometry should fit in it</param>
        public void UpdateDynamicGeometry(Rectangle parentRect)
        {
            foreach (Layer l in DynamicLayers)
            {
                l.UpdateThisLayerGeometry(l.ParentLayer.ClientRect);
            }
        }

        /// <summary>
        ///     Creates a Bitmap from a file located in the skin folder. Use this to prevent the
        ///     file from being locked all the time.
        /// </summary>
        public Bitmap GetSkinImage(string fileName)
        {
            Image img = Image.FromFile(GetSkinFilePath(fileName));

            MemoryStream mstr = new MemoryStream();
            img.Save(mstr, img.RawFormat);

            Bitmap retImg = new Bitmap(mstr);

            img.Dispose();

            return retImg;
        }

        protected override Size GetMinimalSizeImpl()
        {
            // don't ask geometry..
            return GetContentSize();
        }

        /// <summary>
        ///     Used for easily finding out what is the path to a skin's file (image, xml, extension,...)
        /// </summary>
        /// <param name="fileName">Name of the file which is searched for.</param>
        /// <returns>Path composed of app's data directory and skin's directory.</returns>
        private string GetSkinFilePath(string fileName)
        {
            return Path.Combine(_skinDirectory, fileName);
        }

        private void InitializeAnchor()
        {
            string anchorTypeStr = GetAttributeValue(_skinXml, "anchor", "top,left");
            float anchorDx = GetNumberFromAttribute(_skinXml, "anchor_dx", 0);
            float anchorDy = GetNumberFromAttribute(_skinXml, "anchor_dy", 0);

            DockAnchorType anchorType = DockAnchorType.None;
            foreach (string i in anchorTypeStr.ToLower().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                switch (i)
                {
                    case "top":
                        anchorType |= DockAnchorType.Top;
                        break;
                    case "bottom":
                        anchorType |= DockAnchorType.Bottom;
                        break;
                    case "right":
                        anchorType |= DockAnchorType.Right;
                        break;
                    case "left":
                        anchorType |= DockAnchorType.Left;
                        break;
                    case "center":
                        anchorType |= DockAnchorType.Center;
                        break;
                }
            }

            _skinForm.InitializeAnchor(anchorType, anchorDx, anchorDy);
        }

        private static bool HasToolTipLayer(Layer layer)
        {
            var result_layer = layer.SubLayers.FirstOrDefault(i => i.HasToolTip || HasToolTipLayer(i));
            return result_layer != null;
        }

        private static bool IsMouseOverButton(Layer layer)
        {
            var result_layer = layer.SubLayers.FirstOrDefault(i => i.Type == "button" && i.IsMouseOver || IsMouseOverButton(i));
            return result_layer != null;
        }

        /// <summary>
        ///     Does not check for exceptions
        /// </summary>
        /// <param name="eventObj">This must be a delegate</param>
        /// <param name="p">Parameters for the delegate</param>
        private static void SendEvent(object eventObj, params object[] p)
        {
            if (eventObj != null)
            {
                Delegate d = (Delegate)eventObj;
                d.DynamicInvoke(p);
            }
        }

        private static void SendEventCatch(object eventObj, params object[] p)
        {
            try
            {
                SendEvent(eventObj, p);
            }
            catch (Exception e)
            {
                Utils.ReportErrorWithPopup(e.ToString());
            }
        }

#pragma warning disable 0168, 219, 67
        public event TrackPlaybackPositionChanged_EventHandler TrackPlaybackPositionChanged;
        public event PlaybackAdvancedToNewTrack_EventHandler PlaybackAdvancedToNewTrack;
        public event Quit_EventHandler Quit;
        public event Initialized_EventHandler Initialized;
        public event PlaybackStoppedEventhandler PlaybackStopped;
        public event PlaybackPausedStateChanged_EventHandler PlaybackPausedStateChanged;
        public event DynamicTrackInfoChanged_EventHandler DynamicTrackInfoChanged;
#pragma warning restore 0168, 219, 67

        public event MouseEventHandler MouseMove;
        public event MouseEventHandler MouseDown;
        public event MouseEventHandler MouseDoubleClick;
        public event MouseEventHandler MouseUp;
        public event EventHandler MouseLeave;
        public event MouseEventHandler MouseWheel;

        private void OnPlaybackTime(double time)
        {
            SendEvent(TrackPlaybackPositionChanged, time);
        }

        public void OnPlaybackNewTrack(IMetadbHandle song)
        {
            SendEvent(PlaybackAdvancedToNewTrack, song);
            CheckSize();
        }

        public void OnPlaybackStop(PlaybackStopReason reason)
        {
            SendEvent(PlaybackStopped, reason);
            if (reason != PlaybackStopReason.StartingAnother)
            {
                CheckSize();
            }
        }

        public void OnPlaybackPause(bool state)
        {
            SendEvent(PlaybackPausedStateChanged, state);
        }

        private void Display_MouseUp(object sender, MouseEventArgs e)
        {
            SendEventCatch(MouseUp, sender, e);
            ToolTip?.OnMouseUp(sender, e);
        }

        private void Display_MouseDown(object sender, MouseEventArgs e)
        {
            SendEventCatch(MouseDown, sender, e);
            ToolTip?.OnMouseDown(sender, e);
        }

        private void Display_MouseMove(object sender, MouseEventArgs e)
        {
            SendEventCatch(MouseMove, sender, e);
            ToolTip?.OnMouseMove(sender, e);
            Main.Get().CanDragDisplay = !IsMouseOverButton(this);
        }

        private void Display_MouseLeave(object sender, EventArgs e)
        {
            Main.Get().CanDragDisplay = true;
            SendEventCatch(MouseLeave, sender, e);
            ToolTip?.ClearToolTip();
        }

        private void Display_MouseWheel(object sender, EventArgs e)
        {
            SendEventCatch(MouseWheel, sender, e);
        }

        private void Display_MouseDoubleClick(object sender, EventArgs e)
        {
            SendEventCatch(MouseDoubleClick, sender, e);
        }
    }
}
