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
		public System::Collections::Generic::IEnumerator<CMainMenuGroupPopup^>,
		public Generic::IEnumerable<CMainMenuGroupPopup^> {
	private:
		service_enum_t<mainmenu_group> *enumerator;
		service_ptr_t<mainmenu_group_popup> *current;
	public:
		CMainMenuGroupPopupEnumerator() {
			enumerator = new service_enum_t<mainmenu_group>();
			current = NULL;
		}

		~CMainMenuGroupPopupEnumerator() {
			SAFE_DELETE(current);
			SAFE_DELETE(enumerator);
		}

		!CMainMenuGroupPopupEnumerator() {
			this->~CMainMenuGroupPopupEnumerator();
		}

		virtual bool MoveNext();

		virtual void Reset() {
			throw gcnew NotImplementedException();
		}

		virtual property CMainMenuGroupPopup ^Current {
			virtual CMainMenuGroupPopup ^get() = System::Collections::Generic::IEnumerator<CMainMenuGroupPopup^>::Current::get {
				if (current == NULL)
					throw gcnew InvalidOperationException("First call MoveNext before accessing the current element.");

				return gcnew CMainMenuGroupPopup(*current);
			}
		}

		virtual property Object ^Current2 {
			virtual Object ^get() = System::Collections::IEnumerator::Current::get {
				return this->Current::get();
			}
		}


		virtual Generic::IEnumerator<CMainMenuGroupPopup^> ^GetEnumerator() = 
			Collections::Generic::IEnumerable<CMainMenuGroupPopup^>::GetEnumerator {
				return this;
		}

		virtual Collections::IEnumerator ^GetEnumerator2() =
			Collections::IEnumerable::GetEnumerator {
				return this;
		}
		
	};

};