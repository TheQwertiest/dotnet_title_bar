#pragma once
#include "stdafx.h"
#include <vcclr.h>

namespace fooManagedWrapper {

	private ref class CManagedInitQuit {
	public:
		CManagedInitQuit();

		void on_init();
		void on_quit();

	private:
		IPlayControl ^playControl;

	};

	class CInitQuit : public initquit {
	public:
		gcroot<CManagedInitQuit^> managedInitQuit;
		CPlayCallback *playCallback;

		void createPlayCallback();

		virtual void on_init();
		virtual void on_quit();
		virtual void on_system_shutdown();
	};

}