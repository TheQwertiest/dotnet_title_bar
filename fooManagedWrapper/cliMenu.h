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

	public ref class CMainMenuGroupPopup : public IFoobarService {
	private:
		mainmenu_group_popup_factory *wrapper;
		service_ptr_t<mainmenu_group_popup> *ptr;
	public:
		// this creates and registers a new menu group popup
		CMainMenuGroupPopup(Guid ^guid, Guid ^parent, int priority, String ^ name);

		// this constructor wraps an existing menu group popup
		CMainMenuGroupPopup(const service_ptr_t<mainmenu_group_popup> &existingPtr);

		!CMainMenuGroupPopup() {
			this->~CMainMenuGroupPopup();
		}

		~CMainMenuGroupPopup() {
			SAFE_DELETE(ptr);
			SAFE_DELETE(wrapper);
		}

		property String ^Name {
			String ^get() {
				pfc::string8 pfcName;
				(*ptr)->get_display_string(pfcName);
				return CManagedWrapper::PfcStringToString(pfcName);
			};
		}

		property Guid ^MyGuid {
			Guid ^get() {
				return CManagedWrapper::FromGUID((*ptr)->get_guid());
			}
		}

		property Guid ^Parent {
			Guid ^get() {
				return CManagedWrapper::FromGUID((*ptr)->get_parent());
			}
		}

	};


	public ref class CMainMenuGroupPopupEnumerator :
		public CEnumeratorAdapterBase<mainmenu_group, CMainMenuGroupPopup^> {
	private:
		typedef CEnumeratorAdapterBase<mainmenu_group, CMainMenuGroupPopup^> BaseType;
		service_ptr_t<mainmenu_group_popup> *castCurrent;
	public:
		CMainMenuGroupPopupEnumerator() {
			castCurrent = NULL;
		}

		~CMainMenuGroupPopupEnumerator() {
			SAFE_DELETE(castCurrent);
		}

		virtual bool MoveNext() override;


		virtual property CMainMenuGroupPopup ^Current {
			virtual CMainMenuGroupPopup ^get() override = System::Collections::Generic::IEnumerator<CMainMenuGroupPopup^>::Current::get {
				if (castCurrent == NULL)
					throw gcnew InvalidOperationException("First call MoveNext before accessing the current element.");

				return gcnew CMainMenuGroupPopup(*castCurrent);
			}
		}
		
	};

};