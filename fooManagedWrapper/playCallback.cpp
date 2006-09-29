#include "stdafx.h"

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
		MetaDBHandle ^h = gcnew MetaDBHandle(p_track.get_ptr());
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