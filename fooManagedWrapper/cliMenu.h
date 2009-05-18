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

#pragma once

#include "stdafx.h"
#include "fooServices.h"
#include "ManagedWrapper.h"
#include "utils.h"

using namespace System;
using namespace System::Collections::Generic;


namespace fooManagedWrapper {

	// This does not yet support creating a group from the managed code.
	// Only wrapping is supported.
	public ref class CMainMenuGroup : public IFoobarService {
	protected:
		service_ptr_t<mainmenu_group> *ptr;

		CMainMenuGroup() { ptr = NULL; };

	public:
		CMainMenuGroup(const service_ptr_t<mainmenu_group> &existingPtr);
		virtual ~CMainMenuGroup();

		!CMainMenuGroup() {
			this->~CMainMenuGroup();
		}

		property Guid MyGuid {
			Guid get() {
				return safe_cast<Guid>(CManagedWrapper::FromGUID((*ptr)->get_guid()));
			}
		}

		property Guid Parent {
			Guid get() {
				return safe_cast<Guid>(CManagedWrapper::FromGUID((*ptr)->get_parent()));
			}
		}
	};

	public ref class CMainMenuGroupPopup : public CMainMenuGroup {
	private:
		mainmenu_group_popup_factory *wrapper;
		service_ptr_t<mainmenu_group_popup> *castPtr;
	public:
		// this creates and registers a new menu group popup
		CMainMenuGroupPopup(Guid ^guid, Guid ^parent, int priority, String ^ name);

		// this constructor wraps an existing menu group popup
		CMainMenuGroupPopup(const service_ptr_t<mainmenu_group_popup> &existingPtr);

		virtual ~CMainMenuGroupPopup() {
			SAFE_DELETE(wrapper);
			SAFE_DELETE(castPtr);
		}

		property String ^Name {
			String ^get() {
				pfc::string8 pfcName;
				(*castPtr)->get_display_string(pfcName);
				return CManagedWrapper::PfcStringToString(pfcName);
			};
		}
	};


	public ref class CMainMenuGroupEnumerator :
		public CEnumeratorAdapter<mainmenu_group, CMainMenuGroup, CMainMenuGroup^> {
	public:
		virtual property CMainMenuGroup ^Current {
			virtual CMainMenuGroup ^get() override = System::Collections::Generic::IEnumerator<CMainMenuGroup^>::Current::get {
				if (GetCurrent() == NULL)
					throw gcnew InvalidOperationException("First call MoveNext before accessing the current element.");

				// try to cast
				service_ptr_t<mainmenu_group_popup> castCurrent;
				if (GetCurrent()->service_query_t<mainmenu_group_popup>(castCurrent)) {
					return gcnew CMainMenuGroupPopup(castCurrent);
				} else {
					return gcnew CMainMenuGroup(GetCurrent());
				}
			}
		}
		
	};


	// ------------------------------------------------------------------------------
	// Wrapper for context menu commands. Creating a new command is not yet supported.
	// Dynamic submenus are not supported.
	public ref class CContextMenuItem : public IFoobarService {
	private:
		service_ptr_t<contextmenu_item> *ptr;

	public:
		CContextMenuItem(const service_ptr_t<contextmenu_item> &existingPtr);

		virtual ~CContextMenuItem() {
			SAFE_DELETE(ptr);
		}

		String ^GetName(unsigned int index);
		String ^GetDefaultPath(unsigned int index);
		void ExecuteOnPlaylist(unsigned int index);
		void ExecuteOnNowPlaying(unsigned int index);

		Nullable<Guid> FindDynamicCommand(unsigned int itemToSearch, String ^path);
		void ExecuteOnPlaylist(Guid ^cmd);
		void ExecuteOnNowPlaying(Guid ^cmd);

		property unsigned int Count {
			unsigned int get() {
				return (*ptr)->get_num_items();
			}
		}

	};

	public ref class CContextMenuItemEnumerator:
		public CEnumeratorAdapter<contextmenu_item, CContextMenuItem, CContextMenuItem^> {
	};


};