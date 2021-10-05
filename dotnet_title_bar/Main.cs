using fooTitle.Config;
using fooTitle.Geometries;
using fooTitle.Layers;
using Qwr.ComponentInterface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace fooTitle
{

    public enum EnableDragging
    {
        Always,
        WhenPropertiesOpen,
        Never,
    }

    public class Main : IComponent, ICallbackSender
    {

        public IConfigStorage Config;
        public IUtils Fb2kUtils;
        public IDynamicServicesManager Fb2kServices;
        public IControls Fb2kControls;
        public IPlaybackControls Fb2kPlaybackControls;
        static public ConsoleWrapper Console;
        static public string ComponentNameUnderscored = "dotnet_title_bar";
        static public string ComponentName = "Title Bar";
        public SkinState SkinState;

        public IConfigStorage TestConfig;

        // TODO: remove?
        // public Tests.TestServices TestServicesInstance;

        public ToolTipDisplay Ttd;

        protected ShowControl showControl;

        /// <summary>
        /// Set to true after everything has been created and can be manipulated
        /// </summary>
        private bool _initDone = false;

        private IPlaybackCallbacks _fb2kPlaybackCallbacks;

        private static readonly ConfString _skinsDir = new("base/dataDir", "dotnet_title_bar\\skins\\");

        /// <summary>
        /// The name of the currently used skin. Can be changed
        /// </summary>
        private readonly ConfString _skinPath = new ConfString("base/skinName", null);
        private readonly ConfInt _positionX = new ConfInt("display/positionX", 0);
        private readonly ConfInt _positionY = new ConfInt("display/positionY", 0);
        private readonly ConfBool _edgeSnap = new ConfBool("display/edgeSnap", true);
        private readonly ConfEnum<EnableDragging> _draggingEnabled = new ConfEnum<EnableDragging>("display/enableDragging", EnableDragging.Always);
        private readonly ConfInt _artLoadEvery = new ConfInt("display/artLoadEvery", 10, 1, int.MaxValue);
        private readonly ConfInt _artLoadMaxTimes = new ConfInt("display/artLoadMaxTimes", 2, -1, int.MaxValue);
        private System.Windows.Forms.Timer _redrawTimer;

        private IMetadbHandle _lastSong;
        /// <summary>
        /// Automatically handles reshowing foo_title if it's supposed to be always on top.
        /// </summary>
        private RepeatedShowing _repeatedShowing;

        // singleton
        private static Main _instance;

        /// <summary>
        /// How often the display should be redrawn (in FPS)
        /// </summary>
        private readonly ConfInt _updateInterval = new ConfInt("display/refreshRate", 30, 1, 250);

        /// <summary>
        /// List of layers, that need continuous redrawing (e.g. animation, scrolling text)
        /// </summary>
        private readonly HashSet<IContiniousRedraw> _redrawRequesters = new HashSet<IContiniousRedraw>();

        private bool _needToRedraw = false;

        private Display _display;
        private bool _isMouseOnDragLayer = true;
        /// <summary>
        /// Returns the foo_title data directory located in the foobar2000 user directory (documents and settings)
        /// </summary>
        public static string UserDataDir;

        public string SkinPath
        {
            set => _skinPath.Value = value;
            get => _skinPath.Value;
        }

        public bool EdgeSnapEnabled => _edgeSnap.Value;

        public int ArtReloadFreq => _artLoadEvery.Value;
        public int ArtReloadMax => _artLoadMaxTimes.Value;

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

        /// <summary>
        /// Provides access to the display.
        /// </summary>
        public Display Display
        {
            get
            {
                if (_display != null && _display.IsDisposed)
                {
                    _redrawTimer.Stop();
                    UnloadSkin();
                    _display = null;
                }

                return _display;
            }
        }

        public bool CanDragDisplay
        {
            get
            {
                switch (_draggingEnabled.Value)
                {
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

        public ComponentInfo GetInfo()
        {
            ComponentInfo info;
            info.Name = ComponentName;
            info.Version = Assembly.GetExecutingAssembly().GetName().Version;
            info.Description =
                "Displays a title - bar(like Winamp's WindowShade mode)\n\n"
                + "Copyright( c ) 2005-2006 by Roman Plasil\n\n"
                + "Copyright( c ) 2016 by Miha Lepej\n"
                + "https://github.com/LepkoQQ/foo_title \n\n"
                + "Copyright( c ) 2017-2021 by TheQwertiest\n"
                + "https://github.com/TheQwertiest/foo_title";

            return info;
        }

        public void Initialize(IStaticServicesManager servicesManager, IUtils utils)
        {
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            _instance = this;
            Fb2kUtils = utils;

            // create the property sheet form
            servicesManager.RegisterPreferencesPage(Properties.Info, typeof(Properties));

            // TODO: remove?
            //// create the services for testing
            // TestServicesInstance = new Tests.TestServices();

            // create a notifying string value for saving the configuration
            var cfgEntry = servicesManager.RegisterConfigVar(new Guid(457, 784, 488, 36, 48, 79, 54, 12, 36, 47, 13), "<config/>");

            // create the configuration manager
            Config = new XmlConfigStorage(cfgEntry);

            // TODO: remove?
            //TestConfig = new XmlConfigStorage(TestServicesInstance.testConfigStorage);

            // initialize show control
            showControl = new ShowControl();

            // initialize menu commands
            var menuGroup = servicesManager.GetMainMenuGroup(Fb2kUtils.Fb2kGuid(Fb2kGuidId.MainMenuGroups_View));
            var commandSection = menuGroup.AddCommandSection();
            commandSection.AddCommand(new Guid(457, 784, 488, 36, 48, 79, 54, 12, 36, 56, 1), "Toggle Title Bar", "Enables or disables dotnet_title_bar popup.", () => Hotkey_PopupToggle());
            var peekCmd = commandSection.AddCommand(new Guid(457, 784, 488, 36, 48, 79, 54, 12, 36, 56, 2), "Peek Title Bar", "Shows dotnet_title_bar popup briefly.", () => Hotkey_PopupPeek());
            peekCmd.IsDefaultHidden = true;
        }

        public void Start(IDynamicServicesManager servicesManager, IControls controls)
        {
            Fb2kServices = servicesManager;
            Fb2kControls = controls;
            Fb2kPlaybackControls = controls.PlaybackControls();
            // TODO: make public?
            _fb2kPlaybackCallbacks = servicesManager.RegisterForPlaybackCallbacks();
            Console = new ConsoleWrapper(Fb2kControls.Console());

            // TODO: cleanup
            _fb2kPlaybackCallbacks.DynamicTrackInfoChanged += (sender, args) =>
            {
                var currentTrack = Fb2kControls.PlaybackControls().NowPlaying();
                OnPlaybackDynamicInfoTrack(currentTrack.FileInfo());
            };
            _fb2kPlaybackCallbacks.PlaybackAdvancedToNewTrack += (sender, args) =>
            {
                OnPlaybackNewTrack(args.Value);
            };
            _fb2kPlaybackCallbacks.PlaybackPausedStateChanged += (sender, args) =>
            {
                OnPlaybackPause(args.Value);
            };
            _fb2kPlaybackCallbacks.PlaybackStopped += (sender, args) =>
            {
                OnPlaybackStop(args.Value);
            };
            _fb2kPlaybackCallbacks.TrackPlaybackPositionChanged += (sender, args) =>
            {
                OnPlaybackTime(args.Value);
            };

            OnInit();
        }

        public void Shutdown()
        {
            OnQuit();
        }

        /// <summary>
        /// Same as assignment to SkinPath, but triggers OnChanged even if the value is the same.
        /// </summary>
        /// <param name="value">Value</param>
        public void ForceAssignSkinPath(string value)
        {
            _skinPath.ForceUpdate(value);
        }

        public void RequestRedraw(bool force = false)
        {
            if (Display == null || !Display.Visible)
                return;

            if (force)
            {
                Display.Invalidate();
            }
            else
            {
                _needToRedraw = true;
            }
        }

        public void positionX_OnChanged(string name)
        {
            Display.SetAnchorPosition(_positionX.Value, _positionY.Value);
        }
        public void positionY_OnChanged(string name)
        {
            Display.SetAnchorPosition(_positionX.Value, _positionY.Value);
        }

        public void EnableFooTitle()
        {
            if (!_initDone || Display.Visible)
                return;

            Display.Show();
            RequestRedraw(true);
        }

        public void DisableFooTitle()
        {
            if (!_initDone || !Display.Visible)
                return;

            Display.Hide();
        }

        public void StartDisplayAnimation(AnimationManager.Animation animationName, AnimationManager.OnAnimationStopDelegate onStopCallback = null)
        {
            if (_initDone && Display.Visible)
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

        public static Main GetInstance()
        {
            return _instance;
        }

        public void DrawForm()
        {
            if (Display != null && Display.Visible && CurrentSkin != null)
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

        public void SavePosition()
        {
            Win32.Point anchorPos = Display.GetAnchorPosition();
            _positionX.Value = anchorPos.x;
            _positionY.Value = anchorPos.y;

            if (_draggingEnabled.Value == EnableDragging.Always && !Properties.IsOpen)
            {
                ConfValuesManager.GetInstance().SaveTo(Config);
            }
        }

        public void RestorePosition()
        {
            Display.SetAnchorPosition(_positionX.Value, _positionY.Value);
        }

        protected void SkinPath_OnChanged(string name)
        {
            if (_initDone)
            {
                try
                {
                    SkinState.ResetState();
                    LoadSkin(SkinPath);
                    // Changing to skin with different anchor type 
                    // may cause window to go beyond screen borders
                    Display.ReadjustPosition();
                    SavePosition();
                }
                catch (Exception e)
                {
                    CurrentSkin = null;
                    System.Windows.Forms.MessageBox.Show($"dotnet_title_bar - There was an error loading skin {SkinPath}:\n {e.Message} \n {e}", "dotnet_title_bar");
                }
            }
        }

        protected void UpdateInterval_OnChanged(string name)
        {
            if (_initDone)
                _redrawTimer.Interval = 1000 / _updateInterval.Value;
        }

        private void _redrawTimer_Tick(object sender, EventArgs e)
        {
            if (_needToRedraw)
            {
                _needToRedraw = false;
                Display.Invalidate();
            }
            else if (_redrawRequesters.Count > 0)
            {
                if (_redrawRequesters.Any(i => i.IsRedrawNeeded()))
                {
                    Display.Invalidate();
                }
            }
        }

        private void CreateDefaultDir()
        {
            try
            {
                // create dotnet_title_bar folder (does nothing if exists)
                Directory.CreateDirectory(UserDataDir);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"dotnet_title_bar - Failed to create default directory.\nPath:\n{UserDataDir}\nError message:\n{e.Message}\nError details:\n{e}"
                    , "dotnet_title_bar");
            }
        }

        /// <summary>
        /// When the Display window is closed for some reason and we want to
        /// show it again, it must be re-created and reinitialized.
        /// </summary>
        private void ReinitDisplay()
        {
            // initialize the form displaying the images
            _display = new Display(300, 22);
            _display.Closing -= myDisplay_Closing;
            _display.Closing += myDisplay_Closing;
            _display.Show();
            _redrawTimer.Start();
        }

        void myDisplay_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
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
        private void LoadSkin(string path)
        {
            try
            {
                // delete the old one
                UnloadSkin();

                if (Display == null)
                    ReinitDisplay();

                if (path == null || !Directory.Exists(path))
                {
                    path = Path.Combine(UserDataDir, "white");
                    if (!Directory.Exists(path))
                        return;
                }

                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

                CurrentSkin = new Skin(path);
                CurrentSkin.Init(Display);

                RestorePosition();

                // need to tell it about the currently playing song
                if (_lastSong != null)
                    CurrentSkin.OnPlaybackNewTrack(_lastSong);
                else
                    CurrentSkin.OnPlaybackStop(PlaybackStopReason.User);

                CurrentSkin.FirstCheckSize();

                Console.LogInfo($"skin loaded in {(int)sw.Elapsed.TotalMilliseconds} ms");

                RequestRedraw(true);
            }
            catch (Exception e)
            {
                CurrentSkin?.Free();
                CurrentSkin = null;
                System.Windows.Forms.MessageBox.Show(
                    $"dotnet_title_bar - There was an error loading skin {path}:\n {e.Message} \n {e}"
                    , "dotnet_title_bar");
            }
        }

        private void UnloadSkin()
        {
            _redrawRequesters.Clear();
            CurrentSkin?.Free();
            CurrentSkin = null;
        }

        #region DpiScale
        // System DPI only
        private static readonly int CurrentDpi = (int)Graphics.FromHwnd(IntPtr.Zero).DpiX;
        private static readonly float ScaleCoefficient = (float)CurrentDpi / 96;

        private readonly ConfBool _enableDpiScale = new ConfBool("display/dpiScale", true);
        public bool IsDpiScalable
        {
            set => _enableDpiScale.Value = value;
            get => _enableDpiScale.Value;
        }
        public void enableDpiScale_OnChanged(string name)
        {
            ForceAssignSkinPath(SkinPath);
            RequestRedraw();
        }

        public int ScaleValue(int oldValue)
        {
            return IsDpiScalable ? (int)(ScaleCoefficient * oldValue) : oldValue;
        }

        #endregion

        #region Events
        public event OnInitDelegate OnInitEvent;

        public event OnQuitDelegate OnQuitEvent;

        public event OnPlaybackNewTrackDelegate OnPlaybackNewTrackEvent;

        public event OnPlaybackTimeDelegate OnPlaybackTimeEvent;

        public event OnPlaybackStopDelegate OnPlaybackStopEvent;
        public event OnPlaybackPauseDelegate OnPlaybackPauseEvent;
        public event OnPlaybackDynamicInfoTrackDelegate OnPlaybackDynamicInfoTrackEvent;

        /// <summary>
        /// Called by init_quit, creates form, loads skin,...
        /// </summary>
        private void OnInit()
        {
            UserDataDir = Path.Combine(Fb2kUtils.ProfilePath(), _skinsDir.Value);
            CreateDefaultDir();

            Config.Load();
            ConfValuesManager.GetInstance().LoadFrom(Config);
            ConfValuesManager.GetInstance().SetStorage(Config);

            // init registered clients
            OnInitEvent?.Invoke();

            // start a timer updating the display
            _redrawTimer = new System.Windows.Forms.Timer { Interval = 1000 / _updateInterval.Value };
            _redrawTimer.Tick += _redrawTimer_Tick;

            // create layer factory
            LayerFactory = new LayerFactory();
            LayerFactory.SearchAssembly(Assembly.GetExecutingAssembly());

            // create geometry factory
            GeometryFactory = new GeometryFactory();
            GeometryFactory.SearchAssembly(Assembly.GetExecutingAssembly());

            // Skin state manager
            SkinState = new SkinState();

            // initialize the display and skin
            ReinitDisplay();
            LoadSkin(SkinPath);

            Ttd = new ToolTipDisplay();

            // register response events on some variables
            _updateInterval.OnChanged += UpdateInterval_OnChanged;
            _skinPath.OnChanged += SkinPath_OnChanged;
            _positionX.OnChanged += positionX_OnChanged;
            _positionY.OnChanged += positionY_OnChanged;
            _enableDpiScale.OnChanged += enableDpiScale_OnChanged;

            // init reshower
            _repeatedShowing = new RepeatedShowing();

            _initDone = true;
        }

        private void OnQuit()
        {
            OnQuitEvent?.Invoke();
            Display?.Hide();
            Ttd?.Hide();
        }

        private void OnPlaybackNewTrack(IMetadbHandle song)
        {
            _lastSong = song;
            SendEvent(OnPlaybackNewTrackEvent, song);
        }

        private void OnPlaybackTime(double time)
        {
            SendEvent(OnPlaybackTimeEvent, time);
        }

        private void OnPlaybackStop(PlaybackStopReason reason)
        {
            if (reason != PlaybackStopReason.StartingAnother)
                _lastSong = null;
            SendEvent(OnPlaybackStopEvent, reason);
        }

        private void SendEvent(object _event, params object[] p)
        {
            try
            {
                if (_event != null)
                {
                    Delegate d = (Delegate)_event;
                    d.DynamicInvoke(p);
                }
            }
            catch (Exception e)
            {
                Console.LogError(e.ToString());
            }
        }

        public void OnPlaybackPause(bool state)
        {
            SendEvent(OnPlaybackPauseEvent, state);
        }

        public void OnPlaybackDynamicInfoTrack(IFileInfo fileInfo)
        {
            SendEvent(OnPlaybackDynamicInfoTrackEvent, fileInfo);
        }

        #endregion
    }
}