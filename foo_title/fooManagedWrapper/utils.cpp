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
#include "utils.h"
#include "ManagedWrapper.h"

using namespace fooManagedWrapper;
using namespace System;
using namespace pfc;

CMetaDBHandle::CMetaDBHandle(const metadb_handle_ptr &src) {
	this->handle = new metadb_handle_ptr(src);
}

CMetaDBHandle::!CMetaDBHandle() {
	// The GC may postpone freeing objects after foobar's services are not
	// available, so trying to free the handle gives an assert violation.
	// As this happens only when exiting the process, we needn't care.
	if (!core_api::are_services_available()) return;

	if (this->handle != NULL) {
		delete this->handle;
		this->handle = NULL;
	}
}

String ^CMetaDBHandle::GetPath() {
	if (handle == NULL)
		throw gcnew System::NullReferenceException("GetPath() called on a MetaDBHandle containing NULL handle");
	const char *c_path = (*handle)->get_path();
	return gcnew String(c_path, 0, strlen(c_path), gcnew System::Text::UTF8Encoding(true, true));
}

double CMetaDBHandle::GetLength() {
	return (*handle)->get_length();
}

Bitmap ^CMetaDBHandle::GetArtworkBitmap(Boolean get_stub) {
	static_api_ptr_t<album_art_manager_v2> p_album_art_manager_v2;
	abort_callback_dummy cb_abort;

	pfc::list_t<GUID> guids;
	guids.add_item(album_art_ids::cover_front);

	album_art_extractor_instance_v2::ptr artwork_api_v2;
	if (get_stub) {
		artwork_api_v2 = p_album_art_manager_v2->open_stub(cb_abort);
	} else {
		artwork_api_v2 = p_album_art_manager_v2->open(pfc::list_single_ref_t<metadb_handle_ptr>(*handle), guids, cb_abort);
	}

	boolean b_found = false;
	album_art_data_ptr data;
	try {
		data = artwork_api_v2->query(album_art_ids::cover_front, cb_abort);
		b_found = true;
	}
	catch (const exception_aborted &) {}
	catch (exception_io_not_found const &) {}
	catch (exception_io const &e) {
		console::formatter formatter;
		formatter << "Requested Album Art entry could not be retrieved: " << e.what() << "; get_stub: " << get_stub;
	}

	if (b_found && data.is_valid()) {
		System::IO::UnmanagedMemoryStream ^stream = gcnew System::IO::UnmanagedMemoryStream((unsigned char*)data->get_ptr(), data->get_size());
		Bitmap ^bmp = gcnew Bitmap(stream);
		delete stream;
		data.release();
		return bmp;
	}

	return nullptr;
}

String ^CPlayControl::FormatTitle(CMetaDBHandle ^handle, String ^spec) {
	if (handle == nullptr) return gcnew String("abc");

	const char* spec_c = (const char*)(System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(spec)).ToPointer();
	string8 out;

	static_api_ptr_t<play_control> pc;

	static_api_ptr_t<titleformat_compiler> titlecompiler;
	service_ptr_t<titleformat_object> compiledScript;
	titlecompiler->compile(compiledScript, spec_c);

	pc->playback_format_title_ex(handle->GetHandle(), NULL, out, compiledScript, NULL, playback_control::display_level_all);
	
	String ^res = CManagedWrapper::PfcStringToString(out);
	System::Runtime::InteropServices::Marshal::FreeHGlobal(IntPtr((void*)spec_c));
	return res;
}

CMetaDBHandle ^CPlayControl::GetNowPlaying() {
	static_api_ptr_t<play_control> pc;
	metadb_handle_ptr out;
	pc->get_now_playing(out);

	CMetaDBHandle ^res = gcnew CMetaDBHandle(out.get_ptr());
	return res;
}

double CPlayControl::PlaybackGetPosition() {
	static_api_ptr_t<play_control> pc;
	return pc->playback_get_position();
}

bool CPlayControl::IsPlaying() {
	static_api_ptr_t<play_control> pc;
	return pc->is_playing();
}

bool CPlayControl::IsPaused() {
	static_api_ptr_t<play_control> pc;
	return pc->is_paused();
}


void fooManagedWrapper::CConsole::Error(String ^a) {
	const char *c_msg = CManagedWrapper::ToCString(a);
	console::error(c_msg);
	CManagedWrapper::FreeCString(c_msg);
}

void fooManagedWrapper::CConsole::Warning(String ^a) {
	const char *c_msg = CManagedWrapper::ToCString(a);
	console::warning(c_msg);
	CManagedWrapper::FreeCString(c_msg);
}

void fooManagedWrapper::CConsole::Write(String ^a) {
	const char *c_msg = CManagedWrapper::ToCString(a);
	console::print(c_msg);
	CManagedWrapper::FreeCString(c_msg);
}

