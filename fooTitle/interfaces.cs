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

using fooManagedWrapper;

namespace fooTitle {

    public delegate void OnPlaybackTimeDelegate(double time);
    public delegate void OnPlaybackNewTrackDelegate(MetaDBHandle song);
    public delegate void OnQuitDelegate();
    public delegate void OnInitDelegate();
    public delegate void OnPlaybackStopDelegate(IPlayControl.StopReason reason);
    public delegate void OnPlaybackPauseDelegate(bool state);


    public interface IPlayCallbackSender {
        event OnPlaybackTimeDelegate OnPlaybackTimeEvent;
        event OnPlaybackNewTrackDelegate OnPlaybackNewTrackEvent;
        event OnPlaybackStopDelegate OnPlaybackStopEvent;
        event OnPlaybackPauseDelegate OnPlaybackPauseEvent;
        event OnQuitDelegate OnQuitEvent;
        event OnInitDelegate OnInitEvent;
    }
}