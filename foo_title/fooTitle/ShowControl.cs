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
        public enum EnableWhenEnum
        {
            Always,
            WhenMinimized,
            Never
        }
        public enum ShowWhenEnum {
            Always,
            OnTrigger
        }

        private readonly ConfEnum<ShowWhenEnum> showWhen = new ConfEnum<ShowWhenEnum>("showControl/popupShowing", ShowWhenEnum.Always);
        private readonly ConfBool onSongStart = new ConfBool("showControl/onSongStart", true);
        private readonly ConfBool beforeSongEnds = new ConfBool("showControl/beforeSongEnds", true);
        private readonly ConfInt onSongStartStay = new ConfInt("showControl/onSongStartStay", 5, 0, int.MaxValue);
        private readonly ConfInt beforeSongEndsStay = new ConfInt("showControl/beforeSongEndsStay", 5, 0, int.MaxValue);
        private readonly ConfBool showWhenNotPlaying = new ConfBool("showControl/showWhenNotPlaying", false);
        private readonly ConfInt timeBeforeFade = new ConfInt("showControl/timeBeforeFade", 2, 0, int.MaxValue);
        public ConfEnum<EnableWhenEnum> enableWhen = new ConfEnum<EnableWhenEnum>("display/showWhen", EnableWhenEnum.Always);

        private readonly Timer hideAfterSongStart = new Timer();
        private readonly Timer _reshowWhenMinimizedTimer = new Timer { Interval = 100 };
        private bool reachedEndSat = false;
        private bool newSongSat = false;
        private readonly Main main;

        private fooManagedWrapper.CMetaDBHandle lastSong;

        public ShowControl() {
            main = Main.GetInstance();

            main.OnPlaybackNewTrackEvent += main_OnPlaybackNewTrackEvent;
            main.OnPlaybackDynamicInfoTrackEvent += main_OnPlaybackDynamicInfoTrackEvent;
            main.OnPlaybackPauseEvent += main_OnPlaybackPauseEvent;
            main.OnPlaybackStopEvent += main_OnPlaybackStopEvent;
            main.OnPlaybackTimeEvent += main_OnPlaybackTimeEvent;

            // react to settings change
            showWhen.OnChanged += showWhen_OnChanged;
            showWhenNotPlaying.OnChanged += showWhenNotPlaying_OnChanged;
            enableWhen.OnChanged += enableWhen_OnChanged;

            // init the timers
            hideAfterSongStart.Tick += hideAfterSongStart_Tick;

            _reshowWhenMinimizedTimer.Tick += _reshowWhenMinimizedTimer_Tick;

            // Init popup state
            enableWhen_OnChanged("");
        }

        #region Event handling

        /// <summary>
        /// When the showing option changes, check the situation and disable/enable foo_title as needed
        /// </summary>
        private void showWhen_OnChanged(string name) {
            showByCriteria();
        }

        private void showWhenNotPlaying_OnChanged(string name) {
            showByCriteria();
        }

        private void enableWhen_OnChanged(string name)
        {
            _reshowWhenMinimizedTimer.Stop();
            switch (enableWhen.Value)
            {
                case EnableWhenEnum.Always:
                    showByCriteria();
                    break;
                case EnableWhenEnum.Never:
                    DoDisableWithAnimation();
                    break;
                case EnableWhenEnum.WhenMinimized:
                    DoDisableWithAnimation();
                    _reshowWhenMinimizedTimer.Start();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// We don't have callback on foobar minimized state, so we have to update manually
        /// </summary>
        private void _reshowWhenMinimizedTimer_Tick(object sender, System.EventArgs e)
        {
            if (showWhen.Value == ShowWhenEnum.Always || notPlayingSat())
            {
                if (!CManagedWrapper.getInstance().IsFoobarActivated())
                    StartTriggerAnimation(false);
            }
        }

        private void hideAfterSongStart_Tick(object sender, EventArgs e)
        {
            showByCriteria();
            hideAfterSongStart.Stop();
        }
        #endregion //Event handling

        #region Showing and hiding functions
        /// <summary>
        /// This enables foo_title, but only if it's enabled in the preferences
        /// Should be called from functions that check if it's time to show
        /// </summary>
        private void DoEnable()
        {
            switch (enableWhen.Value)
            {
                case EnableWhenEnum.WhenMinimized:
                    // can show only if minimized
                    if (!CManagedWrapper.getInstance().IsFoobarActivated())
                        main.EnableFooTitle();
                    break;
                case EnableWhenEnum.Never:
                    // nothing
                    break;
                case EnableWhenEnum.Always:
                    main.EnableFooTitle();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DoDisable()
        {
            // can always hide
            main.DisableFooTitle();
        }

        private void StartTriggerAnimation(bool useOverAnimation)
        {
            DoEnable();
            AnimationManager.Animation animName = 
                useOverAnimation ? AnimationManager.Animation.FadeInOver : AnimationManager.Animation.FadeInNormal;
            main.StartDisplayAnimation(animName);
        }

        private void EndTriggerAnimation()
        {
            AnimationManager.Animation animName = 
                (showWhen.Value == ShowWhenEnum.Always) ? AnimationManager.Animation.FadeOut : AnimationManager.Animation.FadeOutFull;
            AnimationManager.OnAnimationStopDelegate onStop = 
                (showWhen.Value != ShowWhenEnum.Always) ? DoDisable : (AnimationManager.OnAnimationStopDelegate)null;
            main.StartDisplayAnimation(animName, onStop);
        }

        private void DoDisableWithAnimation()
        {
            main.StartDisplayAnimation(AnimationManager.Animation.FadeOutFull, DoDisable);
        }

        public void TriggerPopup()
        {
            DoEnable();
            main.StartDisplayAnimation(AnimationManager.Animation.FadeInOver, StartFadeOutTimer);
        }

        public void TogglePopup()
        {
            if (enableWhen.Value == EnableWhenEnum.Always)
                enableWhen.Value = EnableWhenEnum.Never;
            else
                enableWhen.Value = EnableWhenEnum.Always;
        }

        private void StartFadeOutTimer()
        {
            // plan a hide event
            hideAfterSongStart.Interval = timeBeforeFade.Value * 1000;
            hideAfterSongStart.Stop(); // without this the timer is not reset and fires in the old planned time
            hideAfterSongStart.Start();
        }
        #endregion

        #region Main event handling
        /// <summary>
        /// Checks time and displays foo_title when time has come
        /// </summary>
        private void main_OnPlaybackTimeEvent(double time) {
            if (lastSong == null)
                return;

            // streams
            if (lastSong.GetLength() <= 0)
                return;

            if (onSongStart.Value && time < onSongStartStay.Value)
            {
                if (!newSongSat)
                {// We don't want to trigger animation every time
                    StartTriggerAnimation(true);
                    newSongSat = true;
                }
            }
            else if (beforeSongEnds.Value && (lastSong.GetLength() - beforeSongEndsStay.Value <= time ) )
            {
                if (!reachedEndSat)
                {// We don't want to trigger animation every time
                    StartTriggerAnimation(true);
                    reachedEndSat = true;
                }
            }
            else if (!hideAfterSongStart.Enabled)
            {// Do not skip wait event
                newSongSat = false;
                reachedEndSat = false;
                EndTriggerAnimation();
            }
        }

        /// <summary>
        /// Displays foo_title when it's set to display on new song and also hides foo_title if not set
        /// </summary>
        private void main_OnPlaybackNewTrackEvent(fooManagedWrapper.CMetaDBHandle song) {
            // store the song
            lastSong = song;
            reachedEndSat = false;
            newSongSat = false;

            if (!onSongStart.Value)
            {
                return;
            }

            StartTriggerAnimation(true);
        }

        private FileInfo lastFileInfo;

        /// <summary>
        /// Handles the dynamic info change. Checks if the file info is different from
        /// the last one and if it is, shows foo_title. Used for displaying on
        /// stream title change.
        /// </summary>
        private void main_OnPlaybackDynamicInfoTrackEvent(FileInfo fileInfo) {
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


        private void main_OnPlaybackStopEvent(IPlayControl.StopReason reason) {
            showByCriteria();
        }

        private void main_OnPlaybackPauseEvent(bool state) {
            if (state) {
                // paused, leave hiding it for a later time
                hideAfterSongStart.Stop();
            }
            showByCriteria();
        }

        #endregion

        #region Criteria satisfaction queries

        /// <summary>
        /// Returns true if foo_title should be shown according to the timing criteria - n seconds after song start
        /// and that criteria is enabled.
        /// </summary>
        protected bool songStartSat() {
            IPlayControl pc = Main.PlayControl;
            if (!onSongStart.Value || !pc.IsPlaying() || pc.IsPaused())
            {
                return false;
            }
            return (pc.PlaybackGetPosition() < onSongStartStay.Value);
        }

        /// <summary>
        /// Returns true if it should be shown before song end and that criteria is enabled.
        /// </summary>
        /// <returns></returns>
        protected bool beforeSongEndSat() {
            IPlayControl pc = Main.PlayControl;
            if (!beforeSongEnds.Value || !pc.IsPlaying() || pc.IsPaused())
            {
                return false;
            }            
            return lastSong != null && (lastSong.GetLength() - beforeSongEndsStay.Value <= pc.PlaybackGetPosition());
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
            if (showWhen.Value == ShowWhenEnum.Always || notPlayingSat())
            {
                StartTriggerAnimation(false);
            }
            else if (songStartSat() || beforeSongEndSat())
            {
                StartTriggerAnimation(true);
            }
            else
            {
                EndTriggerAnimation();
            }
        }

        #endregion
    }
}
