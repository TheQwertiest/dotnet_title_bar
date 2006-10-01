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
#include <vcclr.h>


using namespace fooManagedWrapper;
using namespace std;

namespace fooManagedWrapper {

	CManagedMainMenuCommands::CManagedMainMenuCommands() {
		CManagedWrapper::getInstance()->AddService(this);

		wrapper = new CMainMenuCommandsFactoryWrapper();
		wrapper->mainMenuCommands.get_static_instance().SetImplementation(this);
	};

	CManagedMainMenuCommands::!CManagedMainMenuCommands() {
		this->~CManagedMainMenuCommands();
	}

	CManagedMainMenuCommands::~CManagedMainMenuCommands() {
		delete wrapper;
	}



	void CCustomMainMenuCommands::SetImplementation(gcroot<CManagedMainMenuCommands ^> impl) {
		managedMainMenuCommands = impl;
	}

	t_uint32 CCustomMainMenuCommands::get_command_count() {
		return managedMainMenuCommands->GetCommandsCount();
	}

	GUID CCustomMainMenuCommands::get_command(t_uint32 p_index) {
		return CManagedWrapper::ToGUID(managedMainMenuCommands->GetGuid(p_index));
	}

	void CCustomMainMenuCommands::get_name(t_uint32 p_index, pfc::string_base & p_out) {
		const char *c_str = CManagedWrapper::ToCString(managedMainMenuCommands->GetName(p_index));
		p_out = c_str;
		CManagedWrapper::FreeCString(c_str);
	}

	bool CCustomMainMenuCommands::get_description(t_uint32 p_index, pfc::string_base &p_out) {
		String ^str2 = gcnew String("");
		bool res;
		res = managedMainMenuCommands->GetDescription(p_index, (String ^)str2);
		const char *c_str = CManagedWrapper::ToCString(str2);
		p_out = c_str;
		CManagedWrapper::FreeCString(c_str);
		return res;
	}

	GUID CCustomMainMenuCommands::get_parent() {
		return CManagedWrapper::ToGUID(managedMainMenuCommands->GetCommandsParent());
	}

	void CCustomMainMenuCommands::execute(t_uint32 p_index, service_ptr_t<service_base> p_callback) {
		managedMainMenuCommands->Execute(p_index);
	}

	unsigned int CCustomMainMenuCommands::get_sort_priority() {
		return managedMainMenuCommands->GetCommandsSortPriority();
	}
	
	bool CCustomMainMenuCommands::get_display(t_uint32 p_index,pfc::string_base & p_text,t_uint32 & p_flags) {
		String ^str = gcnew String("");
		bool res = managedMainMenuCommands->GetDisplay(p_index, str, p_flags);
		const char *c_str = CManagedWrapper::ToCString(str);
		p_text = c_str;
		CManagedWrapper::FreeCString(c_str);
		return res;
	}

};