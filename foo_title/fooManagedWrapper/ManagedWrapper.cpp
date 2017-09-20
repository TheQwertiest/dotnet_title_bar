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
#include "fooServices.h"
#include "ComponentLoader.h"
#include "ManagedWrapper.h"
#include "prefPages_v3.h"
#include "mainMenuCommands.h"

#include <msclr\marshal_cppstd.h>
#include <codecvt>

using namespace msclr::interop;

using namespace System;
using namespace fooManagedWrapper;
using namespace System::Text;

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
		CManagedPrefPage_v3::guid_root = FromGUID(preferences_page::guid_root);
		CManagedPrefPage_v3::guid_hidden = FromGUID(preferences_page::guid_hidden);
		CManagedPrefPage_v3::guid_tools = FromGUID(preferences_page::guid_tools);
		CManagedPrefPage_v3::guid_core = FromGUID(preferences_page::guid_core);
		CManagedPrefPage_v3::guid_display = FromGUID(preferences_page::guid_display);
		CManagedPrefPage_v3::guid_playback = FromGUID(preferences_page::guid_playback);
		CManagedPrefPage_v3::guid_visualisations = FromGUID(preferences_page::guid_visualisations);
		CManagedPrefPage_v3::guid_input = FromGUID(preferences_page::guid_input);
		CManagedPrefPage_v3::guid_tag_writing = FromGUID(preferences_page::guid_tag_writing);
		CManagedPrefPage_v3::guid_media_library = FromGUID(preferences_page::guid_media_library);

		// and the guids for mainmenu groups 
		CMainMenuCommandsImpl::file = FromGUID(mainmenu_groups::file);
		CMainMenuCommandsImpl::view = FromGUID(mainmenu_groups::view);
		CMainMenuCommandsImpl::edit = FromGUID(mainmenu_groups::edit);
		CMainMenuCommandsImpl::playback = FromGUID(mainmenu_groups::playback);
		CMainMenuCommandsImpl::library = FromGUID(mainmenu_groups::library);
		CMainMenuCommandsImpl::help = FromGUID(mainmenu_groups::help);

		// find and create components
		try {
			componentLoader = gcnew CComponentLoader();
			componentClients = componentLoader->LoadComponentsInDir(GetModuleDirectory(), "foo_");

			for each (IComponentClient ^cl in componentClients) {
				cl->Create();
			}
		} catch (Exception ^e) {
			fooManagedWrapper::CConsole::Error(e->Message);
		}

          isStarted = true;
	}

     String ^ CManagedWrapper::GetNetComponentName()
     {
          if ( !isStarted || !componentClients->Count )
          {
               return gcnew String( "Managed Wrapper Sub-Component" );
          }
          else
          {
               return componentClients[0]->GetName();
          }
     }

     String ^ CManagedWrapper::GetNetComponentVersion()
     {
          if ( !isStarted || !componentClients->Count )
          {
               return gcnew String( "0.0.0" );
          }
          else
          {
               return componentClients[0]->GetVersion();
          }
     }

     String ^ CManagedWrapper::GetNetComponentDescription()
     {
          if ( !isStarted || !componentClients->Count )
          {
               return gcnew String( ".NET component was not loaded" );
          }
          else
          {
               return componentClients[0]->GetDescription();
          }
     }

     void CManagedWrapper::OnInit()
     {
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

	_GUID CManagedWrapper::ToGUID(Guid^ guid) {
		array<Byte>^ guidData = guid->ToByteArray();
		pin_ptr<Byte> data = &(guidData[0]);

		return *(_GUID *)data;
	}

	Guid ^CManagedWrapper::FromGUID(const _GUID& guid) {
		return gcnew Guid(guid.Data1, guid.Data2, guid.Data3,
			guid.Data4[0], guid.Data4[1],
			guid.Data4[2], guid.Data4[3],
			guid.Data4[4], guid.Data4[5],
			guid.Data4[6], guid.Data4[7]);
	}

	String ^CManagedWrapper::GetModuleDirectory() {
		return System::IO::Path::GetDirectoryName(modulePath);
	}

	String ^CManagedWrapper::GetProfilePath() {
		String ^result = gcnew String(core_api::get_profile_path());
		if (result->StartsWith("file://")) {
			result = result->Substring(7);
		}
		return result;
	}

	void CManagedWrapper::DoMainMenuCommand(String ^name) {
		if (core_api::is_main_thread()) {
               std::string c_name( CManagedWrapper::ToStdString( name ) );
			GUID guid;
			mainmenu_commands::g_find_by_name(c_name.c_str(), guid);
			mainmenu_commands::g_execute(guid);
		} else {
			pfc::crash();
		}
	}

	String ^CManagedWrapper::GetAllCommands() {
		String ^res = gcnew String(">");

		service_enum_t<contextmenu_item> ec;

		service_ptr_t<contextmenu_item> ptr;
		while (ec.next(ptr)) {
			unsigned int count = ptr->get_num_items();

			for (unsigned int i = 0; i < count; i++) {
				pfc::string8 str;
				ptr->get_item_name(i, str);

				res += PfcStringToString(str);
				res += " ";

				ptr->get_item_default_path(i, str);

				res += PfcStringToString(str);
				res += "\n";
			}
		}

		return res;
	}

	bool CManagedWrapper::IsFoobarActivated() {
		return (*UIControlInstance).get_ptr()->is_visible();
	}

	Icon ^CManagedWrapper::GetMainIcon() {
		HICON hIcon = (*UIControlInstance).get_ptr()->get_main_icon();
		return Icon::FromHandle((IntPtr)hIcon);
	}

	pfc::string8 CManagedWrapper::StringToPfcString(String ^a) {
          std::string tmpStr( ToStdString( a ) );
          return pfc::string8( tmpStr.c_str() );
	}

     String ^CManagedWrapper::PfcStringToString( const pfc::string8 &stringToConvert )
     {
          return gcnew String( stringToConvert.get_ptr(), 0, stringToConvert.get_length(), gcnew System::Text::UTF8Encoding() );
     }

     std::string CManagedWrapper::ToStdString( String^ string )
     {
          marshal_context ctx;
          std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> convert;

          return convert.to_bytes( ctx.marshal_as<std::wstring>( string ) );
     }
};
