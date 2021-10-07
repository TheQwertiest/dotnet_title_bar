/*
*  This file is part of foo_title.
*  Copyright 2017 TheQwertiest (https://github.com/TheQwertiest/foo_title)
*  
*  This library is free software; you can redistribute it and/or
*  modify it under the terms of the GNU Lesser General Public
*  License as published by the Free Software Foundation; either
*  version 2.1 of the License, or (at your option) any later version.
*  
*  This library is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  
*  
*  See the file COPYING included with this distribution for more
*  information.
*/

using fooTitle.Config;
using System;
using System.Windows.Forms;

namespace fooTitle
{
    public class AnimationManager
    {
        private readonly Display _display;

        private readonly ConfInt _normalOpacity = Configs.Display_NormalOpacity;
        private readonly ConfInt _overOpacity = Configs.Display_MouseOverOpacity;

        private AnimationStoppedEventHandler _mouseOverSavedCallback;
        private AnimationStoppedEventHandler _onAnimationStopEvent;

        private readonly object _animationLock = new();
        private readonly Timer _animationTimer;
        private Fade _fadeAnimation;
        private OpacityFallbackType _opacityFallbackType = OpacityFallbackType.Normal;
        private Animation _curAnimationName;
        private bool _mouseIn;

        public AnimationManager(Display display)
        {
            _display = display;

            _display.MouseEnter += Display_MouseEnterEventHandler;
            _display.MouseLeave += Display_MouseLeaveEventHandler;
            _animationTimer = new Timer { Interval = 33 };
            _animationTimer.Tick += AnimationTimer_TickEventHandler;
        }

        /// <summary>
        ///     Clean up any resources being used.
        /// </summary>
        public void Dispose()
        {
            _animationTimer.Stop();
        }

        public delegate void AnimationStoppedEventHandler();

        public enum Animation
        {
            FadeInNormal,
            FadeInOver,
            FadeOut,
            FadeOutFull
        }

        private enum OpacityFallbackType
        {
            Normal,
            Transparent
        }

        public void StartAnimation(Animation animName, AnimationStoppedEventHandler actionAfterAnimation = null,
                                   bool forceAnimation = false)
        {
            lock (_animationLock)
            {
                if (_curAnimationName != animName)
                {
                    _onAnimationStopEvent = null;
                    _fadeAnimation = null;
                }

                if (!_mouseIn)
                {
                    _onAnimationStopEvent = actionAfterAnimation;
                }
                else if (!forceAnimation)
                {
                    _mouseOverSavedCallback = actionAfterAnimation;
                    return;
                }

                if (_animationTimer.Enabled)
                {
                    _animationTimer.Stop();
                }

                switch (animName)
                {
                    case Animation.FadeInNormal:
                        _fadeAnimation = new Fade(_display.MyOpacity, _normalOpacity.Value, 100 /*fadeLength.Value*/);
                        _opacityFallbackType = OpacityFallbackType.Normal;
                        break;
                    case Animation.FadeInOver:
                        _fadeAnimation = new Fade(_display.MyOpacity, _overOpacity.Value, 100 /*fadeLength.Value*/);
                        break;
                    case Animation.FadeOut:
                        _fadeAnimation = new Fade(_display.MyOpacity, _normalOpacity.Value, 400 /*fadeLength.Value*/);
                        _opacityFallbackType = OpacityFallbackType.Normal;
                        break;
                    case Animation.FadeOutFull:
                        _fadeAnimation = new Fade(_display.MyOpacity, 0, 400 /*fadeLength.Value*/);
                        _opacityFallbackType = OpacityFallbackType.Transparent;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(animName), animName, null);
                }
                _curAnimationName = animName;

                _animationTimer.Start();
            }
        }

        public void UpdateOpacity()
        {
            lock (_animationLock)
            {
                if (_fadeAnimation != null)
                {
                    _display.MyOpacity = _fadeAnimation.GetOpacity();
                }
            }
        }

        private void AnimationTimer_TickEventHandler(object sender, EventArgs e)
        {
            int prevOpacity = _display.MyOpacity;
            lock (_animationLock)
            {
                if (_fadeAnimation != null)
                {
                    _display.MyOpacity = _fadeAnimation.GetOpacity();
                    if (_fadeAnimation.Done())
                    {
                        _animationTimer.Stop();
                        _fadeAnimation = null;
                        _onAnimationStopEvent?.Invoke();
                        _onAnimationStopEvent = null;
                    }
                }
            }
            if (prevOpacity != _display.MyOpacity)
            {
                Main.GetInstance().RequestRedraw(true);
            }
        }

        private void Display_MouseLeaveEventHandler(object sender, EventArgs e)
        {
            _mouseIn = false;
            Animation animName = _opacityFallbackType == OpacityFallbackType.Normal
                                     ? Animation.FadeOut
                                     : Animation.FadeOutFull;
            StartAnimation(animName, _mouseOverSavedCallback);
            _mouseOverSavedCallback = null;
        }

        private void Display_MouseEnterEventHandler(object sender, EventArgs e)
        {
            _mouseOverSavedCallback = _onAnimationStopEvent;
            _mouseIn = true;
            StartAnimation(Animation.FadeInOver, forceAnimation: true);
        }

        private class Fade
        {
            private readonly int _length;
            private readonly long _startTime;
            private readonly int _startVal;
            private readonly int _stopVal;

            private float _phase;

            public Fade(int startVal, int stopVal, int length)
            {
                _startVal = startVal;
                _stopVal = stopVal;
                _startTime = DateTime.Now.Ticks / 10000;
                _length = length;
            }

            public int GetOpacity()
            {
                long now = DateTime.Now.Ticks / 10000;

                // special cases
                if (now == _startTime)
                {
                    return _startVal;
                }

                if (_length == 0)
                {
                    _phase = 1; // end it now
                }

                // normal processing
                _phase = (now - _startTime) / (float)_length;
                if (_phase > 1)
                {
                    _phase = 1;
                }

                return (int)(_startVal + _phase * (_stopVal - _startVal));
            }

            public bool Done()
            {
                return _phase >= 1;
            }
        }
    }
}
