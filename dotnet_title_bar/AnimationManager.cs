using fooTitle.Config;
using System;
using System.Windows.Forms;

namespace fooTitle
{
    public enum FadeAnimation
    {
        FadeInNormal,
        FadeInOver,
        FadeOut,
        FadeOutFull
    }

    public class AnimationManager
    {
        private readonly SkinForm _display;

        private readonly ConfInt _normalOpacity = Configs.Display_NormalOpacity;
        private readonly ConfInt _overOpacity = Configs.Display_MouseOverOpacity;

        private Action _mouseOverSavedCallback;
        private Action _onAnimationStopCallback;

        private readonly object _animationLock = new();
        private readonly Timer _animationTimer;
        private Fade _fadeAnimation;
        private OpacityFallbackType _opacityFallbackType = OpacityFallbackType.Normal;
        private FadeAnimation _curAnimationName;
        private bool _mouseIn;

        public AnimationManager(SkinForm display)
        {
            _display = display;

            _display.MouseEnter += Display_MouseEnter_EventHandler;
            _display.MouseLeave += Display_MouseLeave_EventHandler;
            _animationTimer = new Timer { Interval = 33 };
            _animationTimer.Tick += AnimationTimer_Tick_EventHandler;
        }

        /// <summary>
        ///     Clean up any resources being used.
        /// </summary>
        public void Dispose()
        {
            _animationTimer.Stop();
        }

        private enum OpacityFallbackType
        {
            Normal,
            Transparent
        }

        public void StartAnimation(FadeAnimation animName, Action actionAfterAnimation = null,
                                   bool forceAnimation = false)
        {
            lock (_animationLock)
            {
                if (_curAnimationName != animName)
                {
                    _onAnimationStopCallback = null;
                    _fadeAnimation = null;
                }

                if (!_mouseIn)
                {
                    _onAnimationStopCallback = actionAfterAnimation;
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
                    case FadeAnimation.FadeInNormal:
                        _fadeAnimation = new Fade(_display.CurrentOpacity, _normalOpacity.Value, 100 /*fadeLength.Value*/);
                        _opacityFallbackType = OpacityFallbackType.Normal;
                        break;
                    case FadeAnimation.FadeInOver:
                        _fadeAnimation = new Fade(_display.CurrentOpacity, _overOpacity.Value, 100 /*fadeLength.Value*/);
                        break;
                    case FadeAnimation.FadeOut:
                        _fadeAnimation = new Fade(_display.CurrentOpacity, _normalOpacity.Value, 400 /*fadeLength.Value*/);
                        _opacityFallbackType = OpacityFallbackType.Normal;
                        break;
                    case FadeAnimation.FadeOutFull:
                        _fadeAnimation = new Fade(_display.CurrentOpacity, 0, 400 /*fadeLength.Value*/);
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
                    _display.CurrentOpacity = _fadeAnimation.GetOpacity();
                }
            }
        }

        private void AnimationTimer_Tick_EventHandler(object sender, EventArgs e)
        {
            int prevOpacity = _display.CurrentOpacity;
            lock (_animationLock)
            {
                if (_fadeAnimation != null)
                {
                    _display.CurrentOpacity = _fadeAnimation.GetOpacity();
                    if (_fadeAnimation.Done())
                    {
                        _animationTimer.Stop();
                        _fadeAnimation = null;
                        _onAnimationStopCallback?.Invoke();
                        _onAnimationStopCallback = null;
                    }
                }
            }
            if (prevOpacity != _display.CurrentOpacity)
            {
                Main.Get().RedrawTitleBar(true);
            }
        }

        private void Display_MouseLeave_EventHandler(object sender, EventArgs e)
        {
            _mouseIn = false;
            var animName = _opacityFallbackType == OpacityFallbackType.Normal
                               ? FadeAnimation.FadeOut
                               : FadeAnimation.FadeOutFull;
            StartAnimation(animName, _mouseOverSavedCallback);
            _mouseOverSavedCallback = null;
        }

        private void Display_MouseEnter_EventHandler(object sender, EventArgs e)
        {
            _mouseOverSavedCallback = _onAnimationStopCallback;
            _mouseIn = true;
            StartAnimation(FadeAnimation.FadeInOver, forceAnimation: true);
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
