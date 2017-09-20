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
using System.IO;
using System.Reflection;
using fooManagedWrapper;
using fooTitle.Layers;
using fooTitle.Geometries;
using fooTitle.Config;


namespace fooTitle {
 
    public enum EnableDragging {
        Always,
        WhenPropertiesOpen,
        Never,
    }

    public class Main : IComponentClient, IPlayCallbackSender {
        public static IPlayControl PlayControl;
        /// <summary>
        /// Set to true after everything has been created and can be manipulated
        /// </summary>
        private bool initDone = false;

        private static readonly ConfString myDataDir = new ConfString("base/dataDir", "foo_title\\");

        /// <summary>
        /// Returns the foo_title data directory located in the foobar2000 user directory (documents and settings)
        /// </summary>
        public static string UserDataDir => Path.Combine(CManagedWrapper.getInstance().GetProfilePath(), myDataDir.Value);

        /// <summary>
        /// How often the display should be redrawn
        /// </summary>
        private readonly ConfInt _updateInterval = new ConfInt("display/refreshRate", 30, 1, 250);
        protected void UpdateInterval_OnChanged(string name) {
            if (initDone)
                _redrawTimer.Interval = 1000/_updateInterval.Value;
        }

        /// <summary>
        /// List of layers, that need continuous redrawing (e.g. animation, scrolling text)
        /// </summary>
        private readonly HashSet<IContiniousRedraw> _redrawRequesters = new HashSet<IContiniousRedraw>();

        private bool _needToRedraw = false;

        public void RequestRedraw(bool force = false)
        {
            if (Display == null || !Display.Visible)
            {
                return;
            }

            if (force )
            {
                Display.Invalidate();
            }
            else
            {
                _needToRedraw = true;
            }
        }

        /// <summary>
        /// The name of the currently used skin. Can be changed
        /// </summary>
        public ConfString SkinPath = new ConfString("base/skinName", null);

        protected void SkinPath_OnChanged(string name) {
            if (initDone) {
                try {
                    LoadSkin(SkinPath.Value);
                    // Changing to skin with different anchor type 
                    // may cause window to go beyond screen borders
                    Display.ReadjustPosition();
                    SavePosition();
                } catch (Exception e) {
                    CurrentSkin = null;
                    System.Windows.Forms.MessageBox.Show($"foo_title - There was an error loading skin { SkinPath.Value}:\n {e.Message} \n {e}", "foo_title");
                }
            }
        }

        /// <summary>
        /// Provides access to the current skin. May be null.
        /// </summary>
        public Skin CurrentSkin { get; private set; }

        /// <summary>
        /// Provides access to the layer factory for creating new layers.
        /// </summary>
        public LayerFactory LayerFactory { get; private set; }

        /// <summary>
        /// Provides access to the geometry factory for creating new geometries.
        /// </summary>
        public GeometryFactory GeometryFactory { get; private set; }

        private Display _display;
        /// <summary>
        /// Provides access to the display.
        /// </summary>
        public Display Display {
            get {
                if ((_display != null) && _display.IsDisposed) {
                    _redrawTimer.Stop();
                    UnloadSkin();
                    _display = null;
                }

                return _display;
            }
        }

        public ToolTipDisplay ttd;

        /// <summary>
        /// Automatically handles reshowing foo_title if it's supposed to be always on top.
        /// </summary>
        protected RepeatedShowing repeatedShowing;

