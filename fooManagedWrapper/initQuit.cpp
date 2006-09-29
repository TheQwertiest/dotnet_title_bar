#include "stdafx.h"
#include <vcclr.h>


using namespace fooManagedWrapper;
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
					fooManagedWrapper::Console::Error(e->ToString());
				}
			}
		} catch (Exception ^e) {
			fooManagedWrapper::Console::Error(e->ToString());
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