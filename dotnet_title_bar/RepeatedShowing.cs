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
        private readonly ConfBool _reShowOnTop;
        private readonly Timer _timer = new() { Interval = 1000 * 1 * 60 };// every minute
        private readonly Main _main;

        public RepeatedShowing()
        {
            _main = Main.GetInstance();

            this._timer.Tick += Timer_TickEventHandler;

            this._reShowOnTop = new ConfBool("display/reShowOnTop", true);
            this._reShowOnTop.Changed += ReShowOnTop_OnChangedEventHandler;

            _main.Display.WindowPosition.Changed += WindowPosition_OnChangedEventHandler;

            this.UpdateTimerState();
        }

        private void UpdateTimerState()
        {
            if (_reShowOnTop.Value && _main.Display.WindowPosition.Value == Win32.WindowPosition.Topmost)
            {
                _timer.Start();
            }
            else
            {
                _timer.Stop();
            }
        }

        private void WindowPosition_OnChangedEventHandler(string name)
        {
            UpdateTimerState();
        }

        private void Timer_TickEventHandler(object sender, EventArgs e)
        {
            _main.Display?.SetWindowsPos(Win32.WindowPosition.Topmost);
        }

        private void ReShowOnTop_OnChangedEventHandler(string name)
        {
            UpdateTimerState();
        }
    }
}