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
#include "cliMenu.h"

namespace fooManagedWrapper {

	CContextMenuItem::CContextMenuItem(const service_ptr_t<contextmenu_item> &existingPtr) {
		ptr = new service_ptr_t<contextmenu_item>(existingPtr);
	}

	String ^CContextMenuItem::GetName(unsigned int index) {
		pfc::string8 str;
		(*ptr)->get_item_name(index, str);
		return CManagedWrapper::PfcStringToString(str);
	}

	String ^CContextMenuItem::GetDefaultPath(unsigned int index) {
		pfc::string8 str;
		(*ptr)->get_item_default_path(index, str);
		return CManagedWrapper::PfcStringToString(str);
	}
	
	void CContextMenuItem::ExecuteOnPlaylist(unsigned int index) {
		metadb_handle_list temp;
		static_api_ptr_t<playlist_manager> api;
		api->activeplaylist_get_selected_items(temp);
		(*ptr)->item_execute_simple(index, pfc::guid_null, temp, contextmenu_item::caller_undefined);
	}

	void CContextMenuItem::ExecuteOnNowPlaying(unsigned int index) {
		metadb_handle_ptr item;
		if (!static_api_ptr_t<playback_control>()->get_now_playing(item)) return;//not playing
		(*ptr)->item_execute_simple(index, pfc::guid_null, pfc::list_single_ref_t<metadb_handle_ptr>(item), contextmenu_item::caller_undefined);
	}

	/** 
		Finds and returns the guid of given dynamically generated command.
		The command is searched under the item with index @param itemToSearch.
		@param path is a slash separated list of nodes relative to the starting
					command. If empty, the GUID of the item itself is returned.
					This allows to use the same code for dynamic and static commands.
		@return Returns the GUID if found or null.
	*/
	Nullable<Guid> CContextMenuItem::FindDynamicCommand(unsigned int itemToSearch, String ^path) {
		metadb_handle_list_cref data = pfc::list_t<metadb_handle_ptr>();
		contextmenu_item_node_root *root = (*ptr)->instantiate_item(itemToSearch, data, contextmenu_item::caller_undefined);
		contextmenu_item_node *current = root;
		
		pfc::string8 str;
		current->get_description(str);
		console::info(str);

		// search only if there's is some path. Otherwise current command is what we want.
		if (path != String::Empty) {
			cli::array<__wchar_t> ^separators = gcnew cli::array<__wchar_t>(1);
			separators->SetValue(L'/', 0);
			cli::array<String^> ^parts = path->Split(separators);

			// traverse
			pfc::string8 str;
			unsigned int displayflags;

			for each (String ^part in parts) {
				bool found = false;

				for (unsigned int i = 0; i < current->get_children_count(); i++) {
					contextmenu_item_node *next = current->get_child(i);
					next->get_display_data(str, displayflags, data, contextmenu_item::caller_undefined);
				
					if (CManagedWrapper::PfcStringToString(str)->ToLowerInvariant() == part->ToLowerInvariant()) {
						current = next;
						found = true;
						break; // goto next part
					}
				}

				if (!found) return Nullable<Guid>();
			}
			
			// construct result
			Guid ^res = CManagedWrapper::FromGUID(current->get_guid());
			SAFE_DELETE(root);
			return Nullable<Guid>(res);

		} else {
			return Nullable<Guid>(CManagedWrapper::FromGUID((*ptr)->get_item_guid(itemToSearch)));
		}
	}

	void CContextMenuItem::ExecuteOnPlaylist(Guid ^cmd) {
		metadb_handle_list temp;
		static_api_ptr_t<playlist_manager> api;
		api->activeplaylist_get_selected_items(temp);
		// maybe we should provide correct command index here... but it works this way now
		(*ptr)->item_execute_simple(-1, CManagedWrapper::ToGUID(cmd), temp, contextmenu_item::caller_undefined);
	}

	void CContextMenuItem::ExecuteOnNowPlaying(Guid ^cmd) {
		metadb_handle_ptr item;
		if (!static_api_ptr_t<playback_control>()->get_now_playing(item)) return;//not playing
		(*ptr)->item_execute_simple(-1, CManagedWrapper::ToGUID(cmd), pfc::list_single_ref_t<metadb_handle_ptr>(item), contextmenu_item::caller_undefined);
	}
};
