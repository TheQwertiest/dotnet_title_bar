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
using System.Drawing;
using System.Collections;
using System.IO;

using fooManagedWrapper;
using fooTitle.Layers;
using fooTitle.Geometries;
using fooTitle;
using fooTitle.Config;


namespace fooTitle {
    public enum ShowWhenEnum {
        Always,
        WhenMinimized,
        Never
    }

    public class Main : IComponentClient, IPlayCallbackSender {
        public static IPlayControl PlayControl;
        /// <summary>
        /// Set to true after everything has been created and can be manipulated
        /// </summary>
        private bool initDone = false;

        private static ConfString myDataDir = new ConfString("base/dataDir", "foo_title\\");

        /// <summary>
        /// Returns the foo_title data directory in the foobar2000 application directory.
        /// </summary>
        public static string AppDataDir {
            get {
                return Path.Combine(CManagedWrapper.getInstance().GetFoobarDirectory(), myDataDir.Value);
            }
        }

        /// <summary>
        /// Returns the foo_title data directory located in the foobar2000 user directory (documents and settings)
        /// </summary>
        public static string UserDataDir {
            get {
                
                return Path.Combine(CManagedWrapper.getInstance().GetProfilePath(), myDataDir.Value);
            }
        }

        /// <summary>
        /// How often the display should be redrawn
        /// </summary>
        private ConfInt UpdateInterval = new ConfInt("display/updateInterval", 100, 50, 500);
        protected void updateIntervalChanged(string name) {
            if (initDone)
                timer.Interval = UpdateInterval.Value;
        }

        /// <summary>
        /// The name of the currently used skin. Can be changed
        /// </summary>
        public ConfString SkinPath = new ConfString("base/skinName", "white");

        protected void skinNameChanged(string name) {
            if (initDone) {
                try {
                    loadSkin(SkinPath.Value);
                } catch (Exception e) {
                    skin = null;
                    System.Windows.Forms.MessageBox.Show("foo_title - There was an error loading skin " + SkinPath.Value + ":\n" + e.Message, "foo_title");
                }
            }
        }

        private Skin skin;
        /// <summary>
        /// Provides access to the current skin. May be null.
        /// </summary>
        public Skin CurrentSkin {
            get {
                return skin;
            }
        }

        /// <summary>
        /// A semicolon separated list of possible filenames (without extension) for album art. 
        /// May contain formatting strings.
        /// </summary>
        public ConfString AlbumArtFilenames = new ConfString("skin/albumArtFilenames", "folder");

        private LayerFactory myLayerFactory;
        /// <summary>
        /// Provides access to the layer factory for creating new layers.
        /// </summary>
        public LayerFactory LayerFactory {
            get {
                return myLayerFactory;
            }
        }

        private GeometryFactory myGeometryFactory;
        /// <summary>
        /// Provides access to the geometry factory for createing new geometries.
        /// </summary>
        public GeometryFactory GeometryFactory {
            get {
                return myGeometryFactory;
            }
        }

        private Display myDisplay;
        /// <summary>
        /// Provides access to the display.
        /// </summary>
        public Display Display {
            get {
                if (myDisplay.IsDisposed) {
                    // need to open the window again
                    reinitDisplay();
                }

                return myDisplay;
            }
        }

        /// <summary>
        /// Automatically handles reshowing foo_title if it's supposed to be always on top.
        /// </summary>
        protected RepeatedShowing repeatedShowing;

        /// <summary>
        /// When to show foo_title: Always, never or only when foobar is minimized
        /// </summary>
        public ConfEnum<ShowWhenEnum> ShowWhen = new ConfEnum<ShowWhenEnum>("display/showWhen", ShowWhenEnum.Always);
        void ShowWhen_OnChanged(string name) {

            if (ShowWhen.Value == ShowWhenEnum.Always)
                EnableFooTitle();
            else if (ShowWhen.Value == ShowWhenEnum.Never)
                DisableFooTitle();
            else {
                checkFoobarMinimized();
            }
        }

        protected bool fooTitleEnabled = true;
        public void DisableFooTitle() {
            if (initDone)
                Display.Hide();
            fooTitleEnabled = false;
        }