        private readonly ConfEnum<EnableDragging> DraggingEnabled = new ConfEnum<EnableDragging>("display/enableDragging", EnableDragging.Always);
        private bool _isMouseOnDragLayer = true;
        public bool CanDragDisplay {
            get {
                switch (DraggingEnabled.Value) {
                    case EnableDragging.Always:
                        return _isMouseOnDragLayer;
                    case EnableDragging.WhenPropertiesOpen:
                        return Properties.IsOpen;
                    case EnableDragging.Never:
                        return false;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            set
            {
                _isMouseOnDragLayer = value;
            }
        }

        private bool _fooTitleEnabled = true;

        public void EnableFooTitle()
        {
            if (!initDone || (_fooTitleEnabled && Display.Visible))
                return;

            _fooTitleEnabled = true;
            Display.Show();
            RequestRedraw(true);
        }

        public void DisableFooTitle()
        {
            if (!initDone || !_fooTitleEnabled && !Display.Visible)
                return;

            Display.Hide();
            _fooTitleEnabled = false;
        }
        
        public void StartDisplayAnimation(AnimationManager.Animation animationName, AnimationManager.OnAnimationStopDelegate onStopCallback = null)
        {
            if (initDone && Display.Visible)
            {
                Display.AnimManager.StartAnimation(animationName, onStopCallback);
            }
        }

        public void Hotkey_PopupToggle()
        {
            showControl.TogglePopup();
        }

        public void Hotkey_PopupPeek()
        {
            showControl.TriggerPopup();
        }

        private readonly ConfInt positionX = new ConfInt("display/positionX", 0);
        private readonly ConfInt positionY = new ConfInt("display/positionY", 0);
        public void positionX_OnChanged(string name) {
            Display.SetAnchorPosition(positionX.Value, positionY.Value);
        }
        public void positionY_OnChanged(string name) {
            Display.SetAnchorPosition(positionX.Value, positionY.Value);
        }

        private readonly ConfBool edgeSnap = new ConfBool("display/edgeSnap", true);
        public bool edgeSnapEnabled => edgeSnap.Value;

        private readonly ConfInt artLoadEvery = new ConfInt("display/artLoadEvery", 10, 1, int.MaxValue);
        private readonly ConfInt artLoadMaxTimes = new ConfInt("display/artLoadMaxTimes", 2, -1, int.MaxValue);
        public int ArtReloadFreq => artLoadEvery.Value;

        public int ArtReloadMax => artLoadMaxTimes.Value;

        private System.Windows.Forms.Timer _redrawTimer;

        private CMetaDBHandle lastSong;
        private Properties propsForm;

        // singleton
        private static Main instance;
        public static Main GetInstance() {
            return instance;
        }

        private void _redrawTimer_Tick(object sender, System.EventArgs e)
        {
            if (_needToRedraw )
            {
                _needToRedraw = false;
                Display.Invalidate();
            }
            else if (_redrawRequesters.Count > 0)
            {
                foreach (var i in _redrawRequesters)
                {
                    if (i.IsRedrawNeeded())
                    {
                        Display.Invalidate();
                        break;
                    }
                }
            }
        }

        public void DrawForm()
        {
            if (_fooTitleEnabled && CurrentSkin != null)
            {
                // need to update all values that are calculated from formatting strings
                //CurrentSkin.UpdateGeometry(CurrentSkin.ClientRect);
                CurrentSkin.UpdateDynamicGeometry(CurrentSkin.ClientRect);
                CurrentSkin.CheckSize();
                CurrentSkin.Draw();
            }
        }

        public void AddRedrawRequester(IContiniousRedraw requester)
        {
            if (!_redrawRequesters.Contains(requester))
            {
                _redrawRequesters.Add(requester);
            }
        }

        public void RemoveRedrawRequester(IContiniousRedraw requester)
        {
            if (_redrawRequesters.Contains(requester))
            {
                _redrawRequesters.Remove(requester);
            }
        }

        /// <summary>
        /// When the Display window is closed for some reason and we want to
        /// show it again, it must be re-created and reinitialized.
        /// </summary>
        private void reinitDisplay() {
            // initialize the form displaying the images
            _display = new Display(300, 22);
            _display.Closing -= myDisplay_Closing;
            _display.Closing += myDisplay_Closing;
            _display.Show();
            _redrawTimer.Start();
        }

        void myDisplay_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            _display.Closing -= myDisplay_Closing;
            _redrawTimer.Stop();
            UnloadSkin();
            _display = null;
        }

        /// <summary>
        /// Loads skin by from the given path. If the path is not absolute,
        /// the application directory is used to load the skin from.
        /// </summary>
        /// <param name="path">The name of the skin's directory</param>
        private void LoadSkin(string path) {
            try
            {
                // delete the old one
                UnloadSkin();

                if (this.Display == null)
                {
                    reinitDisplay();
                }

                if (path == null)
                {
                    path = Path.Combine(UserDataDir, "white");
                }

                if (!Directory.Exists(path))
                {
                    return;
                }

                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

                CurrentSkin = new Skin(path);
                CurrentSkin.Init(Display);

                RestorePosition();

                // need to tell it about the currently playing song
                if (lastSong != null)
                    CurrentSkin.OnPlaybackNewTrack(lastSong);
                else
                    CurrentSkin.OnPlaybackStop(IPlayControl.StopReason.stop_reason_user);

                CurrentSkin.FirstCheckSize();

                CConsole.Write($"skin loaded in {(int)sw.Elapsed.TotalMilliseconds} ms");

                RequestRedraw(true);
            } catch (Exception e) {
                CurrentSkin?.Free();
                CurrentSkin = null;
                System.Windows.Forms.MessageBox.Show(
                    $"foo_title - There was an error loading skin {path}:\n {e.Message} \n {e.ToString()}"
                    , "foo_title");
            }
        }

        private void UnloadSkin()
        {
            _redrawRequesters.Clear();
            CurrentSkin?.Free();
            CurrentSkin = null;
        }

        protected ViewMenuCommands viewMenuCommands;
        protected ShowControl showControl;

        public IConfigStorage Config;
        public IConfigStorage TestConfig;

        public Tests.TestServices TestServicesInstance;

        public void Create() {
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            instance = this;

            // create the property sheet form
            propsForm = new Properties(this);

            // create the services for testing
            TestServicesInstance = new Tests.TestServices();

            // create a notifying string value for saving the configuration
            CNotifyingCfgString cfgEntry = new CNotifyingCfgString(new Guid(457, 784, 488, 36, 48, 79, 54, 12, 36, 47, 13), "<config/>");

            // create the configuration manager
            Config = new XmlConfigStorage(cfgEntry);
            TestConfig = new XmlConfigStorage(TestServicesInstance.testConfigStorage);

            // initialize show control
            showControl = new ShowControl();

            // initialize menu commands
            viewMenuCommands = new ViewMenuCommands();
        }

        public void SavePosition() {
            Win32.Point anchorPos = Display.GetAnchorPosition();
            positionX.Value = anchorPos.x;
            positionY.Value = anchorPos.y;

            if (DraggingEnabled.Value == EnableDragging.Always && !Properties.IsOpen) {
                ConfValuesManager.GetInstance().SaveTo(Main.GetInstance().Config);
            }
        }

        public void RestorePosition() {
            Display.SetAnchorPosition(positionX.Value, positionY.Value);            
        }

        private void CreateDefaultDir()
        {
            try { 
                // create foo_title folder (does nothing if exists)
                Directory.CreateDirectory(UserDataDir);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"foo_title - Failed to create default directory.\nPath:\n{UserDataDir}\nError message:\n{e.Message}\nError details:\n{e}"
                    , "foo_title");
            }
        }

