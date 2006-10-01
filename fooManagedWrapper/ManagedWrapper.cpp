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

using namespace System;
using namespace fooManagedWrapper;

namespace fooManagedWrapper {

	CManagedWrapper::CManagedWrapper() {
		instance = this;
		componentLoader = nullptr;
		componentClients = nullptr;
		services = gcnew List<IFoobarService^>();
		UIControlInstance = NULL;
	}

	CManagedWrapper::~CManagedWrapper() {
		this->!CManagedWrapper();
	}

	CManagedWrapper::!CManagedWrapper() {

		if (UIControlInstance)
			delete UIControlInstance;
		UIControlInstance = NULL;
	}

	void CManagedWrapper::Start(String ^_modulePath) {
		modulePath = _modulePath;

		// fill in the guids for prefpages
		CManagedPrefPage::guid_root = FromGUID(preferences_page::guid_root);
		CManagedPrefPage::guid_hidden = FromGUID(preferences_page::guid_hidden);
		CManagedPrefPage::guid_tools= FromGUID(preferences_page::guid_tools);
		CManagedPrefPage::guid_core= FromGUID(preferences_page::guid_core);
		CManagedPrefPage::guid_display= FromGUID(preferences_page::guid_display);
		CManagedPrefPage::guid_playback= FromGUID(preferences_page::guid_playback);
		CManagedPrefPage::guid_visualisations= FromGUID(preferences_page::guid_visualisations);
		CManagedPrefPage::guid_input= FromGUID(preferences_page::guid_input);
		CManagedPrefPage::guid_tag_writing= FromGUID(preferences_page::guid_tag_writing);
		CManagedPrefPage::guid_media_library= FromGUID(preferences_page::guid_media_library);

		// and the guids for mainmenu groups 
		CManagedMainMenuCommands::file = FromGUID(mainmenu_groups::file);
		CManagedMainMenuCommands::view = FromGUID(mainmenu_groups::view);
		CManagedMainMenuCommands::edit = FromGUID(mainmenu_groups::edit);
		CManagedMainMenuCommands::playback = FromGUID(mainmenu_groups::playback);
		CManagedMainMenuCommands::library = FromGUID(mainmenu_groups::library);
		CManagedMainMenuCommands::help = FromGUID(mainmenu_groups::help);

		// find and create components
		try {
			componentLoader = gcnew ComponentLoader();
			componentClients = componentLoader->LoadComponentsInDir(System::IO::Path::Combine(GetFoobarDirectory(), "components\\"), "dotnet_");

			for each (IComponentClient ^cl in componentClients) {
				cl->Create();
			}
		} catch (Exception ^e) {
			fooManagedWrapper::Console::Error(e->Message);
		}
	}

	void CManagedWrapper::OnInit() {
		UIControlInstance = new static_api_ptr_t<ui_control>();
	}

	CManagedWrapper ^CManagedWrapper::getInstance() {
		return instance;
	}

	System::Collections::IEnumerator ^CManagedWrapper::GetClients() {
		return componentClients->GetEnumerator();
	}

	void CManagedWrapper::AddService(IFoobarService ^a) {
		if (core_api::are_services_available()) {
			throw gcnew ServicesDoneException();
		}
		services->Add(a);
	}

	const char *CManagedWrapper::ToCString(String ^a) {
		return (const char*)(System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(a)).ToPointer();
	}

	void CManagedWrapper::FreeCString(const char *a) {
		System::Runtime::InteropServices::Marshal::FreeHGlobal(IntPtr((void*)a));
	}

	_GUID CManagedWrapper::ToGUID(Guid^ guid) {
		array<Byte>^ guidData = guid->ToByteArray();
		pin_ptr<Byte> data = &(guidData[ 0 ]);

		return *(_GUID *)data;
	}

	Guid ^CManagedWrapper::FromGUID(const _GUID& guid) {
		return gcnew Guid( guid.Data1, guid.Data2, guid.Data3, 
			guid.Data4[ 0 ], guid.Data4[ 1 ], 
			guid.Data4[ 2 ], guid.Data4[ 3 ], 
			guid.Data4[ 4 ], guid.Data4[ 5 ], 
			guid.Data4[ 6 ], guid.Data4[ 7 ] );
	}

	
	String ^CManagedWrapper::GetFoobarDirectory() {
		String ^res = System::IO::Path::GetDirectoryName(modulePath);
		res = res->Substring(0, res->Length - 10);
		return res;
	}

	void CManagedWrapper::DoMainMenuCommand(String ^name) {
		if (core_api::is_main_thread()) {
			const char *c_name = ToCString(name);
			GUID guid;
			mainmenu_commands::g_find_by_name(c_name, guid);
			mainmenu_commands::g_execute(guid);
			FreeCString(c_name);
		} else {
			pfc::crash();
		}
		 
	}

	bool CManagedWrapper::IsFoobarActivated() {
		return (*UIControlInstance).get_ptr()->is_visible();
	}


};
