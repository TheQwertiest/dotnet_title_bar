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


using fooTitle.Config;
using System.Windows.Forms;

namespace fooTitle {
    /// <summary>
    /// This class ensures that the window is placed on top from time to time partially solving windows's bug
    /// that always on top windows sometimes fall down.
    /// </summary>
    public class RepeatedShowing {
        private readonly ConfBool _reShowOnTop;
        private readonly Timer _timer = new Timer {Interval = 1000 * 1 * 60};// every minute
        private readonly Main _main;

        public RepeatedShowing() {
            _main = Main.GetInstance();

            this._timer.Tick += timer_Tick;

            this._reShowOnTop = new ConfBool("display/reShowOnTop", true);
            this._reShowOnTop.OnChanged += reShowOnTop_OnChanged;

            _main.Display.WindowPosition.OnChanged += WindowPosition_OnChanged;

            this.updateTimerState();
        }

        private void updateTimerState() {
            if (_reShowOnTop.Value && _main.Display.WindowPosition.Value == Win32.WindowPosition.Topmost) {
                _timer.Start();
            } else {
                _timer.Stop();
            }
        }

        private void WindowPosition_OnChanged(string name) {
            updateTimerState();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            _main.Display?.SetWindowsPos(Win32.WindowPosition.Topmost);
        }

        private void reShowOnTop_OnChanged(string name) {
            updateTimerState();
        }
    }
}
