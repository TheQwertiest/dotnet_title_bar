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
#include "prefPages_v3.h"
#include "ManagedWrapper.h"

using namespace fooManagedWrapper;
using namespace std;

namespace fooManagedWrapper {
	CCustomPrefPage_v3::CCustomPrefPage_v3() {
		form = NULL;
	}

	CCustomPrefPage_v3::~CCustomPrefPage_v3() {
     }

	preferences_page_instance::ptr CCustomPrefPage_v3::instantiate(HWND parent, preferences_page_callback::ptr callback) {
		return new service_impl_t<CCustomPrefPageInstance_v3>(form, parent, callback);
	}

	const char * CCustomPrefPage_v3::get_name() {
		if (name.empty()) {
               std::string c( CManagedWrapper::ToStdString( form->Text ) );
               name = std::string( c.c_str() );
		}
		return name.c_str();
	}

	//! Retrieves GUID of the page.
	GUID CCustomPrefPage_v3::get_guid() {
		return guid;
	}

	//! Retrieves GUID of parent page/branch of this page. See preferences_page::guid_* constants for list of standard parent GUIDs. Can also be a GUID of another page or a branch (see: preferences_branch).
	GUID CCustomPrefPage_v3::get_parent_guid() {
		return parentGuid;
	}

	void CCustomPrefPage_v3::SetManagedPrefPage(gcroot<CManagedPrefPage_v3^> a) {
		form = a;
	}

	void CCustomPrefPage_v3::SetGUID(GUID a) {
		guid = a;
	}

	void CCustomPrefPage_v3::SetParentGUID(GUID a) {
		parentGuid = a;
	}

	CManagedPrefPage_v3::~CManagedPrefPage_v3() {
		delete wrapper;
		delete cb;
	}

	CManagedPrefPage_v3::!CManagedPrefPage_v3() {
		this->~CManagedPrefPage_v3();
	}

	CManagedPrefPage_v3::CManagedPrefPage_v3(System::Guid ^myGuid, System::Guid ^parentGuid) {
		CManagedWrapper::getInstance()->AddService(this);

		wrapper = new CPrefPageFactoryWrapper_v3();
		wrapper->prefPage.get_static_instance().SetManagedPrefPage(this);
		wrapper->prefPage.get_static_instance().SetGUID(CManagedWrapper::ToGUID(myGuid));
		wrapper->prefPage.get_static_instance().SetParentGUID(CManagedWrapper::ToGUID(parentGuid));
	}

	void CManagedPrefPage_v3::OnChange() {
		if (cb != NULL) {
			cb->m_callback->on_state_changed();
		}
	}

	void CManagedPrefPage_v3::SetPrefPageInstance(preferences_page_callback::ptr _cb) {
		cb = new CPrefPageCallbackWrapper_v3();
		cb->m_callback = _cb;
	}

	CCustomPrefPageInstance_v3::CCustomPrefPageInstance_v3(gcroot<CManagedPrefPage_v3^> _form, HWND parent, preferences_page_callback::ptr callback) : m_callback(callback) {
		form = _form;
		SetParent((HWND)form->Handle.ToPointer(), parent);
		form->SetPrefPageInstance(m_callback);
		form->Show();
	}

	t_uint32 CCustomPrefPageInstance_v3::get_state() {
		t_uint32 state = preferences_state::resettable;
		if (form->HasChanged()) state |= preferences_state::changed;
		return state;
	}

	HWND CCustomPrefPageInstance_v3::get_wnd() {
		return (HWND)form->Handle.ToPointer();
	}

	void CCustomPrefPageInstance_v3::reset() {
		if (static_cast<CManagedPrefPage_v3^>(form) != nullptr)
			form->Reset();
	}

	void CCustomPrefPageInstance_v3::apply() {
		if (static_cast<CManagedPrefPage_v3^>(form) != nullptr)
			form->Apply();
	}
};
