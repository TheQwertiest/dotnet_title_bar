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
using System.Text;
using System.Windows.Forms;

using fooTitle.Config;
using fooManagedWrapper;

namespace fooTitle {
    /// <summary>
    /// This class controls when to show and when to hide foo_title. There can be several
    /// options like show only on song start when foobar is minimized and such.
    /// 
    /// It receives events and shows/hides foo_title by itself, so it does not need any external
    /// agent to activate and works completely independently on other classes.
    /// </summary>
    public class ShowControl {
        public enum PopupShowing {
            AllTheTime,
            OnlySometimes
        }

        protected ConfEnum<PopupShowing> popupShowing = new ConfEnum<PopupShowing>("showControl/popupShowing", PopupShowing.AllTheTime);
        protected ConfBool onSongStart = new ConfBool("showControl/onSongStart", true);
        protected ConfBool beforeSongEnds = new ConfBool("showControl/beforeSongEnds", true);
        protected ConfInt onSongStartStay = new ConfInt("showControl/onSongStartStay", 5);
        protected ConfInt beforeSongEndsStay = new ConfInt("showControl/beforeSongEndsStay", 5);
        

        protected Timer hideAfterSongStart = new Timer();
        private bool timeEventRegistered = false;

        /// <summary>
        /// Set to true when it's time when foo_title should be displayed - such as the 5 seconds after the start of playback
        /// </summary>
        protected bool timeToShow;

        protected fooManagedWrapper.CMetaDBHandle lastSong;
        
        public ShowControl() {
            Main.GetInstance().OnPlaybackNewTrackEvent += new OnPlaybackNewTrackDelegate(ShowControl_OnPlaybackNewTrackEvent);
            Main.GetInstance().OnPlaybackDynamicInfoTrackEvent += new OnPlaybackDynamicInfoTrackDelegate(ShowControl_OnPlaybackDynamicInfoTrackEvent);
            // I don't want to receive the playback time event when before song ends is not checked
            beforeSongEnds.OnChanged += new ConfValuesManager.ValueChangedDelegate(beforeSongEnds_OnChanged);
            if (beforeSongEnds.Value)
                registerTimeEvent();

            // enable foo_title back when restrictions are turned off
            popupShowing.OnChanged += new ConfValuesManager.ValueChangedDelegate(popupShowing_OnChanged);

            // init the timers
            hideAfterSongStart.Tick += new EventHandler(hideAfterSongStart_Tick);

            // on start, foo_title should be probably enabled
            doEnable();
        }


        /// <summary>
        /// When the showing option changes, check the situation and disable/enable foo_title as needed
        /// </summary>
        void popupShowing_OnChanged(string name) {
            if (popupShowing.Value == PopupShowing.AllTheTime)
                doEnable();
            else {
                if (!Main.PlayControl.IsPlaying()) {
                    // just hide it
                    doDisable();
                    return;
                }

                // check time
                double pos = Main.PlayControl.PlaybackGetPosition();
                
                if ((pos < onSongStartStay.Value) && (onSongStart.Value)) {
                    showOnStart(pos);
                } else if ((lastSong.GetLength() - beforeSongEndsStay.Value <= pos) && (beforeSongEnds.Value)){
                    doEnable();
                } else {
                    doDisable();
                }
            }
        }

        #region Showing and hiding functions
        /// <summary>
        /// This enables foo_title, but only if it's enabled in the preferences
        /// Should be called from functions that check if it's time to show
        /// </summary>
        protected void doEnable() {
            timeToShow = true;

            if (Main.GetInstance().ShowWhen.Value == ShowWhenEnum.WhenMinimized) {
                // can show only if minimized
                if (!CManagedWrapper.getInstance().IsFoobarActivated())
                    Main.GetInstance().EnableFooTitle();
            } else if (Main.GetInstance().ShowWhen.Value == ShowWhenEnum.Never) {
                // nothing
            } else {
                // can show it freely
                Main.GetInstance().EnableFooTitle();
            }
        }

