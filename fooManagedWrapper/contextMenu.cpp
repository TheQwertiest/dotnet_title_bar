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
	
	metadb_handle_list CContextMenuItem::getContext(Context context) {
		if (context == Context::NowPlaying) {
			metadb_handle_ptr item;
			if (!static_api_ptr_t<playback_control>()->get_now_playing(item)) {
				return pfc::list_t<metadb_handle_ptr>();
			}
			return pfc::list_single_ref_t<metadb_handle_ptr>(item);
		} else {
			metadb_handle_list res;
			static_api_ptr_t<playlist_manager> api;
			api->activeplaylist_get_selected_items(res);
			return res;
		}
	}

	void CContextMenuItem::Execute(unsigned int index, Context context) {
		(*ptr)->item_execute_simple(index, pfc::guid_null, this->getContext(context), contextmenu_item::caller_undefined);
	}

	void CContextMenuItem::Execute(unsigned int index, Guid ^cmd, Context context) {
		(*ptr)->item_execute_simple(index, CManagedWrapper::ToGUID(cmd), this->getContext(context), contextmenu_item::caller_undefined);
	}

	/** 
		Finds and returns the guid of given dynamically generated command.
		The command is searched under the item with index @param itemToSearch.
		@param path is a slash separated list of nodes relative to the starting
					command. If empty, the GUID of the item itself is returned.
					This allows to use the same code for dynamic and static commands.
		@param context In which context to search the item (should be the same as the context in which we run it).
		@return Returns the GUID if found or null.
	*/
	Nullable<Guid> CContextMenuItem::FindDynamicCommand(unsigned int itemToSearch, String ^path, Context context) {
		metadb_handle_list_cref data = this->getContext(context);

		pfc::ptrholder_t<contextmenu_item_node_root> root = (*ptr)->instantiate_item(itemToSearch, data, contextmenu_item::caller_active_playlist);
		contextmenu_item_node *current = root.get_ptr();
		if (current == NULL) {
			return Nullable<Guid>();
		}
		/*
		// DEBUG
		pfc::string8 str;
		unsigned int x;
		current->get_display_data(str, x, data, contextmenu_item::caller_undefined);
		int typ = current->get_type();
		int pop = contextmenu_item_node::TYPE_POPUP;
		int cmd = contextmenu_item_node::TYPE_COMMAND;

		console::info(str);
*/
		// search only if there's is some path. Otherwise current command is what we want.
		if (path != String::Empty) {
			cli::array<__wchar_t> ^separators = gcnew cli::array<__wchar_t>(1);
			separators->SetValue(L'/', 0);
			cli::array<String^> ^parts = path->Split(separators);

			return this->recFindDynamicCmd(current, parts, 0, data);

		} else {
			return Nullable<Guid>(CManagedWrapper::FromGUID((*ptr)->get_item_guid(itemToSearch)));
		}
	}

	Nullable<Guid> CContextMenuItem::recFindDynamicCmd(contextmenu_item_node *node, cli::array<String^> ^parts, int firstPart, metadb_handle_list_cref data) {
		if (this->nameMatches(node, parts[firstPart], data)) {

			if (firstPart == parts->Length - 1) {
				return Nullable<Guid>(CManagedWrapper::FromGUID(node->get_guid()));
			} else {
				for (unsigned int i = 0; i < node->get_children_count(); i++) {
					Nullable<Guid> found = this->recFindDynamicCmd(node->get_child(i), parts, firstPart + 1, data);
					if (found.HasValue) return found;
				}
			}
		}

		return Nullable<Guid>();
	}

	bool CContextMenuItem::nameMatches(contextmenu_item_node *node, String ^name, metadb_handle_list_cref data) {
		unsigned int displayflags;
		pfc::string8 str;
		node->get_display_data(str, displayflags, data, contextmenu_item::caller_undefined);

		return (CManagedWrapper::PfcStringToString(str)->ToLowerInvariant() == name->ToLowerInvariant());
	}

};
