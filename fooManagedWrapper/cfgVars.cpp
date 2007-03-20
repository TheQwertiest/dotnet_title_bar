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
#include <vcclr.h>


using namespace fooManagedWrapper;
using namespace System;
using namespace std;

namespace fooManagedWrapper {

	CCfgString::CCfgString(Guid^ _guid, String^ _def) {
		const char *_defString = CManagedWrapper::ToCString(_def);
		wrapper = new CCfgStringWrapper(CManagedWrapper::ToGUID(_guid), _defString);
		CManagedWrapper::FreeCString(_defString);
	};

	CCfgString::!CCfgString() {
		delete wrapper;
	};

	CCfgString::~CCfgString() {
		this->!CCfgString();
	}

	void CCfgString::SetVal(String ^a) {
		const char *_defString = CManagedWrapper::ToCString(a);
		wrapper->val = _defString;
		CManagedWrapper::FreeCString(_defString);
	};

	void notifying_cfg_string::get_data_raw(foobar2000_io::stream_writer *p_stream, foobar2000_io::abort_callback &p_abort) {
		owner->FireBeforeWritingEvent();
		cfg_string::get_data_raw(p_stream, p_abort);
	};

	/*
	void notifying_cfg_string::set_data_raw(foobar2000_io::stream_reader *p_stream, t_size p_sizehint, foobar2000_io::abort_callback &p_abort) {
		cfg_string::set_data_raw(p_stream, p_sizehint, p_abort);
	};
	*/

	CNotifyingCfgString::!CNotifyingCfgString() {
		delete wrapper;
	}

	CNotifyingCfgString::~CNotifyingCfgString() {
		this->!CNotifyingCfgString();
	}

	void CNotifyingCfgString::SetVal(String ^a) {
		const char *_defString = CManagedWrapper::ToCString(a);
		wrapper->val = _defString;
		CManagedWrapper::FreeCString(_defString);
	}

	CNotifyingCfgString::CNotifyingCfgString(Guid ^_guid, String ^_def) {
		const char *_defString = CManagedWrapper::ToCString(_def);
		wrapper = new CNotifyingCfgStringWrapper(CManagedWrapper::ToGUID(_guid), _defString, this);
		CManagedWrapper::FreeCString(_defString);
	}

	void CNotifyingCfgString::FireBeforeWritingEvent() {
		BeforeWriting(this);
	}

};