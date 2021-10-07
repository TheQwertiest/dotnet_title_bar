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

    public enum SkinDirType
    {
        Component,
        Profile,
        ProfileOld,
    }

    public class Main : IComponent, ICallbackSender
    {
        public static readonly string ComponentNameUnderscored = "dotnet_title_bar";
        public static readonly string ComponentName = "Title Bar";

        // TODO: replace with properties
        public IConfigStorage Config;
        public IUtils Fb2kUtils;
        public IDynamicServicesManager Fb2kServices;
        public IControls Fb2kControls;
        public IPlaybackControls Fb2kPlaybackControls;
        public static ConsoleWrapper Console;
        public SkinState SkinState;

        public IConfigStorage TestConfig;

        public ToolTipDisplay Ttd;

        private ShowControl _showControl;

        /// <summary>
        /// Set to true after everything has been created and can be manipulated
        /// </summary>
        private bool _initDone = false;

        private IPlaybackCallbacks _fb2kPlaybackCallbacks;

        /// <summary>
        /// The name of the currently used skin. Can be changed
        /// </summary>
        private readonly ConfString _skinPath = Configs.Base_CurrentSkinName;
        private readonly ConfInt _positionX = Configs.Display_PositionX;
        private readonly ConfInt _positionY = Configs.Display_PositionY;
        private readonly ConfBool _edgeSnap = Configs.Display_ShouldEdgeSnap;
        private readonly ConfEnum<EnableDragging> _draggingEnabled = Configs.Display_IsDraggingEnabled;
        private readonly ConfInt _artLoadEvery = Configs.Display_ArtLoadRetryFrequency;
        private readonly ConfInt _artLoadMaxTimes = Configs.Display_ArtLoadMaxRetries;
        private readonly ConfInt _updateInterval = Configs.Display_RefreshRate;

        private System.Windows.Forms.Timer _redrawTimer;

        private IMetadbHandle _lastSong;
        /// <summary>
        /// Automatically handles reshowing foo_title if it's supposed to be always on top.
        /// </summary>
        private RepeatedShowing _repeatedShowing;

        // singleton
        private static Main _instance;

        /// <summary>
        /// List of layers, that need continuous redrawing (e.g. animation, scrolling text)
        /// </summary>
        private readonly HashSet<IContiniousRedraw> _redrawRequesters = new();

        private bool _needToRedraw = false;

        private Display _display;
        private bool _isMouseOnDragLayer = true;
        /// <summary>
        /// Returns the foo_title data directory located in the foobar2000 user directory (documents and settings)
        /// </summary>
        public static string ComponentSkinsDir;
        public static string ProfileSkinsDir;
        public static string ProfileSkinsDirOld;

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
                return _draggingEnabled.Value switch
                {
                    EnableDragging.Always => _isMouseOnDragLayer,
                    EnableDragging.WhenPropertiesOpen => Properties.IsOpen,
                    EnableDragging.Never => false,
                    _ => throw new ArgumentOutOfRangeException(),
                };
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

            // create a notifying string value for saving the configuration
            var cfgEntry = servicesManager.RegisterConfigVar(Guids.Config, "<config/>");

            // create the configuration manager
            Config = new XmlConfigStorage(cfgEntry);

            // create the property sheet form
            servicesManager.RegisterPreferencesPage(Properties.Info, typeof(Properties));

            // initialize show control
            _showControl = new ShowControl();

            // initialize menu commands
            var menuGroup = servicesManager.GetMainMenuGroup(Fb2kUtils.Fb2kGuid(Fb2kGuidId.MainMenuGroups_View));
            var commandSection = menuGroup.AddCommandSection();
            commandSection.AddCommand(Guids.MainMenu_ToggleTitleBar, "Toggle Title Bar", "Enables or disables dotnet_title_bar popup.", () => Hotkey_PopupToggle());
            var peekCmd = commandSection.AddCommand(Guids.MainMenu_PeekTitleBar, "Peek Title Bar", "Shows dotnet_title_bar popup briefly.", () => Hotkey_PopupPeek());
            peekCmd.IsDefaultHidden = true;
        }

        public void Start(IDynamicServicesManager servicesManager, IControls controls)
        {
            Fb2kServices = servicesManager;
            Fb2kControls = controls;
            Fb2kPlaybackControls = controls.PlaybackControls();

            _fb2kPlaybackCallbacks = servicesManager.RegisterForPlaybackCallbacks();
            Console = new ConsoleWrapper(Fb2kControls.Console());

            // TODO: cleanup
            _fb2kPlaybackCallbacks.DynamicTrackInfoChanged += (sender, e) =>
            {
                var currentTrack = Fb2kControls.PlaybackControls().NowPlaying();
                OnDynamicTrackInfoChanged(currentTrack.FileInfo());
            };
            _fb2kPlaybackCallbacks.PlaybackAdvancedToNewTrack += (sender, e) =>
            {
                OnPlaybackAdvancedToNewTrack(e.Value);
            };
            _fb2kPlaybackCallbacks.PlaybackPausedStateChanged += (sender, e) =>
            {
                OnPlaybackPausedStateChanged(e.Value);
            };
            _fb2kPlaybackCallbacks.PlaybackStopped += (sender, e) =>
            {
                OnPlaybackStopped(e.Value);
            };
            _fb2kPlaybackCallbacks.TrackPlaybackPositionChanged += (sender, e) =>
            {
                OnTrackPlaybackPositionChanged(e.Value);
            };

            OnInitialized();
        }

        public void Shutdown()
        {
            OnQuit();
        }

        public static SkinDirType SkinRootPathToEnum(string rootPath)
        {
            if (rootPath.StartsWith(ComponentSkinsDir))
            {
                return SkinDirType.Component;
            }
            else if (rootPath.StartsWith(ProfileSkinsDir))
            {
                return SkinDirType.Profile;
            }
            else if (rootPath.StartsWith(ProfileSkinsDirOld))
            {
                return SkinDirType.ProfileOld;
            }
            else
            {
                throw new Exception("Internal error: unexpected skin root path `{rootPath}`");
            }
        }

        public static string SkinEnumToRootPath(SkinDirType skinDirType)
        {
            return skinDirType switch
            {
                SkinDirType.Component => ComponentSkinsDir,
                SkinDirType.Profile => ProfileSkinsDir,
                SkinDirType.ProfileOld => ProfileSkinsDirOld,
                _ => throw new Exception("Internal error: unexpected skin dir type path `{skinDirType}`")
            };
        }

        /// <summary>
        /// Same as assignment to SkinPath, but triggers OnChanged even if the value is the same.
        /// </summary>
        public void ForceAssignSkinPath(SkinDirType dirType, string skinDir)
        {
            Configs.Base_SkinDirType.Value = dirType;
            Configs.Base_CurrentSkinName.ForceUpdate(skinDir);
        }

        public void RequestRedraw(bool force = false)
        {
            if (Display == null || !Display.Visible)
            {
                return;
            }

            if (force)
            {
                Display.Invalidate();
            }
            else
            {
                _needToRedraw = true;
            }
        }

        public void PositionX_OnChanged(string name)
        {
            Display.SetAnchorPosition(_positionX.Value, _positionY.Value);
        }
        public void PositionY_OnChanged(string name)
        {
            Display.SetAnchorPosition(_positionX.Value, _positionY.Value);
        }

        public void EnableTitleBar()
        {
            if (!_initDone || Display.Visible)
            {
                return;
            }

            Display.Show();
            RequestRedraw(true);
        }

        public void DisableTitleBar()
        {
            if (!_initDone || !Display.Visible)
            {
                return;
            }

            Display.Hide();
        }

        public void StartDisplayAnimation(AnimationManager.Animation animationName, AnimationManager.AnimationStoppedEventHandler onStopCallback = null)
        {
            if (_initDone && Display.Visible)
            {
                Display.AnimManager.StartAnimation(animationName, onStopCallback);
            }
        }

        public void Hotkey_PopupToggle()
        {
            _showControl.TogglePopup();
        }

        public void Hotkey_PopupPeek()
        {
            _showControl.TriggerPopup();
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

        public static bool IsCurrentSkin(SkinDirType dirType, string skinDir)
        {
            return (dirType == Configs.Base_SkinDirType.Value && skinDir == Configs.Base_CurrentSkinName.Value);
        }

        protected void SkinPath_OnChanged(string name)
        {
            if (!_initDone)
            {
                return;
            }

            try
            {
                SkinState.ResetState();
                LoadSkin();
                // Changing to skin with different anchor type
                // may cause window to go beyond screen borders
                Display.ReadjustPosition();
                SavePosition();
            }
            catch (Exception e)
            {
                CurrentSkin = null;
                Utils.ReportErrorWithPopup($"There was an error loading skin:\n"
                                           + $"{e.Message}\n\n"
                                           + $"{e}");
            }
        }

        protected void UpdateInterval_OnChanged(string name)
        {
            if (!_initDone)
            {
                return;
            }
            _redrawTimer.Interval = 1000 / _updateInterval.Value;
        }

        private void RedrawTimer_Tick(object sender, EventArgs e)
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

        private void InitializeDisplay()
        {
            // initialize the form displaying the images
            _display = new Display(300, 22);
            _display.Closing += MyDisplay_Closing;
            _redrawTimer.Start();
        }

        void MyDisplay_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _display.Closing -= MyDisplay_Closing;
            _redrawTimer.Stop();
            UnloadSkin();
            _display = null;
        }

        private void LoadSkin()
        {
            try
            {
                UnloadSkin();

                var sw = System.Diagnostics.Stopwatch.StartNew();

                CurrentSkin = new Skin(Configs.Base_SkinDirType.Value, Configs.Base_CurrentSkinName.Value);
                CurrentSkin.Initialize(Display);

                RestorePosition();

                // need to tell it about the currently playing song
                if (_lastSong != null)
                {
                    CurrentSkin.OnPlaybackNewTrack(_lastSong);
                }
                else
                {
                    CurrentSkin.OnPlaybackStop(PlaybackStopReason.User);
                }

                CurrentSkin.FirstCheckSize();

                Console.LogInfo($"skin loaded in {(int)sw.Elapsed.TotalMilliseconds} ms");

                RequestRedraw(true);
            }
            catch (Exception e)
            {
                CurrentSkin?.Dispose();
                CurrentSkin = null;

                Utils.ReportErrorWithPopup($"There was an error loading skin `{Configs.Base_CurrentSkinName.Value}`:\n"
                                           + $"{e.Message}\n\n"
                                           + $"{e}");
            }
        }

        private void UnloadSkin()
        {
            _redrawRequesters.Clear();
            CurrentSkin?.Dispose();
            CurrentSkin = null;
        }

        // System DPI only
        private static readonly int CurrentDpi = (int)Graphics.FromHwnd(IntPtr.Zero).DpiX;
        private static readonly float ScaleCoefficient = (float)CurrentDpi / 96;

        private readonly ConfBool _enableDpiScale = Configs.Display_IsDpiScalingEnabled;
        public bool IsDpiScalable
        {
            set => _enableDpiScale.Value = value;
            get => _enableDpiScale.Value;
        }
        public void EnableDpiScale_OnChanged(string name)
        {
            ForceAssignSkinPath(Configs.Base_SkinDirType.Value, Configs.Base_CurrentSkinName.Value);
            RequestRedraw();
        }

        public int ScaleValue(int oldValue)
        {
            return IsDpiScalable ? (int)(ScaleCoefficient * oldValue) : oldValue;
        }

        public event InitializedEventHandler Initialized;
        public event QuitEventHandler Quit;
        public event PlaybackAdvancedToNewTrackEventHandler PlaybackAdvancedToNewTrack;
        public event TrackPlaybackPositionChangedEventHandler TrackPlaybackPositionChanged;
        public event PlaybackStoppedEventhandler PlaybackStopped;
        public event PlaybackPausedStateChangedEventHandler PlaybackPausedStateChanged;
        public event DynamicTrackInfoChangedEventHandler DynamicTrackInfoChanged;

        /// <summary>
        /// Called by init_quit, creates form, loads skin,...
        /// </summary>
        private void OnInitialized()
        {
            ComponentSkinsDir = Path.Combine(Assembly.GetExecutingAssembly().Location, "skins");
            ProfileSkinsDir = Path.Combine(Fb2kUtils.ProfilePath(), ComponentNameUnderscored, "skins");
            ProfileSkinsDirOld = Path.Combine(Fb2kUtils.ProfilePath(), "foo_title");

            Config.Load();
            ConfValuesManager.GetInstance().LoadFrom(Config);
            ConfValuesManager.GetInstance().SetStorage(Config);

            // init registered clients
            Initialized?.Invoke();

            // start a timer updating the display
            _redrawTimer = new System.Windows.Forms.Timer { Interval = 1000 / _updateInterval.Value };
            _redrawTimer.Tick += RedrawTimer_Tick;

            // create layer factory
            LayerFactory = new LayerFactory();
            LayerFactory.SearchAssembly(Assembly.GetExecutingAssembly());

            // create geometry factory
            GeometryFactory = new GeometryFactory();
            GeometryFactory.SearchAssembly(Assembly.GetExecutingAssembly());

            // Skin state manager
            SkinState = new SkinState();

            // initialize the display and skin
            InitializeDisplay();
            LoadSkin();

            Ttd = new ToolTipDisplay();

            // register response events on some variables
            _updateInterval.Changed += UpdateInterval_OnChanged;
            _skinPath.Changed += SkinPath_OnChanged;
            _positionX.Changed += PositionX_OnChanged;
            _positionY.Changed += PositionY_OnChanged;
            _enableDpiScale.Changed += EnableDpiScale_OnChanged;

            // init reshower
            _repeatedShowing = new RepeatedShowing();

            _initDone = true;

            // Now we are ready to show stuff
            _showControl.InitializeState();
        }

        private void OnQuit()
        {
            Quit?.Invoke();
            Display?.Hide();
            Ttd?.Hide();
        }

        private void OnPlaybackAdvancedToNewTrack(IMetadbHandle song)
        {
            _lastSong = song;
            SendEvent(PlaybackAdvancedToNewTrack, song);
        }

        private void OnTrackPlaybackPositionChanged(double time)
        {
            SendEvent(TrackPlaybackPositionChanged, time);
        }

        private void OnPlaybackStopped(PlaybackStopReason reason)
        {
            if (reason != PlaybackStopReason.StartingAnother)
            {
                _lastSong = null;
            }
            SendEvent(PlaybackStopped, reason);
        }

        private static void SendEvent(object _event, params object[] p)
        {
            try
            {
                if (_event != null)
                {
                    var d = (Delegate)_event;
                    d.DynamicInvoke(p);
                }
            }
            catch (Exception e)
            {
                Console.LogError(e.ToString());
            }
        }

        public void OnPlaybackPausedStateChanged(bool state)
        {
            SendEvent(PlaybackPausedStateChanged, state);
        }

        public void OnDynamicTrackInfoChanged(IFileInfo fileInfo)
        {
            SendEvent(DynamicTrackInfoChanged, fileInfo);
        }
    }
}
