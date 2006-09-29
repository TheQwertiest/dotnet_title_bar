#pragma once
#include "stdafx.h"
#include <vcclr.h>


using namespace fooManagedWrapper;

namespace fooManagedWrapper {

	class CPrefPageFactoryWrapper;

	// to create a preferences page, create a new form (or control) that inherits from this class
	public ref class CManagedPrefPage : public Windows::Forms::ContainerControl, public IFoobarService {
	private:
		CPrefPageFactoryWrapper *wrapper;

	public:
		CManagedPrefPage(System::Guid ^myGuid, System::Guid ^parentGuid);
		virtual ~CManagedPrefPage();
		!CManagedPrefPage();

		static System::Guid ^guid_root, ^guid_hidden, ^guid_tools, ^guid_core, ^guid_display, ^guid_playback, ^guid_visualisations, ^guid_input, ^guid_tag_writing, ^guid_media_library;

	};

	class CCustomPrefPage : public preferences_page {
	protected:		
		gcroot<CManagedPrefPage^> form;	
		GUID guid;
		GUID parentGuid;
		char *name;

		CCustomPrefPage();

	public:

		void SetManagedPrefPage(gcroot<CManagedPrefPage^> a);
		void SetGUID(GUID a);
		void SetParentGUID(GUID a);

		//! Creates preferences page dialog window. It is safe to assume that two dialog instances will never coexist. Caller is responsible for embedding it into preferences dialog itself.
		virtual HWND create(HWND p_parent);
		//! Retrieves name of the prefernces page to be displayed in preferences tree (static string).
		virtual const char * get_name();
		//! Retrieves GUID of the page.
		virtual GUID get_guid();
		//! Retrieves GUID of parent page/branch of this page. See preferences_page::guid_* constants for list of standard parent GUIDs. Can also be a GUID of another page or a branch (see: preferences_branch).
		virtual GUID get_parent_guid() ;
		//! Queries whether this page supports "reset page" feature.
		virtual bool reset_query() {
			return false;
		}
		//! Activates "reset page" feature. It is safe to assume that the preferences page dialog does not exist at the point this is called (caller destroys it before calling reset and creates it again afterwards).
		virtual void reset() {

		}

		virtual ~CCustomPrefPage();

	};

	class CPrefPageFactoryWrapper {
	public:
		preferences_page_factory_t<CCustomPrefPage> prefPage;
	};


};