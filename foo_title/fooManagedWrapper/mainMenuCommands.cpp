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
#include "mainMenuCommands.h"
#include "ManagedWrapper.h"
#include "utils.h"


using namespace fooManagedWrapper;
using namespace std;
using namespace System::Collections::Generic;
using namespace System::Text;

namespace fooManagedWrapper {

	CMainMenuCommandsImpl::CMainMenuCommandsImpl(List<CCommand ^> ^_cmds) {
		commonInit();
		cmds = _cmds;
	};

	CMainMenuCommandsImpl::CMainMenuCommandsImpl() {
		commonInit();
	}

	void CMainMenuCommandsImpl::commonInit() {
		cmds = gcnew List<CCommand ^>();
		CManagedWrapper::getInstance()->AddService(this);

		wrapper = new CMainMenuCommandsFactoryWrapper();
		wrapper->mainMenuCommands.get_static_instance().SetImplementation(this);
	}

	CMainMenuCommandsImpl::!CMainMenuCommandsImpl() {
		this->~CMainMenuCommandsImpl();
	}

	CMainMenuCommandsImpl::~CMainMenuCommandsImpl() {
		delete wrapper;
	}

	unsigned int CMainMenuCommandsImpl::CommandsCount::get() {
		return cmds->Count;
	}

	Guid CMainMenuCommandsImpl::GetGuid(unsigned int index) {
		return cmds[index]->GetGuid();
	}

	String ^CMainMenuCommandsImpl::GetName(unsigned int index) {
		return cmds[index]->GetName();
	}

	bool CMainMenuCommandsImpl::GetDescription(unsigned int index, String ^ %desc) {
		return cmds[index]->GetDescription(desc);
	}

	void CMainMenuCommandsImpl::Execute(unsigned int index) {
		return cmds[index]->Execute();
	}

	bool CMainMenuCommandsImpl::GetDisplay(unsigned int index, String ^ %text, unsigned int %flags) {
		return cmds[index]->GetDisplay(text, flags);
	}

	void CCustomMainMenuCommands::SetImplementation(gcroot<CMainMenuCommandsImpl ^> impl) {
		managedMainMenuCommands = impl;
	}

	t_uint32 CCustomMainMenuCommands::get_command_count() {
		return managedMainMenuCommands->CommandsCount;
	}

	GUID CCustomMainMenuCommands::get_command(t_uint32 p_index) {
		return CManagedWrapper::ToGUID(managedMainMenuCommands->GetGuid(p_index));
	}

	void CCustomMainMenuCommands::get_name(t_uint32 p_index, pfc::string_base & p_out) {
		p_out = CManagedWrapper::StringToPfcString(managedMainMenuCommands->GetName(p_index));
	}

	bool CCustomMainMenuCommands::get_description(t_uint32 p_index, pfc::string_base &p_out) {
		String ^str;
		bool res = managedMainMenuCommands->GetDescription(p_index, str);
		p_out = CManagedWrapper::StringToPfcString(str);
		return res;
	}

	GUID CCustomMainMenuCommands::get_parent() {
		return CManagedWrapper::ToGUID(managedMainMenuCommands->Parent);
	}

	void CCustomMainMenuCommands::execute(t_uint32 p_index, service_ptr_t<service_base> p_callback) {
		managedMainMenuCommands->Execute(p_index);
	}

	unsigned int CCustomMainMenuCommands::get_sort_priority() {
		return managedMainMenuCommands->SortPriority;
	}
	
	bool CCustomMainMenuCommands::get_display(t_uint32 p_index,pfc::string_base & p_text,t_uint32 & p_flags) {
		String ^str;
		bool res = managedMainMenuCommands->GetDisplay(p_index, str, p_flags);
		p_text = CManagedWrapper::StringToPfcString(str);
		return res;
	}



	CMainMenuCommands::CMainMenuCommands(const service_ptr_t<mainmenu_commands> &_ptr) {
		this->ptr = new service_ptr_t<mainmenu_commands>(_ptr);
	}

	CMainMenuCommands::~CMainMenuCommands() {
		SAFE_DELETE(ptr);
	}

	String ^CMainMenuCommands::GetName(unsigned int index) {
		pfc::string8 pfcName;
		(*ptr)->get_name(index, pfcName);
		return CManagedWrapper::PfcStringToString(pfcName);
	}

	Guid CMainMenuCommands::Parent::get() {
		return safe_cast<Guid>(CManagedWrapper::FromGUID((*ptr)->get_parent()));
	}

	Guid ^CMainMenuCommands::GetCommand(unsigned int index) {
		return CManagedWrapper::FromGUID((*ptr)->get_command(index));
	}

	void CMainMenuCommands::Execute(unsigned int index) {
		(*ptr)->execute(index, NULL);
	}

};