        public void EnableFooTitle() {
            fooTitleEnabled = true;
            if (initDone) {
                Display.Show();
            }
        }

        public void ToggleEnabled() {
            if (ShowWhen.Value == ShowWhenEnum.Always)
                ShowWhen.Value = ShowWhenEnum.Never;
            else
                ShowWhen.Value = ShowWhenEnum.Always;
        }

        /// <summary>
        /// Checks if foobar is minimized or active and shows/hides display according to it
        /// </summary>
        private void checkFoobarMinimized() {
            if (ShowWhen.Value != ShowWhenEnum.WhenMinimized)
                return;

            //CConsole.Write(String.Format("foobar activated {0}", CManagedWrapper.getInstance().IsFoobarActivated()));

            if (CManagedWrapper.getInstance().IsFoobarActivated()) {
                DisableFooTitle();
            } else {
                showControl.TryShowWhenMinimized();
            }
        }

        private ConfInt positionX = new ConfInt("display/positionX", 0);
        private ConfInt positionY = new ConfInt("display/positionY", 0);


        System.Windows.Forms.Timer timer;

        CMetaDBHandle lastSong;
        Properties propsForm;

        // singleton
        static private Main instance = null;
        static public Main GetInstance() {
            return instance;
        }

        private void timerUpdate(object sender, System.EventArgs e) {
            if (fooTitleEnabled && skin != null) {
                // need to update all values that are calculated from formatting strings
                //CurrentSkin.UpdateGeometry(CurrentSkin.ClientRect);
                CurrentSkin.FrameUpdateGeometry(CurrentSkin.ClientRect);
                CurrentSkin.CheckSize();
                updateDisplay();
            }
            checkFoobarMinimized();
        }

        /// <summary>
        /// Redraws the display, called every UpdateInterval miliseconds by the timer
        /// </summary>
        private void updateDisplay() {
            if (skin != null) {
                skin.Draw();
                myDisplay.FrameRedraw();
            }
        }

        /// <summary>
        /// When the Display window is closed for some reason and we want to
        /// show it again, it must be re-created and reinitialized.
        /// </summary>
        private void reinitDisplay() {
            // initialize the form displaying the images
            myDisplay = new Display(300, 22);
            myDisplay.Show();

            // load the skin
            loadSkin(SkinPath.Value);

            RestorePosition();
        }

        /// <summary>
        /// Loads skin by from the given path. If the path is not absolute,
        /// the application directory is used to load the skin from.
        /// </summary>
        /// <param name="path">The name of the skin's directory</param>
        private void loadSkin(string path) {
            try {
                // delete the old one
                if (skin != null)
                    skin.Free();
                skin = null;

                // determine the absolute path
                string absolutePath = path;
                if (!Path.IsPathRooted(path))
                    absolutePath = Path.Combine(Main.AppDataDir, path);

                skin = new Skin(absolutePath);
                skin.Init(Display);

                // need to tell it about the currently playing song
                if (lastSong != null)
                    skin.OnPlaybackNewTrack(lastSong);
                else
                    skin.OnPlaybackStop(IPlayControl.StopReason.stop_reason_user);

                skin.FirstCheckSize();
            } catch (Exception e) {
                if (skin != null)
                    skin.Free();
                skin = null;
                System.Windows.Forms.MessageBox.Show(
                    String.Format("foo_title - There was an error loading skin {0}:\n {1} \n {2}", path, e.Message, e.ToString())
                    , "foo_title");
            }
        }

        protected ViewMenuCommands viewMenuCommands;
        protected ShowControl showControl;

        public IConfigStorage Config;
        public IConfigStorage TestConfig;

        public Tests.TestServices TestServicesInstance;

        public void Create() {
            instance = this;

            // create the property sheet form
            propsForm = new Properties(this);

            // create the services for testing
            TestServicesInstance = new Tests.TestServices();

            // create a notifying string value for saving the configuration
            CNotifyingCfgString cfgEntry = new CNotifyingCfgString(new Guid(457, 784, 488, 36, 48, 79, 54, 12, 36, 47, 13), "<config/>");
            cfgEntry.BeforeWriting += new CNotifyingCfgString.BeforeWritingDelegate(cfgEntry_BeforeWriting);

            // create the configuration manager
            Config = new XmlConfigStorage(cfgEntry);
            TestConfig = new XmlConfigStorage(TestServicesInstance.testConfigStorage);

            // initialize show control
            showControl = new ShowControl();

            // initialize menu commands
            viewMenuCommands = new ViewMenuCommands();

        }

