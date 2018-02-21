/*
*  This file is part of foo_title.
*  Copyright 2005 - 2006 Roman Plasil (http://foo-title.sourceforge.net)
*  Copyright 2016 Miha Lepej (https://github.com/LepkoQQ/foo_title)
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

#pragma once


namespace fooManagedWrapper {

	//! this class relays foobar's play callbacks to the managed components
	class CPlayCallback : public play_callback {
	public:

		CPlayCallback();

		virtual void on_playback_new_track(metadb_handle_ptr p_track);
		virtual void on_playback_time(double p_time);
		virtual void on_playback_stop(play_control::t_stop_reason reason);
		virtual void on_playback_pause(bool p_state);
		virtual void on_playback_dynamic_info_track(const file_info & p_info);

		// play_callback methods (the rest)
		virtual void on_playback_starting(play_control::t_track_command p_command, bool p_paused) {}
		virtual void on_playback_seek(double p_time) {}
		virtual void on_playback_edited(metadb_handle_ptr p_track) {}
		virtual void on_playback_dynamic_info(const file_info & p_info) {}
		virtual void on_volume_change(float p_new_val) {}

	};
}