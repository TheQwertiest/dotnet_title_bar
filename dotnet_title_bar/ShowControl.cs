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

using fooTitle.Config;
using Qwr.ComponentInterface;
using System;
using System.Windows.Forms;

namespace fooTitle
{
    enum EnableWhenEnum
    {
        Always,
        WhenMinimized,
        Never
    }
    enum ShowWhenEnum
    {
        Always,
        OnTrigger
    }

    /// <summary>
    /// This class controls when to show and when to hide foo_title. There can be several
    /// options like show only on song start when foobar is minimized and such.
    ///
    /// It receives events and shows/hides foo_title by itself, so it does not need any external
    /// agent to activate and works completely independently on other classes.
    /// </summary>
    public class ShowControl
    {
        private readonly ConfEnum<EnableWhenEnum> _enableWhen = Configs.ShowControl_EnableWhen;
        private readonly ConfEnum<ShowWhenEnum> _showWhen = Configs.ShowControl_ShowPopupWhen;
        private readonly ConfBool _onSongStart = Configs.ShowControl_ShouldShow_OnSongStart;
        private readonly ConfBool _beforeSongEnds = Configs.ShowControl_ShouldShow_OnSongEnd;
        private readonly ConfInt _onSongStartStay = Configs.ShowControl_StayDelay_OnSongStart;
        private readonly ConfInt _beforeSongEndsStay = Configs.ShowControl_StayDelay_OnSongEnd;
        private readonly ConfBool _showWhenNotPlaying = Configs.ShowControl_ShouldShow_WhenNotPlaying;
        private readonly ConfInt _timeBeforeFade = Configs.ShowControl_StayDelay_OnPeek;

        private readonly Timer _hideAfterSongStart = new();
        private readonly Timer _reshowWhenMinimizedTimer = new() { Interval = 100 };

        private bool _reachedEndSat = false;
        private bool _newSongSat = false;

        private readonly Main _main;

        private IMetadbHandle _lastSong;
        private IFileInfo _lastFileInfo;

        public ShowControl()
        {
            _main = Main.GetInstance();

            _main.PlaybackAdvancedToNewTrack += Main_OnPlaybackNewTrackEvent;
            _main.DynamicTrackInfoChanged += Main_OnPlaybackDynamicInfoTrackEvent;
            _main.PlaybackPausedStateChanged += Main_OnPlaybackPauseEvent;
            _main.PlaybackStopped += Main_OnPlaybackStopEvent;
            _main.TrackPlaybackPositionChanged += Main_OnPlaybackTimeEvent;

            // react to settings change
            _showWhen.Changed += ShowWhen_ChangedEventHandler;
            _showWhenNotPlaying.Changed += ShowWhenNotPlaying_ChangedEventHandler;
            _enableWhen.Changed += EnableWhen_ChangedEventHandler;

            // init the timers
            _hideAfterSongStart.Tick += HideAfterSongStart_TickEventHandler;

            _reshowWhenMinimizedTimer.Tick += ReshowWhenMinimizedTimer_TickEventHandler;
        }

        /// <summary>
        /// Init popup state
        /// </summary>
        public void InitializeState()
        {
            DoEnable();
        }

        /// <summary>
        /// When the showing option changes, check the situation and disable/enable foo_title as needed
        /// </summary>
        private void ShowWhen_ChangedEventHandler(string name)
        {
            ShowByCriteria();
        }

        private void ShowWhenNotPlaying_ChangedEventHandler(string name)
        {
            ShowByCriteria();
        }

        private void EnableWhen_ChangedEventHandler(string name)
        {
            _reshowWhenMinimizedTimer.Stop();
            switch (_enableWhen.Value)
            {
                case EnableWhenEnum.Always:
                    ShowByCriteria();
                    break;
                case EnableWhenEnum.Never:
                    DoDisableWithAnimation();
                    break;
                case EnableWhenEnum.WhenMinimized:
                    DoDisableWithAnimation();
                    _reshowWhenMinimizedTimer.Start();
                    break;
                default:
                    throw new Exception("Internal error: unexpected `enable when` value");
            }
        }

        /// <summary>
        /// We don't have callback on foobar minimized state, so we have to update manually
        /// </summary>
        private void ReshowWhenMinimizedTimer_TickEventHandler(object sender, EventArgs e)
        {
            if (_showWhen.Value == ShowWhenEnum.Always || NotPlayingSat())
            {
                if (Main.GetInstance().Fb2kUtils.IsFb2kMinimized())
                {
                    StartTriggerAnimation(false);
                }
            }
        }

        private void HideAfterSongStart_TickEventHandler(object sender, EventArgs e)
        {
            ShowByCriteria();
            _hideAfterSongStart.Stop();
        }

