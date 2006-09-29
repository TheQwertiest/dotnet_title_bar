#pragma once
#include "stdafx.h"
#include <vcclr.h>

namespace fooManagedWrapper {

	// this class relays foobar's play callbacks to the managed components
	class CPlayCallback : public play_callback {
	public:

		CPlayCallback();

		virtual void on_playback_new_track(metadb_handle_ptr p_track);
		virtual void on_playback_time(double p_time);
		virtual void on_playback_stop(play_control::t_stop_reason reason);
		virtual void on_playback_pause(bool p_state);

		// play_callback methods (the rest)
		virtual void on_playback_dynamic_info_track(const file_info & p_info) {}
		virtual void on_playback_starting(play_control::t_track_command p_command, bool p_paused) {}
		virtual void on_playback_seek(double p_time) {}
		virtual void on_playback_edited(metadb_handle_ptr p_track) {}
		virtual void on_playback_dynamic_info(const file_info & p_info) {}
		virtual void on_volume_change(float p_new_val) {}

	};
}