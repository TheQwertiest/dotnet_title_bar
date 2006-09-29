using System;
using System.Drawing;
using System.Collections;
using System.IO;

using fooManagedWrapper;
using fooTitle.Layers;
using fooTitle.Geometries;
using fooTitle;


namespace fooTitle
{
    public enum ShowWhenEnum {
        Always = 1,
        WhenMinimized = 2,
        Never = 3
    }

	public class Main : IComponentClient, IPlayCallbackSender
	{
		public static IPlayControl PlayControl;
        /// <summary>
        /// Set to true after everything has been created and can be manipulated
        /// </summary>
        private bool initDone = false;

        private static CCfgString myDataDir;
        /// <summary>
		/// Returns where the skin and other components should look for their files.
		/// </summary>
		public static string DataDir {
			get {
				return Path.Combine(CManagedWrapper.getInstance().GetFoobarDirectory(), myDataDir.GetVal());
			}
		}

        private CCfgInt myUpdateInterval;
        /// <summary>
        /// How often the display should be redrawn
        /// </summary>
        public int UpdateInterval {
            get {
                return myUpdateInterval.GetVal();
            }
            set {
                myUpdateInterval.SetVal(value);
                if (initDone)
                    timer.Interval = value;
            }
        }

        private CCfgString mySkinName;
        /// <summary>
        /// The name of the currently used skin. Can be changed
        /// </summary>
        public string SkinName {
            get {
                return mySkinName.GetVal();
            }
            set {
                mySkinName.SetVal(value);
                if (initDone) {
                    try {
                        loadSkin(value);
                    } catch (Exception e) {
                        skin = null;
                        System.Windows.Forms.MessageBox.Show("foo_title - There was an error loading skin " + value + ":\n" + e.Message, "foo_title");
                    }
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

        private CCfgString myAlbumArtFilenames;
        /// <summary>
        /// A semicolon separated list of possible filenames (without extension) for album art. 
        /// May contain formatting strings.
        /// </summary>
        public string AlbumArtFilenames {
            get {
                return myAlbumArtFilenames.GetVal();
            }
            set {
                myAlbumArtFilenames.SetVal(value);
            }
        }

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
                return myDisplay;
            }
        }

        private CCfgInt myShowWhen;
        /// <summary>
        /// When to show foo_title: Always, never or only when foobar is minimized
        /// </summary>
        public ShowWhenEnum ShowWhen {
            get {
                return (ShowWhenEnum)myShowWhen.GetVal();
            }
            set {
                myShowWhen.SetVal((int)value);

                if (value == ShowWhenEnum.Always)
                    EnableFooTitle();
                else if (value == ShowWhenEnum.Never)
                    DisableFooTitle();
                else {
                    checkFoobarMinimized();
                }
            }
        }

        private CCfgInt myNormalOpacity;
        /// <summary>
        /// The opacity in normal state
        /// </summary>
        public int NormalOpacity {
            get {
                return myNormalOpacity.GetVal();
            }
            set {
                myNormalOpacity.SetVal(value);
                if (Display != null)
                    Display.SetNormalOpacity(value);
            }
        }

        /// <summary>
        /// The opacity when the mouse is over foo_title
        /// </summary>
        private CCfgInt myOverOpacity;
        public int OverOpacity {
            get {
                return myOverOpacity.GetVal();
            }
            set {
                myOverOpacity.SetVal(value);
            }
        }

        /// <summary>
        /// The length of fade between normal and over states in miliseconds
        /// </summary>
        private CCfgInt myFadeLength;
        public int FadeLength {
            get {
                return myFadeLength.GetVal();
            }
            set {
                myFadeLength.SetVal(value);
            }
        }

        /// <summary>
        /// The z position of the window - either always on top or on the bottom.
        /// </summary>
        private CCfgInt myWindowPosition;
        public Win32.WindowPosition WindowPosition {
            get {
                return (Win32.WindowPosition)myWindowPosition.GetVal();
            }
            set {
                myWindowPosition.SetVal((int)value);
                Display.SetWindowsPos(value);
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
            if (initDone)
                Display.Show();
        }

        /// <summary>
        /// Checks if foobar is minimized or active and shows/hides display according to it
        /// </summary>
        private void checkFoobarMinimized() {
            if (ShowWhen != ShowWhenEnum.WhenMinimized) return;

            if (CManagedWrapper.getInstance().IsFoobarActivated())
                DisableFooTitle();                
            else
                EnableFooTitle();
        }

        private CCfgInt positionX;
        private CCfgInt positionY;

		System.Windows.Forms.Timer timer;
		
        MetaDBHandle lastSong;
        Properties propsForm;

        // singleton
        static private Main instance = null;
        static public Main GetInstance() {
            return instance;
        }

		private void timerUpdate(object sender, System.EventArgs e) {
            if (fooTitleEnabled) {
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
        /// Loads skin by name
        /// </summary>
        /// <param name="name">The name of the skin's directory (skins are placed in foo_title directory, each skin has it's own directory)</param>
        private void loadSkin(string name) {
            try {
                // delete the old one
                if (skin != null)
                    skin.Free();
                skin = null;  

                skin = new Skin(name);
                skin.Init(Display);
                
                // need to tell it about the currently playing song
                if (lastSong != null)
                    skin.OnPlaybackNewTrack(lastSong);
                else
                    skin.OnPlaybackStop(IPlayControl.StopReason.stop_reason_user);

                skin.CheckSize();
            } catch (Exception e) {
                if (skin != null)
                    skin.Free();
                skin = null;
                System.Windows.Forms.MessageBox.Show(
                    String.Format("foo_title - There was an error loading skin {0}:\n {1} \n {2}", name, e.Message, e.ToString() )
                    , "foo_title");
            }
        }
		
        /// <summary>
        /// Creates foobar2000 objects that need to be created before component's initialisation ends
        /// </summary>
        public void Create() {
            instance = this;
            positionX = new CCfgInt(new Guid(457, 784, 488, 36, 48, 79, 54, 12, 36, 47, 5), 100);
            positionY = new CCfgInt(new Guid(457, 784, 488, 36, 48, 79, 54, 12, 36, 47, 6), 100);
            myShowWhen = new CCfgInt(new Guid(457, 784, 488, 36, 48, 79, 54, 12, 36, 47, 7), 1);
            myUpdateInterval = new CCfgInt(new Guid(457, 784, 488, 36, 48, 79, 54, 12, 36, 47, 59), 100);
            mySkinName = new CCfgString(new Guid(457, 784, 488, 36, 48, 79, 54, 12, 36, 47, 2), "white");
            myDataDir = new CCfgString(new Guid(457, 784, 488, 36, 48, 79, 54, 12, 36, 47, 1), "foo_title\\");
            myAlbumArtFilenames = new CCfgString(new Guid(457, 784, 488, 36, 48, 79, 54, 12, 36, 47, 4), "folder");
            myNormalOpacity = new CCfgInt(new Guid(457, 784, 488, 36, 48, 79, 54, 12, 36, 47, 8), 255);
            myOverOpacity = new CCfgInt(new Guid(457, 784, 488, 36, 48, 79, 54, 12, 36, 47, 9), 120);
            myFadeLength = new CCfgInt(new Guid(457, 784, 488, 36, 48, 79, 54, 12, 36, 47, 10), 500);
            myWindowPosition = new CCfgInt(new Guid(457, 784, 488, 36, 48, 79, 54, 12, 36, 47, 11), (int)Win32.WindowPosition.Topmost);
            propsForm = new Properties(this);
        }

        public void SavePosition() {
            positionX.SetVal(Display.Left);
            positionY.SetVal(Display.Top);
        }

        public void RestorePosition() {
            Display.Left = positionX.GetVal();
            Display.Top = positionY.GetVal();
        }

        #region Events
        public event OnInitDelegate OnInitEvent;

        /// <summary>
        /// Called by init_quit, creates form, loads skin,...
        /// </summary>
		public void OnInit(IPlayControl a) {
            Main.PlayControl = a;

            propsForm.UpdateValues();

			// init registered clients
            OnInitDelegate temp = OnInitEvent;
            if (temp != null)
                temp();

			// start a timer updating the display
			timer = new System.Windows.Forms.Timer();
            timer.Interval = myUpdateInterval.GetVal();
			timer.Tick += new System.EventHandler(timerUpdate);
			timer.Enabled = true;

            // create layer factory
            myLayerFactory = new LayerFactory();
            myLayerFactory.SearchAssembly(System.Reflection.Assembly.GetExecutingAssembly());

            // create geometry factory
            myGeometryFactory = new GeometryFactory();
            myGeometryFactory.SearchAssembly(System.Reflection.Assembly.GetExecutingAssembly());

			// initialize the form displaying the images
            myDisplay = new Display(300, 22);
			myDisplay.Show();

            // load the skin
            loadSkin(SkinName);

            RestorePosition();

            if (ShowWhen == ShowWhenEnum.Never)
                DisableFooTitle();

            initDone = true;

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

		public void OnPlaybackNewTrack(fooManagedWrapper.MetaDBHandle song) {
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
                fooManagedWrapper.Console.Error(e.ToString());
            }
        }

        public void OnPlaybackPause(bool state) {
            sendEvent(OnPlaybackPauseEvent, state);
        }

        #endregion
    }
}
