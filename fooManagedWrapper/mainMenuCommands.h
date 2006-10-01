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
#include <vcclr.h>


using namespace fooManagedWrapper;

namespace fooManagedWrapper {

	class CMainMenuCommandsFactoryWrapper;

	public ref class CManagedMainMenuCommands abstract : public IFoobarService {
	protected:
		CMainMenuCommandsFactoryWrapper *wrapper;
	public:
		CManagedMainMenuCommands();
		virtual ~CManagedMainMenuCommands();
		!CManagedMainMenuCommands();

		// now the prototypes for the interface methods
		virtual unsigned int GetCommandsCount() = 0;
		virtual Guid GetGuid(unsigned int index) = 0;
		virtual String ^GetName(unsigned int index) = 0;
		virtual bool GetDescription(unsigned int index, String^ %desc) = 0;
		virtual Guid GetCommandsParent() = 0;
		virtual void Execute(unsigned int index) = 0;
		virtual unsigned int GetCommandsSortPriority() = 0;
		virtual bool GetDisplay(unsigned int index, String^ %text, unsigned int %flags) = 0;

		static System::Guid ^file, ^view, ^edit, ^playback, ^library, ^help;
		
		enum class Flags {
			Disabled = mainmenu_commands::flag_disabled,
			Checked = mainmenu_commands::flag_checked,
			PriorityBase = mainmenu_commands::sort_priority_base,
			PriorityDontCare = mainmenu_commands::sort_priority_dontcare,
			PriorityLast = mainmenu_commands::sort_priority_last
		};
	};

	class CCustomMainMenuCommands : public mainmenu_commands {
	protected:
		gcroot<CManagedMainMenuCommands^> managedMainMenuCommands;
	
	public:
		void SetImplementation(gcroot<CManagedMainMenuCommands ^> impl);

		//! Retrieves number of implemented commands. Index parameter of other methods must be in 0....command_count-1 range.
		virtual t_uint32 get_command_count();
		//! Retrieves GUID of specified command.
		virtual GUID get_command(t_uint32 p_index);
		//! Retrieves name of item, for list of commands to assign keyboard shortcuts to etc.
		virtual void get_name(t_uint32 p_index,pfc::string_base & p_out);
		//! Retrieves item's description for statusbar etc.
		virtual bool get_description(t_uint32 p_index,pfc::string_base & p_out);
		//! Retrieves GUID of owning menu group.
		virtual GUID get_parent();
		
		//! Retrieves sorting priority of the command; the lower the number, the upper in the menu your commands will appear. Third party components should use sorting_priority_base and up (values below are reserved for internal use). In case of equal priority, order is undefined.
		virtual t_uint32 get_sort_priority();
		//! Retrieves display string and display flags to use when menu is about to be displayed. If returns false, menu item won't be displayed. You can create keyboard-shortcut-only commands by always returning false from get_display().
		virtual bool get_display(t_uint32 p_index,pfc::string_base & p_text,t_uint32 & p_flags);
		
		//! Executes the command. p_callback parameter is reserved for future use and should be ignored / set to null pointer.
		virtual void execute(t_uint32 p_index,service_ptr_t<service_base> p_callback);

	};

	class CMainMenuCommandsFactoryWrapper {
	public:
		mainmenu_commands_factory_t< CCustomMainMenuCommands > mainMenuCommands;
	};

};