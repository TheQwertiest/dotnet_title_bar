using fooTitle.Config;
using fooTitle.Geometries;
using fooTitle.Layers;
using Qwr.ComponentInterface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace fooTitle
{
    public class Main : IComponent, ICallbackSender
    {
        private static Main _instance;

        private ShowControl _showControl;

        /// <summary>
        /// Set to true after everything has been created and can be manipulated
        /// </summary>
        private bool _initDone = false;

        private Skin _currentSkin;

        private ToolTipForm _tooltipForm;

        private IPlaybackCallbacks _fb2kPlaybackCallbacks;

        private readonly ConfInt _positionX = Configs.Display_PositionX;
        private readonly ConfInt _positionY = Configs.Display_PositionY;

        private System.Windows.Forms.Timer _redrawTimer;
        private IMetadbHandle _lastSong;
        private IMainMenuCommand _enableDisplayCommand;

        /// <summary>
        /// List of layers, that need continuous redrawing (e.g. animation, scrolling text)
        /// </summary>
        private readonly HashSet<IContiniousRedraw> _redrawRequesters = new();

        private bool _needToRedraw = false;

        private SkinForm _skinForm;
        private bool _isMouseOnDragLayer = true;

        public event Initialized_EventHandler Initialized;
        public event Quit_EventHandler Quit;
        public event PlaybackAdvancedToNewTrack_EventHandler PlaybackAdvancedToNewTrack;
        public event TrackPlaybackPositionChanged_EventHandler TrackPlaybackPositionChanged;
        public event PlaybackStoppedEventhandler PlaybackStopped;
        public event PlaybackPausedStateChanged_EventHandler PlaybackPausedStateChanged;
        public event DynamicTrackInfoChanged_EventHandler DynamicTrackInfoChanged;

        [ComponentInterfaceVersion("0.1.1")]
        public Main()
        {
        }

        public ComponentInfo GetInfo()
        {
            ComponentInfo info;
            info.Name = Constants.ComponentName;
            info.Version = Utils.GetVersion();
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
            _instance = this;
            Fb2kUtils = utils;

            // create a notifying string value for saving the configuration
            var cfgEntry = servicesManager.RegisterConfigVar(Guids.Config, "<config/>");

            // create the configuration manager
            Config = new XmlConfigStorage(cfgEntry);

            servicesManager.RegisterAcfu(Guids.Acfu, Constants.ComponentNameUnderscored, "TheQwertiest");

            // create the property sheet form
            servicesManager.RegisterPreferencesPage(Properties.Info, typeof(Properties));

            // initialize show control
            _showControl = new ShowControl();

            // initialize menu commands
            var menuGroup = servicesManager.GetMainMenuGroup(Fb2kUtils.Fb2kGuid(Fb2kGuidId.MainMenuGroups_View));
            var commandSection = menuGroup.AddCommandSection();
            _enableDisplayCommand = commandSection.AddCommand(Guids.MainMenu_ToggleTitleBar, "Toggle Title Bar", "Enables or disables dotnet_title_bar popup.", () =>
            {
                _showControl.TogglePopup();
                _enableDisplayCommand.IsChecked = (Configs.ShowControl_EnableWhen.Value != EnableWhenEnum.Never);
            });
            _enableDisplayCommand.IsChecked = (Configs.ShowControl_EnableWhen.Value != EnableWhenEnum.Never);
            var peekCmd = commandSection.AddCommand(Guids.MainMenu_PeekTitleBar, "Peek Title Bar", "Shows dotnet_title_bar popup briefly.", () => _showControl.TriggerPopup());
            peekCmd.IsDefaultHidden = true;
        }

        public void Start(IDynamicServicesManager servicesManager, IControls controls)
        {
            Fb2kServices = servicesManager;
            Fb2kControls = controls;
            Fb2kPlaybackControls = controls.PlaybackControls();

            _fb2kPlaybackCallbacks = servicesManager.RegisterForPlaybackCallbacks();

            OnInitialized();
        }

        public void Shutdown()
        {
            OnQuit();
        }

        public IConfigStorage Config { get; private set; }
        public IUtils Fb2kUtils { get; private set; }
        public IDynamicServicesManager Fb2kServices { get; private set; }
        public IControls Fb2kControls { get; private set; }
        public IPlaybackControls Fb2kPlaybackControls { get; private set; }

        /// <summary>
        /// Provides access to the layer factory for creating new layers.
        /// </summary>
        public LayerFactory LayerFactory { get; private set; }

        /// <summary>
        /// Provides access to the geometry factory for creating new geometries.
        /// </summary>
        public GeometryFactory GeometryFactory { get; private set; }

        public bool CanDragDisplay
        {
            get
            {
                return Configs.Display_IsDraggingEnabled.Value switch
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

        private SkinForm SkinForm
        {
            get
            {
                if (_skinForm != null && _skinForm.IsDisposed)
                {
                    _redrawTimer.Stop();
                    UnloadSkin();
                    _skinForm = null;
                }

                return _skinForm;
            }
        }

        public static Main Get()
        {
            return _instance;
        }

        public void ReloadSkin()
        {
            try
            {
                LoadSkin();
                // Changing to skin with different anchor type
                // may cause window to go beyond screen borders
                SkinForm.ReadjustPosition();
                SavePosition();
            }
            catch (Exception e)
            {
                _currentSkin = null;
                Utils.ReportErrorWithPopup($"There was an error loading skin:\n"
                                           + $"{e.Message}\n\n"
                                           + $"{e}");
            }
        }

        public void ShowTitleBar()
        {
            if (!_initDone || SkinForm.Visible)
            {
                return;
            }

            SkinForm.Show();
            RedrawTitleBar(true);
        }

        public void HideTitleBar()
        {
            if (!_initDone || !SkinForm.Visible)
            {
                return;
            }

            SkinForm.Hide();
        }

        public void RedrawTitleBar(bool force = false)
        {
            if (SkinForm == null || !SkinForm.Visible)
            {
                return;
            }

            if (force)
            {
                SkinForm.Invalidate();
            }
            else
            {
                _needToRedraw = true;
            }
        }

        public void StartDisplayAnimation(FadeAnimation animationName, Action onStopCallback = null)
        {
            if (_initDone && SkinForm.Visible)
            {
                SkinForm.AnimManager.StartAnimation(animationName, onStopCallback);
            }
        }

        public void DrawTitleBar(Graphics canvas)
        {
            if (_currentSkin != null)
            {
                // need to update all values that are calculated from formatting strings
                //_currentSkin.UpdateGeometry(_currentSkin.ClientRect);
                _currentSkin.UpdateDynamicGeometry(_currentSkin.ClientRect);
                _currentSkin.CheckSize();
                _currentSkin.Draw(canvas);
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
            Win32.Point anchorPos = SkinForm.GetAnchorPosition();
            _positionX.Value = anchorPos.x;
            _positionY.Value = anchorPos.y;

            if (Configs.Display_IsDraggingEnabled.Value == EnableDragging.Always && !Properties.IsOpen)
            {
                ConfValuesManager.GetInstance().SaveTo(Config);
            }
        }

        public void RestorePosition()
        {
            SkinForm.SetAnchorPosition(_positionX.Value, _positionY.Value);
        }

        private void InitializeDisplay()
        {
            // initialize the form displaying the images
            _skinForm = new SkinForm(300, 22);
            _skinForm.Closing += MyDisplay_Closing;
            _redrawTimer.Start();
        }

        private void LoadSkin()
        {
            try
            {
                UnloadSkin();

                var sw = System.Diagnostics.Stopwatch.StartNew();

                var skinPath = SkinUtils.GenerateSkinPath(Configs.Base_SkinDirType.Value, Configs.Base_CurrentSkinName.Value);
                _currentSkin = new Skin(skinPath, SkinForm, _tooltipForm);

                RestorePosition();

                // need to tell it about the currently playing song
                if (_lastSong != null)
                {
                    _currentSkin.OnPlaybackNewTrack(_lastSong);
                }
                else
                {
                    _currentSkin.OnPlaybackStop(PlaybackStopReason.User);
                }

                _currentSkin.FirstCheckSize();

                Console.Get().LogInfo($"skin loaded in {(int)sw.Elapsed.TotalMilliseconds} ms");

                RedrawTitleBar(true);
            }
            catch (Exception e)
            {
                _currentSkin?.Dispose();
                _currentSkin = null;

                Utils.ReportErrorWithPopup($"There was an error loading skin `{Configs.Base_CurrentSkinName.Value}`:\n"
                                           + $"{e.Message}\n\n"
                                           + $"{e}");
            }
        }

        private void UnloadSkin()
        {
            _redrawRequesters.Clear();
            _currentSkin?.Dispose();
            _currentSkin = null;
        }

        private void SkinPath_Changed_EventHandler(string name)
        {
            if (!_initDone)
            {
                return;
            }

            SkinState.ResetState();
            ReloadSkin();
        }

        private void UpdateInterval_Changed_EventHandler(string name)
        {
            if (!_initDone)
            {
                return;
            }

            _redrawTimer.Interval = 1000 / Configs.Display_RefreshRate.Value;
        }

        private void RedrawTimer_Tick(object sender, EventArgs e)
        {
            if (_needToRedraw || _redrawRequesters.Any(i => i.IsRedrawNeeded()))
            {
                _needToRedraw = false;
                SkinForm.Invalidate();
            }
        }

        private void EnableDpiScale_Changed_EventHandler(string name)
        {
            ReloadSkin();
            RedrawTitleBar();
        }

        private void MyDisplay_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _skinForm.Closing -= MyDisplay_Closing;
            _redrawTimer.Stop();
            UnloadSkin();
            _skinForm = null;
        }

        private void PositionX_Changed_EventHandler(string name)
        {
            SkinForm.SetAnchorPosition(_positionX.Value, _positionY.Value);
        }
        private void PositionY_Changed_EventHandler(string name)
        {
            SkinForm.SetAnchorPosition(_positionX.Value, _positionY.Value);
        }

        /// <summary>
        /// Called by init_quit, creates form, loads skin,...
        /// </summary>
        private void OnInitialized()
        {
            Config.Load();
            ConfValuesManager.GetInstance().LoadFrom(Config);
            ConfValuesManager.GetInstance().SetStorage(Config);

            Initialized?.Invoke();

            _redrawTimer = new System.Windows.Forms.Timer { Interval = 1000 / Configs.Display_RefreshRate.Value };
            _redrawTimer.Tick += RedrawTimer_Tick;

            LayerFactory = new LayerFactory();
            LayerFactory.CollectDataFromAssembly(Assembly.GetExecutingAssembly());

            GeometryFactory = new GeometryFactory();
            GeometryFactory.CollectDataFromAssembly(Assembly.GetExecutingAssembly());

            _tooltipForm = new ToolTipForm();

            InitializeDisplay();
            LoadSkin();

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
            Configs.Display_RefreshRate.Changed += UpdateInterval_Changed_EventHandler;
            Configs.Base_CurrentSkinName.Changed += SkinPath_Changed_EventHandler;
            Configs.Display_IsDpiScalingEnabled.Changed += EnableDpiScale_Changed_EventHandler;
            _positionX.Changed += PositionX_Changed_EventHandler;
            _positionY.Changed += PositionY_Changed_EventHandler;

            _initDone = true;

            // Now we are ready to show stuff
            _showControl.InitializeState();
        }

        private void OnQuit()
        {
            Quit?.Invoke();
            SkinForm?.Hide();
            _tooltipForm?.Hide();
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
                Console.Get().LogError(e.ToString());
            }
        }

        private void OnPlaybackPausedStateChanged(bool state)
        {
            SendEvent(PlaybackPausedStateChanged, state);
        }

        private void OnDynamicTrackInfoChanged(IFileInfo fileInfo)
        {
            SendEvent(DynamicTrackInfoChanged, fileInfo);
        }
    }
}