        public string GetName()
        {
            return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
        }
        public string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public string GetDescription()
        {
            return "foo_title\n" +
                   "Displays a title-bar (like Winamp's WindowShade mode)\n\n" +
                   "Copyright( c ) 2005-2006 by Roman Plasil\n\n" +
                   "Updated by Miha Lepej\n" +
                   "https://github.com/LepkoQQ/foo_title \n\n" +
                   "Further updated by TheQwertiest\n" +
                   "https://github.com/TheQwertiest/foo_title";
        }

        #region Events
        public event OnInitDelegate OnInitEvent;

        /// <summary>
        /// Called by init_quit, creates form, loads skin,...
        /// </summary>
        public void OnInit(IPlayControl a) {
            Main.PlayControl = a;
#if false
            // run the tests
            Tests.TestFramework t = new Tests.test_all();
            t.Run();
            t.ReportGUI();
#endif
            CreateDefaultDir();

            Config.Load();
            ConfValuesManager.GetInstance().LoadFrom(Config);
            ConfValuesManager.GetInstance().SetStorage(Config);

            // init registered clients
            OnInitEvent?.Invoke();

            // start a timer updating the display
            _redrawTimer = new System.Windows.Forms.Timer {Interval = 1000/_updateInterval.Value };
            _redrawTimer.Tick += _redrawTimer_Tick;            

            // create layer factory
            LayerFactory = new LayerFactory();
            LayerFactory.SearchAssembly(System.Reflection.Assembly.GetExecutingAssembly());

            // create geometry factory
            GeometryFactory = new GeometryFactory();
            GeometryFactory.SearchAssembly(System.Reflection.Assembly.GetExecutingAssembly());

            // initialize the display and skin
            reinitDisplay();
            LoadSkin(SkinPath.Value);

            ttd = new ToolTipDisplay();

            // register response events on some variables
            _updateInterval.OnChanged += UpdateInterval_OnChanged;
            SkinPath.OnChanged += SkinPath_OnChanged;
            positionX.OnChanged += positionX_OnChanged;
            positionY.OnChanged += positionY_OnChanged;

            // init reshower
            repeatedShowing = new RepeatedShowing();

            initDone = true;
        }

        public event OnQuitDelegate OnQuitEvent;

        public void OnQuit() {
            OnQuitEvent?.Invoke();
            Display?.Hide();
            ttd?.Hide();
        }

        public event OnPlaybackNewTrackDelegate OnPlaybackNewTrackEvent;

        public void OnPlaybackNewTrack(fooManagedWrapper.CMetaDBHandle song) {
            lastSong = song;
            SendEvent(OnPlaybackNewTrackEvent, song);
        }

        public event OnPlaybackTimeDelegate OnPlaybackTimeEvent;

        public void OnPlaybackTime(double time) {
            SendEvent(OnPlaybackTimeEvent, time);
        }

        public event OnPlaybackStopDelegate OnPlaybackStopEvent;
        public event OnPlaybackPauseDelegate OnPlaybackPauseEvent;
        public void OnPlaybackStop(IPlayControl.StopReason reason) {
            if (reason != IPlayControl.StopReason.stop_reason_starting_another)
                lastSong = null;
            SendEvent(OnPlaybackStopEvent, reason);
        }

        protected void SendEvent(object _event, params object[] p) {
            try {
                if (_event != null) {
                    System.Delegate d = (System.Delegate)_event;
                    d.DynamicInvoke(p);
                }
            } catch (Exception e) {
                fooManagedWrapper.CConsole.Error(e.ToString());
            }
        }

        public void OnPlaybackPause(bool state) {
            SendEvent(OnPlaybackPauseEvent, state);
        }

        public event OnPlaybackDynamicInfoTrackDelegate OnPlaybackDynamicInfoTrackEvent;

        public void OnPlaybackDynamicInfoTrack(fooManagedWrapper.FileInfo fileInfo) {
            SendEvent(OnPlaybackDynamicInfoTrackEvent, fileInfo);
        }

        #endregion
    }
}

