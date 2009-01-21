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

// this creates and registers a new menu group popup
CMainMenuGroupPopup::CMainMenuGroupPopup(Guid ^guid, Guid ^parent, int priority, String ^ name) {
	CManagedWrapper::getInstance()->AddService(this);
	const char *c_name = CManagedWrapper::ToCString(name);
	wrapper = new mainmenu_group_popup_factory(
		CManagedWrapper::ToGUID(guid),
		CManagedWrapper::ToGUID(parent),
		priority,
		c_name);
	CManagedWrapper::FreeCString(c_name);

	ptr = new service_ptr_t<mainmenu_group_popup>(&wrapper->get_static_instance());
}

// this constructor wraps an existing menu group popup
CMainMenuGroupPopup::CMainMenuGroupPopup(const service_ptr_t<mainmenu_group_popup> &existingPtr) {
	// not adding this service to the CManagedWrapper's list because we are not creating a new one
	wrapper = NULL;
	ptr = new service_ptr_t<mainmenu_group_popup>(existingPtr);
}



bool CMainMenuGroupPopupEnumerator::MoveNext() {
	service_ptr_t<mainmenu_group> i;

	if (current == NULL)
		current = new service_ptr_t<mainmenu_group_popup>();

	do {
		if (!enumerator->next(i)) return false;
	} while (!i->service_query_t<mainmenu_group_popup>(*current));

	return true;

}

};