using fooTitle.Config;
using System;
using System.Windows.Forms;

namespace fooTitle
{
    /// <summary>
    /// This class ensures that the window is placed on top from time to time partially solving windows's bug
    /// that always on top windows sometimes fall down.
    /// </summary>
    public class RepeatedShowing
    {
        private readonly ConfBool _reShowOnTop = Configs.Display_ShouldRefreshOnTop;
        private readonly ConfEnum<Win32.WindowPosition> _windowPosistion = Configs.Display_WindowPosition;
        private readonly Timer _timer = new() { Interval = 1000 * 1 * 60 }; // every minute
        private readonly SkinForm _display;

        public RepeatedShowing(SkinForm display)
        {
            _display = display;

            _timer.Tick += Timer_Tick_EventHandler;

            _reShowOnTop.Changed += ReShowOnTop_Changed_EventHandler;
            _windowPosistion.Changed += WindowPosition_Changed_EventHandler;

            UpdateTimerState();
        }

        private void UpdateTimerState()
        {
            if (_reShowOnTop.Value && _windowPosistion.Value == Win32.WindowPosition.Topmost)
            {
                _timer.Start();
            }
            else
            {
                _timer.Stop();
            }
        }

        private void WindowPosition_Changed_EventHandler(string name)
        {
            UpdateTimerState();
        }

        private void Timer_Tick_EventHandler(object sender, EventArgs e)
        {
            _display.SetWindowsPos(Win32.WindowPosition.Topmost);
        }

        private void ReShowOnTop_Changed_EventHandler(string name)
        {
            UpdateTimerState();
        }
    }
}
