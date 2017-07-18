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
        protected ConfInt onSongStartStay = new ConfInt("showControl/onSongStartStay", 5, 0, int.MaxValue);
        protected ConfInt beforeSongEndsStay = new ConfInt("showControl/beforeSongEndsStay", 5, 0, int.MaxValue);
        protected ConfBool showWhenNotPlaying = new ConfBool("showControl/showWhenNotPlaying", false);
        protected ConfInt timeBeforeFade = new ConfInt("showControl/timeBeforeFade", 2, 0, int.MaxValue);


        protected Timer hideAfterSongStart = new Timer();
        private bool reachedEndSat = false;
        private bool newSongSat = false;
        private Main main;



        protected fooManagedWrapper.CMetaDBHandle lastSong;

        public ShowControl() {
            main = Main.GetInstance();

            main.OnPlaybackNewTrackEvent += new OnPlaybackNewTrackDelegate(main_OnPlaybackNewTrackEvent);
            main.OnPlaybackDynamicInfoTrackEvent += new OnPlaybackDynamicInfoTrackDelegate(main_OnPlaybackDynamicInfoTrackEvent);
            main.OnPlaybackPauseEvent += new OnPlaybackPauseDelegate(main_OnPlaybackPauseEvent);
            main.OnPlaybackStopEvent += new OnPlaybackStopDelegate(main_OnPlaybackStopEvent);
            main.OnPlaybackTimeEvent += new OnPlaybackTimeDelegate(ShowControl_OnPlaybackTimeEvent);

            // react to settings change
            popupShowing.OnChanged += new ConfValuesManager.ValueChangedDelegate(popupShowing_OnChanged);
            showWhenNotPlaying.OnChanged += new ConfValuesManager.ValueChangedDelegate(showWhenNotPlaying_OnChanged);

            // init the timers
            hideAfterSongStart.Tick += new EventHandler(hideAfterSongStart_Tick);

            // on start, foo_title should be probably enabled
            DoEnable();
        }

        /// <summary>
        /// When the showing option changes, check the situation and disable/enable foo_title as needed
        /// </summary>
        void popupShowing_OnChanged(string name) {
            showByCriteria();
        }

        void showWhenNotPlaying_OnChanged(string name) {
            showByCriteria();
        }

        #region Showing and hiding functions
        /// <summary>
        /// This enables foo_title, but only if it's enabled in the preferences
        /// Should be called from functions that check if it's time to show
        /// </summary>
        protected void DoEnable() {
            if (main.ShowWhen.Value == ShowWhenEnum.WhenMinimized) {
                // can show only if minimized
                if (!CManagedWrapper.getInstance().IsFoobarActivated())
                    main.EnableFooTitle();
            } else if (main.ShowWhen.Value == ShowWhenEnum.Never) {
                // nothing
            } else {
                // can show it freely
                main.EnableFooTitle();
            }
        }

        protected void DoDisable()
        {
            // can always hide
            main.DisableFooTitle();
        }

        protected void DoEnableWithAnimation(bool useOverAnimation)
        {
            DoEnable();

            if (main.Display != null && main.Display.Visible)
            {
                Display.Animation animName = useOverAnimation ? Display.Animation.FadeInOver : Display.Animation.FadeInNormal;
                main.Display.StartAnimation(animName);
            }
        }

        protected void DoDisableWithAnimation()
        {
            if (main.Display != null && main.Display.Visible)
            {
                Display.Animation animName = (popupShowing.Value == PopupShowing.AllTheTime) ? Display.Animation.FadeOut : Display.Animation.FadeOutFull;
                Display.OnAnimationStopDelegate onStop = (popupShowing.Value != PopupShowing.AllTheTime) ? new Display.OnAnimationStopDelegate(DoDisable) : null;
                main.Display.StartAnimation(animName, onStop);
            }
        }

        public void PopupPeek()
        {
            DoEnable();

            if (main.Display != null && main.Display.Visible)
            {
                main.Display.StartAnimation(Display.Animation.FadeInOver, new Display.OnAnimationStopDelegate(StartFadeOutTimer));
            }
        }

        protected void StartFadeOutTimer()
        {
            // plan a hide event
            hideAfterSongStart.Interval = timeBeforeFade.Value * 1000;
            hideAfterSongStart.Stop(); // without this the timer is not reset and fires in the old planned time
            hideAfterSongStart.Start();
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

            if (onSongStart.Value && time < onSongStartStay.Value)
            {
                if (!newSongSat)
                {// We don't want to trigger animation every time
                    DoEnableWithAnimation(true);
                    newSongSat = true;
                }
            }
            else if (beforeSongEnds.Value && (lastSong.GetLength() - beforeSongEndsStay.Value <= time ) )
            {
                if (!reachedEndSat)
                {// We don't want to trigger animation every time
                    DoEnableWithAnimation(true);
                    reachedEndSat = true;
                }
            }
            else if (!hideAfterSongStart.Enabled)
            {// Do not skip wait event
                newSongSat = false;
                reachedEndSat = false;
                DoDisableWithAnimation();
            }
        }

        /// <summary>
        /// Displays foo_title when it's set to display on new song and also hides foo_title if not set
        /// </summary>
        void main_OnPlaybackNewTrackEvent(fooManagedWrapper.CMetaDBHandle song) {
            // store the song
            lastSong = song;
            reachedEndSat = false;
            newSongSat = false;

            if (!onSongStart.Value)
            {
                return;
            }

            DoEnableWithAnimation(true);
        }

        FileInfo lastFileInfo;

        /// <summary>
        /// Handles the dynamic info change. Checks if the file info is different from
        /// the last one and if it is, shows foo_title. Used for displaying on
        /// stream title change.
        /// </summary>
        void main_OnPlaybackDynamicInfoTrackEvent(FileInfo fileInfo) {
            // if not stored yet, show
            if (lastFileInfo == null) {
                main_OnPlaybackNewTrackEvent(lastSong);
                lastFileInfo = fileInfo;
                return;
            }

            // if different, show
            if (!FileInfo.IsMetaEqual(lastFileInfo, fileInfo)) {
                lastFileInfo = fileInfo;
                main_OnPlaybackNewTrackEvent(lastSong);
            }
        }


        void main_OnPlaybackStopEvent(IPlayControl.StopReason reason) {
            showByCriteria();
        }

        void main_OnPlaybackPauseEvent(bool state) {
            if (state) {
                // paused, leave hiding it for a later time
                hideAfterSongStart.Stop();
            }
            showByCriteria();
        }

        void hideAfterSongStart_Tick(object sender, EventArgs e) {
            showByCriteria();
            hideAfterSongStart.Stop();
        }

        #endregion

        #region Criteria satisfaction queries

        /// <summary>
        /// Returns true if foo_title should be shown according to the timing criteria - n seconds after song start
        /// and that criteria is enabled.
        /// </summary>
        protected bool songStartSat() {
            if (!onSongStart.Value || !Main.PlayControl.IsPlaying())
            {
                return false;
            }
            return (Main.PlayControl.PlaybackGetPosition() < onSongStartStay.Value);
        }

        /// <summary>
        /// Returns true if it should be shown before song end and that criteria is enabled.
        /// </summary>
        /// <returns></returns>
        protected bool beforeSongEndSat() {
            if (!beforeSongEnds.Value)
            {
                return false;
            }
            double pos = Main.PlayControl.PlaybackGetPosition();
            return lastSong != null && (lastSong.GetLength() - beforeSongEndsStay.Value <= pos);
        }

        /// <summary>
        /// Returns true if foo_title is displayed because nothing is playing and showWhenNotPlaying is enabled.
        /// </summary>
        protected bool notPlayingSat() {
            if (!showWhenNotPlaying.Value)
            {
                return false;
            }
            IPlayControl pc = Main.PlayControl;
            return (!pc.IsPlaying() || pc.IsPaused());
        }


        /// <summary>
        /// Evaluates current state of criteria and shows or hides foo_title.
        /// Should not be used in very frequently used callbacks such as OnPlaybackTimeEvent.
        /// </summary>
        protected void showByCriteria() {
            if (popupShowing.Value == PopupShowing.AllTheTime || notPlayingSat())
            {
                DoEnableWithAnimation(false);
            }
            else if (songStartSat() || beforeSongEndSat())
            {
                DoEnableWithAnimation(true);
            }
            else
            {
                DoDisableWithAnimation();
            }
        }

        #endregion

        /// <summary>
        /// Called by the Main class when foobar2000's window gets minimized
        /// Should check if foo_title should be enabled
        /// </summary>
        public void TryShowWhenMinimized() {
            /*
            if (songStartSat() && beforeSongEndSat()) {
                main.EnableFooTitle();
            }*/

            showByCriteria();
        }
    }
}
