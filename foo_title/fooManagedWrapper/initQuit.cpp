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
#include "initQuit.h"
#include "ManagedWrapper.h"

using namespace std;

namespace fooManagedWrapper {

	CManagedInitQuit::CManagedInitQuit() {

	}

	void CManagedInitQuit::on_init() {
		try {
			playControl = gcnew CPlayControl();

			CManagedWrapper::getInstance()->OnInit();

			for each (IComponentClient ^cl in CManagedWrapper::getInstance()) {
				try {
					cl->OnInit(playControl);
				} catch (Exception ^e) {
					fooManagedWrapper::CConsole::Error(e->ToString());
				}
			}
		} catch (Exception ^e) {
			fooManagedWrapper::CConsole::Error(e->ToString());
		}

	}

	void CManagedInitQuit::on_quit() {
		for each (IComponentClient ^cl in CManagedWrapper::getInstance()) {
			cl->OnQuit();
		}
	}


	void CInitQuit::createPlayCallback() {
		playCallback = new CPlayCallback();
	}

	void CInitQuit::on_init() {
		managedInitQuit = gcnew CManagedInitQuit();
		managedInitQuit->on_init();
		createPlayCallback();
	}

	void CInitQuit::on_quit() {
		managedInitQuit->on_quit();
		delete playCallback;
	}

	void CInitQuit::on_system_shutdown() {
		// System shutdown is handled like a normal
		// application exit here.
		on_quit();
	}

	static initquit_factory_t< CInitQuit > initQuit;

}