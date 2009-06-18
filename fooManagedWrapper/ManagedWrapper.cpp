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
#include "prefPages.h"
#include "mainMenuCommands.h"

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
		CMainMenuCommandsImpl::file = FromGUID(mainmenu_groups::file);
		CMainMenuCommandsImpl::view = FromGUID(mainmenu_groups::view);
		CMainMenuCommandsImpl::edit = FromGUID(mainmenu_groups::edit);
		CMainMenuCommandsImpl::playback = FromGUID(mainmenu_groups::playback);
		CMainMenuCommandsImpl::library = FromGUID(mainmenu_groups::library);
		CMainMenuCommandsImpl::help = FromGUID(mainmenu_groups::help);

		// find and create components
		try {
			componentLoader = gcnew CComponentLoader();
			componentClients = componentLoader->LoadComponentsInDir(System::IO::Path::Combine(GetFoobarDirectory(), "components\\"), "dotnet_");

			for each (IComponentClient ^cl in componentClients) {
				cl->Create();
			}
		} catch (Exception ^e) {
			fooManagedWrapper::CConsole::Error(e->Message);
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

	String ^CManagedWrapper::GetProfilePath() {
		String ^result = gcnew String(core_api::get_profile_path());
		if (result->StartsWith("file://")) {
			result = result->Substring(7);
		}
		return result;
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

	String ^CManagedWrapper::GetAllCommands() {
		String ^res = gcnew String(">");

		service_enum_t<contextmenu_item> ec;

		service_ptr_t<contextmenu_item> ptr;
		while (ec.next(ptr)) {
			unsigned int count = ptr->get_num_items();

			for (unsigned int i = 0 ; i < count; i++) {
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

/*
	union CharToBytes {
		unsigned int netChar;
		struct {
			unsigned char first;
			unsigned char second;
		} bytes;
	};
*/
	pfc::string8 CManagedWrapper::StringToPfcString(String ^a) {
		Encoder ^enc = Encoding::UTF8->GetEncoder();
		int charsUsed, bytesUsed;
		bool completed;
		array<unsigned char> ^out = gcnew array<unsigned char>(a->Length * 4);
		enc->Convert(a->ToCharArray(), 0, a->Length, out, 0, a->Length * 4, true, charsUsed, bytesUsed, completed);
		char *c_str = new char[bytesUsed + 1];
		System::Runtime::InteropServices::Marshal::Copy((array<unsigned char> ^)out, (int)0, (System::IntPtr)c_str, (int)bytesUsed);
		c_str[bytesUsed] = 0;
		pfc::string8 result(c_str);
		delete[] c_str;
		return result;
	}

	String ^CManagedWrapper::PfcStringToString(const pfc::string8 &stringToConvert) {
		return gcnew String(stringToConvert.get_ptr(), 0, stringToConvert.get_length(), gcnew System::Text::UTF8Encoding());
	}

	class MoodItems : public contextmenu_item {
	public:

		class node : public contextmenu_item_node_root {
			virtual bool get_display_data(pfc::string_base & p_out,unsigned & p_displayflags,metadb_handle_list_cref p_data,const GUID & p_caller) {
				p_out = "Mood 1";
				return true;
			}

			virtual t_type get_type() {
				return TYPE_COMMAND;
			}

			virtual void execute(metadb_handle_list_cref p_data,const GUID & p_caller) {
				::MessageBox(NULL, L"Blabla", L"Capshun", MB_OK);
			}

			virtual t_glyph get_glyph(metadb_handle_list_cref p_data,const GUID & p_caller) {return 0;}//RESERVED

			virtual t_size get_children_count() {
				return 0;
			}

			virtual contextmenu_item_node * get_child(t_size p_index) {
				return NULL;
			}

			virtual bool get_description(pfc::string_base & p_out) {
				return false;
			}

			virtual GUID get_guid() {
				GUID g = {5644, 125, 358, "tdibxyu"};
				return g;
			}

			virtual bool is_mappable_shortcut() {
				return false;
			}

		};

		unsigned get_num_items() { return 1; }

		//! Instantiates a context menu item (including sub-node tree for items that contain dynamically-generated sub-items).
		virtual contextmenu_item_node_root * instantiate_item(unsigned p_index,metadb_handle_list_cref p_data,const GUID & p_caller) {
			return new node();
		}

		//! Retrieves GUID of the context menu item.
		virtual GUID get_item_guid(unsigned p_index) {
			GUID g = {564789, 125, 358, "tdibxau"};
			return g;
		}
		//! Retrieves human-readable name of the context menu item.
		virtual void get_item_name(unsigned p_index,pfc::string_base & p_out) {
			p_out = "Mood";
		}
		//! Retrieves default path of the context menu item ("" for root).
		virtual void get_item_default_path(unsigned p_index,pfc::string_base & p_out) {
			p_out = "abc";

		}
		//! Retrieves item's description to show in the status bar. Set p_out to the string to be displayed and return true if you provide a description, return false otherwise.
		virtual bool get_item_description(unsigned p_index,pfc::string_base & p_out) {
			return false;

		}
		//! Signals whether the item should be forcefully hidden (FORCE_OFF), hidden by default but possible to add (DEFAULT_OFF) or shown by default (DEFAULT_ON).
		virtual contextmenu_item::t_enabled_state get_enabled_state(unsigned p_index) {
			return DEFAULT_ON;
		}
		//! Executes the menu item command without going thru the instantiate_item path. For items with dynamically-generated sub-items, p_node is identifies of the sub-item command to execute.
		virtual void item_execute_simple(unsigned p_index,const GUID & p_node,metadb_handle_list_cref p_data,const GUID & p_caller) {
			::MessageBeep(MB_ICONQUESTION);
		}

	};

	static contextmenu_item_factory_t<MoodItems> mood;
};
