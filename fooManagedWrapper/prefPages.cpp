#include "stdafx.h"
#include <vcclr.h>


using namespace fooManagedWrapper;
using namespace std;

namespace fooManagedWrapper {


	CCustomPrefPage::CCustomPrefPage() {
		name = NULL;
	}

	CCustomPrefPage::~CCustomPrefPage(){ 
		/* don't do this - foobar is using the string even after unloading, so we must keep it
		if (name) {
			delete[] name;
		}
		*/     
	}

	HWND CCustomPrefPage::create(HWND p_parent) {
		SetParent((HWND)form->Handle.ToPointer(), p_parent);
		form->Show();
		return reinterpret_cast<HWND>(form->Handle.ToPointer());
	}

	//! Retrieves name of the prefernces page to be displayed in preferences tree (static string).
	const char * CCustomPrefPage::get_name() {
		if (!name) {
			const char *c = CManagedWrapper::ToCString(form->Text);
			name = new char[strlen(c)];
			strcpy(name, c);
			CManagedWrapper::FreeCString(c);
		}
		return name;
	}

	//! Retrieves GUID of the page.
	GUID CCustomPrefPage::get_guid() {
		return guid;
	}

	//! Retrieves GUID of parent page/branch of this page. See preferences_page::guid_* constants for list of standard parent GUIDs. Can also be a GUID of another page or a branch (see: preferences_branch).
	GUID CCustomPrefPage::get_parent_guid() {
		return parentGuid;
	}

	void CCustomPrefPage::SetManagedPrefPage(gcroot<CManagedPrefPage^> a) {
		form = a;
	}

	void CCustomPrefPage::SetGUID(GUID a) {
		guid = a;
	}

	void CCustomPrefPage::SetParentGUID(GUID a) {
		parentGuid = a;
	}

	CManagedPrefPage::~CManagedPrefPage() {
		delete wrapper;
	}

	CManagedPrefPage::!CManagedPrefPage() {
		this->~CManagedPrefPage();
	}

	CManagedPrefPage::CManagedPrefPage(System::Guid ^myGuid, System::Guid ^parentGuid) {
		CManagedWrapper::getInstance()->AddService(this);

		wrapper = new CPrefPageFactoryWrapper();
		wrapper->prefPage.get_static_instance().SetManagedPrefPage(this);
		wrapper->prefPage.get_static_instance().SetGUID(CManagedWrapper::ToGUID(myGuid));
		wrapper->prefPage.get_static_instance().SetParentGUID(CManagedWrapper::ToGUID(parentGuid));

		
	}



};