        protected void doDisable() {
            timeToShow = false;

            // can always hide
            Main.GetInstance().DisableFooTitle();
        }

        /// <summary>
        /// Handles showing foo_title on a start of a new song. Plans the hiding accordingly to the playPos parameter.
        /// </summary>
        /// <param name="playPos">The current playback position in seconds</param>
        protected void showOnStart(double playPos) {
            doEnable();

            // plan a hide event
            hideAfterSongStart.Interval = (int)((float)onSongStartStay.Value - playPos) * 1000;
            hideAfterSongStart.Start();
        }
        #endregion

        #region playback time event tools
        /// <summary>
        /// Registers receiving the playback time event and makes sure that it's not registered multiple times
        /// </summary>
        private void registerTimeEvent() {
            if (timeEventRegistered)
                return;

            Main.GetInstance().OnPlaybackTimeEvent += new OnPlaybackTimeDelegate(ShowControl_OnPlaybackTimeEvent);
            timeEventRegistered = true;
        }

        /// <summary>
        /// Unregisters receiving the playback time event
        /// </summary>
        private void unregisterTimeEvent() {
            if (!timeEventRegistered)
                return;

            Main.GetInstance().OnPlaybackTimeEvent -= new OnPlaybackTimeDelegate(ShowControl_OnPlaybackTimeEvent);
            timeEventRegistered = false;
        }

        /// <summary>
        /// Registers or unregisters the time changed event, in order to prevent wasting performance 
        /// when beforeSongEnds is not enabled
        /// </summary>
        void beforeSongEnds_OnChanged(string name) {
            if (beforeSongEnds.Value) {
                registerTimeEvent();
            } else {
                unregisterTimeEvent();
            }
        }
        #endregion

        #region Event handling
        /// <summary>
        /// Checks time and displays foo_title when time has come
        /// </summary>
        void ShowControl_OnPlaybackTimeEvent(double time) {
            if (lastSong == null)
                return;

            // streams
            if (lastSong.GetLength() <= 0)
                return;

            if (lastSong.GetLength() - beforeSongEndsStay.Value <= time) {
                doEnable();
            }
        }
        
        /// <summary>
        /// Displays foo_title when it's set to display on new song and also hides foo_title if not set
        /// </summary>
        void ShowControl_OnPlaybackNewTrackEvent(fooManagedWrapper.CMetaDBHandle song) {
            // store the song
            lastSong = song;

            if (popupShowing.Value != PopupShowing.OnlySometimes) 
                return;   // no need to do anything
            if (!onSongStart.Value) {
                // hide foo_title when the previous song has ended
                if (beforeSongEnds.Value)
                    doDisable();
                return;
            }

            showOnStart(0);
        }

        FileInfo lastFileInfo;

        /// <summary>
        /// Handles the dynamic info change. Checks if the file info is different from
        /// the last one and if it is, shows foo_title. Used for displaying on
        /// stream title change.
        /// </summary>
        void ShowControl_OnPlaybackDynamicInfoTrackEvent(FileInfo fileInfo) {
            // if not stored yet, show
            if (lastFileInfo == null) {
                ShowControl_OnPlaybackNewTrackEvent(lastSong);
                lastFileInfo = fileInfo;
                return;
            }

            // if different, show
            if (!FileInfo.IsMetaEqual(lastFileInfo, fileInfo)) {
                lastFileInfo = fileInfo;
                ShowControl_OnPlaybackNewTrackEvent(lastSong);
            }
        }

        void hideAfterSongStart_Tick(object sender, EventArgs e) {
            doDisable();
            hideAfterSongStart.Stop();  
        }

        #endregion

        /// <summary>
        /// Called by the Main class when foobar2000's window gets minimized
        /// Should check if foo_title should be enabled
        /// </summary>
        public void TryShowWhenMinimized() {
            if (timeToShow) {
                Main.GetInstance().EnableFooTitle();
            }
        }
    }
}
