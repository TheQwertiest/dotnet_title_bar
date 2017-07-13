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

namespace fooManagedWrapper {
	class CPrefPageFactoryWrapper_v3;
	class CPrefPageCallbackWrapper_v3;

	// to create a preferences page, create a new form (or control) that inherits from this class
	public ref class CManagedPrefPage_v3 : public Windows::Forms::ContainerControl, public IFoobarService {
	private:
		CPrefPageFactoryWrapper_v3 *wrapper;
		CPrefPageCallbackWrapper_v3 *cb;

	public:
		CManagedPrefPage_v3(System::Guid ^myGuid, System::Guid ^parentGuid);
		virtual ~CManagedPrefPage_v3();
		!CManagedPrefPage_v3();
		virtual void Reset() {};
		virtual void Apply() {};
		virtual bool HasChanged() { return false; };
		virtual void OnChange();
		void SetPrefPageInstance(preferences_page_callback::ptr inst);

		static System::Guid ^guid_root, ^guid_hidden, ^guid_tools, ^guid_core, ^guid_display, ^guid_playback, ^guid_visualisations, ^guid_input, ^guid_tag_writing, ^guid_media_library;
	};

	class CCustomPrefPage_v3 : public preferences_page_v3 {
	protected:
		gcroot<CManagedPrefPage_v3^> form;
		GUID guid;
		GUID parentGuid;
		char *name;

		CCustomPrefPage_v3();

	public:
		void SetManagedPrefPage(gcroot<CManagedPrefPage_v3^> a);
		void SetGUID(GUID a);
		void SetParentGUID(GUID a);

		virtual preferences_page_instance::ptr instantiate(HWND parent, preferences_page_callback::ptr callback);
		virtual const char * get_name();
		virtual GUID get_guid();
		virtual GUID get_parent_guid();

		virtual ~CCustomPrefPage_v3();
	};

	class CCustomPrefPageInstance_v3 : public preferences_page_instance {
	protected:
		gcroot<CManagedPrefPage_v3^> form;
		const preferences_page_callback::ptr m_callback;

	public:
		CCustomPrefPageInstance_v3(gcroot<CManagedPrefPage_v3^> _form, HWND parent, preferences_page_callback::ptr callback);
		virtual t_uint32 get_state();
		virtual HWND get_wnd();
		virtual void apply();
		virtual void reset();

	};

	class CPrefPageFactoryWrapper_v3 {
	public:
		preferences_page_factory_t<CCustomPrefPage_v3> prefPage;
	};

	class CPrefPageCallbackWrapper_v3 {
	public:
		service_ptr_t<preferences_page_callback> m_callback;
	};
};