        private void cfgEntry_BeforeWriting(CNotifyingCfgString sender) {
            ConfValuesManager.GetInstance().SaveTo(Config);

        }

        public void SavePosition() {
            positionX.Value = Display.Left;
            positionY.Value = Display.Top;
        }

        public void RestorePosition() {
            Display.Left = positionX.Value;
            Display.Top = positionY.Value;
        }

        #region Events
        public event OnInitDelegate OnInitEvent;

        /// <summary>
        /// Called by init_quit, creates form, loads skin,...
        /// </summary>
        public void OnInit(IPlayControl a) {
            Main.PlayControl = a;

#if DEBUG
            // run the tests
            Tests.TestFramework t = new Tests.test_all();
            t.Run();
            t.ReportGUI();
#endif
            Config.Load();
            ConfValuesManager.GetInstance().LoadFrom(Config);
            ConfValuesManager.GetInstance().SetStorage(Config);



            propsForm.UpdateValues();

            // init registered clients
            OnInitDelegate temp = OnInitEvent;
            if (temp != null)
                temp();

            // start a timer updating the display
            timer = new System.Windows.Forms.Timer();
            timer.Interval = UpdateInterval.Value;
            timer.Tick += new System.EventHandler(timerUpdate);
            timer.Enabled = true;

            // create layer factory
            myLayerFactory = new LayerFactory();
            myLayerFactory.SearchAssembly(System.Reflection.Assembly.GetExecutingAssembly());

            // create geometry factory
            myGeometryFactory = new GeometryFactory();
            myGeometryFactory.SearchAssembly(System.Reflection.Assembly.GetExecutingAssembly());

            // initialize the display and skin
            reinitDisplay();


            // register response events on some variables
            ShowWhen.OnChanged += new ConfValuesManager.ValueChangedDelegate(ShowWhen_OnChanged);
            UpdateInterval.OnChanged += new ConfValuesManager.ValueChangedDelegate(updateIntervalChanged);
            SkinPath.OnChanged += new ConfValuesManager.ValueChangedDelegate(skinNameChanged);

            if (ShowWhen.Value == ShowWhenEnum.Never)
                DisableFooTitle();

            // init reshower
            repeatedShowing = new RepeatedShowing();

            initDone = true;


            // DEBUG
            CConsole.Write(CManagedWrapper.getInstance().GetAllCommands());


        }

        public event OnQuitDelegate OnQuitEvent;

        public void OnQuit() {
            OnQuitDelegate temp = OnQuitEvent;
            if (temp != null)
                temp();

            if (myDisplay != null)
                myDisplay.Hide();
        }


        public event OnPlaybackNewTrackDelegate OnPlaybackNewTrackEvent;

        public void OnPlaybackNewTrack(fooManagedWrapper.CMetaDBHandle song) {
            lastSong = song;
            sendEvent(OnPlaybackNewTrackEvent, song);
        }

        public event OnPlaybackTimeDelegate OnPlaybackTimeEvent;

        public void OnPlaybackTime(double time) {
            sendEvent(OnPlaybackTimeEvent, time);
        }

        public event OnPlaybackStopDelegate OnPlaybackStopEvent;
        public event OnPlaybackPauseDelegate OnPlaybackPauseEvent;
        public void OnPlaybackStop(IPlayControl.StopReason reason) {
            if (reason != IPlayControl.StopReason.stop_reason_starting_another)
                lastSong = null;
            sendEvent(OnPlaybackStopEvent, reason);
        }


        protected void sendEvent(Object _event, params Object[] p) {
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
            sendEvent(OnPlaybackPauseEvent, state);
        }

        public event OnPlaybackDynamicInfoTrackDelegate OnPlaybackDynamicInfoTrackEvent;

        public void OnPlaybackDynamicInfoTrack(fooManagedWrapper.FileInfo fileInfo) {
            sendEvent(OnPlaybackDynamicInfoTrackEvent, fileInfo);
        }

        #endregion
    }
}
