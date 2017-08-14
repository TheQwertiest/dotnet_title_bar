using System;
using System.Windows.Forms;
using fooTitle.Config;

namespace fooTitle
{
    public class AnimationManager
    {
        public delegate void OnAnimationStopDelegate();

        public enum Animation
        {
            FadeInNormal,
            FadeInOver,
            FadeOut,
            FadeOutFull
        }

        private readonly Display _display;

        /// <summary>
        ///     The opacity in normal state
        /// </summary>
        private readonly ConfInt _normalOpacity = ConfValuesManager.CreateIntValue("display/normalOpacity", 255, 5, 255);

        /// <summary>
        ///     The opacity when the mouse is over foo_title
        /// </summary>
        private readonly ConfInt _overOpacity = ConfValuesManager.CreateIntValue("display/overOpacity", 255, 5, 255);

        private OnAnimationStopDelegate _mouseOverSavedCallback;
        private OnAnimationStopDelegate _onAnimationStopEvent;

        private readonly object _animationLock = new object();
        private readonly Timer _animationTimer;
        private Fade _fadeAnimation;
        private OpacityFallbackType _opacityFallbackType = OpacityFallbackType.Normal;
        private Animation _curAnimationName;
        private bool _mouseIn;

        public AnimationManager(Display display)
        {
            _display = display;

            _display.MouseEnter += Display_MouseEnter;
            _display.MouseLeave += Display_MouseLeave;
            _animationTimer = new Timer {Interval = 50};
            _animationTimer.Tick += AnimationTimer_Tick;
        }

        /// <summary>
        ///     Clean up any resources being used.
        /// </summary>
        public void Dispose()
        {
            _animationTimer.Stop();
        }

        public void StartAnimation(Animation animName, OnAnimationStopDelegate actionAfterAnimation = null,
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
                    _animationTimer.Stop();

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
                    _display.MyOpacity = _fadeAnimation.GetOpacity();
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
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
                Main.GetInstance().RequestRedraw();
        }

        private void Display_MouseLeave(object sender, EventArgs e)
        {
            Animation animName = _opacityFallbackType == OpacityFallbackType.Normal
                ? Animation.FadeOut
                : Animation.FadeOutFull;
            StartAnimation(animName, _mouseOverSavedCallback);
            _mouseOverSavedCallback = null;
            _mouseIn = false;
        }

        private void Display_MouseEnter(object sender, EventArgs e)
        {
            _mouseOverSavedCallback = _onAnimationStopEvent;
            _mouseIn = true;
            StartAnimation(Animation.FadeInOver, forceAnimation: true);
        }

        private enum OpacityFallbackType
        {
            Normal,
            Transparent
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
                    return _startVal;
                if (_length == 0)
                    _phase = 1; // end it now

                // normal processing
                _phase = (now - _startTime) / (float) _length;
                if (_phase > 1)
                    _phase = 1;
                return (int) (_startVal + _phase * (_stopVal - _startVal));
            }

            public bool Done()
            {
                return _phase >= 1;
            }
        }
    }
}