        /// <summary>
        /// This enables foo_title, but only if it's enabled in the preferences
        /// Should be called from functions that check if it's time to show
        /// </summary>
        private void DoEnable()
        {
            switch (_enableWhen.Value)
            {
                case EnableWhenEnum.WhenMinimized:
                    // can show only if minimized
                    if (Main.GetInstance().Fb2kUtils.IsFb2kMinimized())
                    {
                        _main.EnableTitleBar();
                    }

                    break;
                case EnableWhenEnum.Never:
                    // nothing
                    break;
                case EnableWhenEnum.Always:
                    _main.EnableTitleBar();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DoDisable()
        {
            // can always hide
            _main.DisableTitleBar();
        }

        private void StartTriggerAnimation(bool useOverAnimation)
        {
            DoEnable();
            AnimationManager.Animation animName =
                useOverAnimation ? AnimationManager.Animation.FadeInOver : AnimationManager.Animation.FadeInNormal;
            _main.StartDisplayAnimation(animName);
        }

        private void EndTriggerAnimation()
        {
            AnimationManager.Animation animName =
                (_showWhen.Value == ShowWhenEnum.Always) ? AnimationManager.Animation.FadeOut : AnimationManager.Animation.FadeOutFull;
            AnimationManager.AnimationStoppedEventHandler onStop =
                (_showWhen.Value != ShowWhenEnum.Always) ? DoDisable : (AnimationManager.AnimationStoppedEventHandler)null;
            _main.StartDisplayAnimation(animName, onStop);
        }

        private void DoDisableWithAnimation()
        {
            _main.StartDisplayAnimation(AnimationManager.Animation.FadeOutFull, DoDisable);
        }

        public void TriggerPopup()
        {
            DoEnable();
            _main.StartDisplayAnimation(AnimationManager.Animation.FadeInOver, StartFadeOutTimer);
        }

        public void TogglePopup()
        {
            if (_enableWhen.Value == EnableWhenEnum.Always)
            {
                _enableWhen.Value = EnableWhenEnum.Never;
            }
            else
            {
                _enableWhen.Value = EnableWhenEnum.Always;
            }
        }

        private void StartFadeOutTimer()
        {
            // plan a hide event
            _hideAfterSongStart.Interval = _timeBeforeFade.Value * 1000;
            _hideAfterSongStart.Stop(); // without this the timer is not reset and fires in the old planned time
            _hideAfterSongStart.Start();
        }

        /// <summary>
        /// Checks time and displays foo_title when time has come
        /// </summary>
        private void Main_OnPlaybackTimeEvent(double time)
        {
            if (_lastSong == null)
            {
                return;
            }

            // streams
            if (_lastSong.Length() <= 0)
            {
                return;
            }

            if (_onSongStart.Value && time < _onSongStartStay.Value)
            {
                if (!_newSongSat)
                { // We don't want to trigger animation every time
                    StartTriggerAnimation(true);
                    _newSongSat = true;
                }
            }
            else if (_beforeSongEnds.Value && (_lastSong.Length() - _beforeSongEndsStay.Value <= time))
            {
                if (!_reachedEndSat)
                { // We don't want to trigger animation every time
                    StartTriggerAnimation(true);
                    _reachedEndSat = true;
                }
            }
            else if (!_hideAfterSongStart.Enabled)
            { // Do not skip wait event
                _newSongSat = false;
                _reachedEndSat = false;
                EndTriggerAnimation();
            }
        }

        /// <summary>
        /// Displays foo_title when it's set to display on new song and also hides foo_title if not set
        /// </summary>
        private void Main_OnPlaybackNewTrackEvent(IMetadbHandle song)
        {
            // store the song
            _lastSong = song;
            _reachedEndSat = false;
            _newSongSat = false;

            if (!_onSongStart.Value)
            {
                return;
            }

            StartTriggerAnimation(true);
        }

        /// <summary>
        /// Handles the dynamic info change. Checks if the file info is different from
        /// the last one and if it is, shows foo_title. Used for displaying on
        /// stream title change.
        /// </summary>
        private void Main_OnPlaybackDynamicInfoTrackEvent(IFileInfo fileInfo)
        {
            // if not stored yet, show
            if (_lastFileInfo == null)
            {
                Main_OnPlaybackNewTrackEvent(_lastSong);
                _lastFileInfo = fileInfo;
                return;
            }

            _lastFileInfo = fileInfo;
            Main_OnPlaybackNewTrackEvent(_lastSong);
        }

        private void Main_OnPlaybackStopEvent(PlaybackStopReason reason)
        {
            ShowByCriteria();
        }

        private void Main_OnPlaybackPauseEvent(bool state)
        {
            if (state)
            {
                // paused, leave hiding it for a later time
                _hideAfterSongStart.Stop();
            }
            ShowByCriteria();
        }

        /// <summary>
        /// Returns true if foo_title should be shown according to the timing criteria - n seconds after song start
        /// and that criteria is enabled.
        /// </summary>
        private bool SongStartSat()
        {
            var pc = Main.GetInstance().Fb2kPlaybackControls;
            if (!_onSongStart.Value || !pc.IsPlaying() || pc.IsPaused())
            {
                return false;
            }
            return (pc.TrackPlaybackPosition() < _onSongStartStay.Value);
        }

        /// <summary>
        /// Returns true if it should be shown before song end and that criteria is enabled.
        /// </summary>
        /// <returns></returns>
        private bool BeforeSongEndSat()
        {
            var pc = Main.GetInstance().Fb2kPlaybackControls;
            if (!_beforeSongEnds.Value || !pc.IsPlaying() || pc.IsPaused())
            {
                return false;
            }
            return _lastSong != null && (_lastSong.Length() - _beforeSongEndsStay.Value <= pc.TrackPlaybackPosition());
        }

        /// <summary>
        /// Returns true if foo_title is displayed because nothing is playing and showWhenNotPlaying is enabled.
        /// </summary>
        private bool NotPlayingSat()
        {
            if (!_showWhenNotPlaying.Value)
            {
                return false;
            }
            var pc = Main.GetInstance().Fb2kPlaybackControls;
            return (!pc.IsPlaying() || pc.IsPaused());
        }

        /// <summary>
        /// Evaluates current state of criteria and shows or hides foo_title.
        /// Should not be used in very frequently used callbacks such as OnPlaybackTimeEvent.
        /// </summary>
        private void ShowByCriteria()
        {
            if (_showWhen.Value == ShowWhenEnum.Always || NotPlayingSat())
            {
                StartTriggerAnimation(false);
            }
            else if (SongStartSat() || BeforeSongEndSat())
            {
                StartTriggerAnimation(true);
            }
            else
            {
                EndTriggerAnimation();
            }
        }
    }
}
