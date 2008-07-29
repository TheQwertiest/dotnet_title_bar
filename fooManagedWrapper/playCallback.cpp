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
#include "stdafx.h"
#include "playCallback.h"
#include "utils.h"
#include "ManagedWrapper.h"

using namespace fooManagedWrapper;

namespace fooManagedWrapper {

	CPlayCallback::CPlayCallback() {
		// Register play callback.
		try {
			static_api_ptr_t<play_callback_manager> pcm;
			pcm->register_callback(this,
				play_callback::flag_on_playback_new_track | play_callback::flag_on_playback_time |
				play_callback::flag_on_playback_pause | play_callback::flag_on_playback_stop
				, false);
		}
		catch (const exception_service_not_found &) {
			// play_callback_manager does not exist; something is very wrong.
		}
	}

	void CPlayCallback::on_playback_new_track(metadb_handle_ptr p_track) {
		CMetaDBHandle ^h = gcnew CMetaDBHandle(p_track.get_ptr());
		for each (IComponentClient ^cl in CManagedWrapper::getInstance()) {
			cl->OnPlaybackNewTrack(h);
		}
	}

	void CPlayCallback::on_playback_time(double p_time) {
		for each (IComponentClient ^cl in CManagedWrapper::getInstance()) {
			cl->OnPlaybackTime(p_time);
		}
	}

	void CPlayCallback::on_playback_pause(bool p_state) {
		for each (IComponentClient ^cl in CManagedWrapper::getInstance()) {
			cl->OnPlaybackPause(p_state);
		}
	}

	void CPlayCallback::on_playback_stop(play_control::t_stop_reason reason) {
		for each (IComponentClient ^cl in CManagedWrapper::getInstance()) {
			cl->OnPlaybackStop((IPlayControl::StopReason)reason);
		}
	}

}