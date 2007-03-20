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

        protected fooManagedWrapper.MetaDBHandle lastSong;
        
        public ShowControl() {
            Main.GetInstance().OnPlaybackNewTrackEvent += new OnPlaybackNewTrackDelegate(ShowControl_OnPlaybackNewTrackEvent);

            // I don't want to receive the playback time event when before song ends is not checked
            beforeSongEnds.OnChanged += new ConfValuesManager.ValueChangedDelegate(beforeSongEnds_OnChanged);
            if (beforeSongEnds.Value)
                registerTimeEvent();


            // enable foo_title back when restrictions are turned off
            popupShowing.OnChanged += new ConfValuesManager.ValueChangedDelegate(popupShowing_OnChanged);

            // init the timers
            hideAfterSongStart.Tick += new EventHandler(hideAfterSongStart_Tick);

        }

        /// <summary>
        /// Check the situation and disable/enable foo_title as needed
        /// </summary>
        void popupShowing_OnChanged(string name) {
            if (popupShowing.Value == PopupShowing.AllTheTime)
                doEnable();
            else {
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
        /// </summary>
        protected void doEnable() {
            // TODO tohle musi kontrolovat jestli to neni nastaveny na minimized only a pripadne se ani nesnazit to zobrazit
            // a na druhe strane - checkFoobarMinimized musi brat ohledy na to ze ShowControl to chce mit schovany
            Main.GetInstance().EnableFooTitle();
        }

        protected void doDisable() {
            Main.GetInstance().DisableFooTitle();
        }
        #endregion

        #region time event tools
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
        #endregion

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

        /// <summary>
        /// Checks time and displays foo_title when time has come
        /// </summary>
        void ShowControl_OnPlaybackTimeEvent(double time) {
            if (lastSong.GetLength() - beforeSongEndsStay.Value <= time) {
                doEnable();
            }
        }
        
        /// <summary>
        /// Displays foo_title when it's set to display on new song and also hides foo_title if not set
        /// </summary>
        void ShowControl_OnPlaybackNewTrackEvent(fooManagedWrapper.MetaDBHandle song) {
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

        void hideAfterSongStart_Tick(object sender, EventArgs e) {
            doDisable();
            hideAfterSongStart.Stop();  
        }
    }
}
