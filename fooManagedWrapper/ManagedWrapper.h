#include "stdafx.h"
#include <vcclr.h>

using namespace System::Collections;
using namespace System::IO;

namespace fooManagedWrapper {



	// this is the main class of the plugin - created in the entrypoint
	public ref class CManagedWrapper : System::Collections::IEnumerable {
	protected:
		
		// for singleton
		static CManagedWrapper ^instance;

		ComponentLoader ^componentLoader;
		// loaded .NET clients
		TComponentClients ^componentClients;

		// list of created services
		// it's used to prevent the services to be destroyed by the GC before the component quits
		// because foobar2000 wouldn't like that
		List<IFoobarService^> ^services;

		String ^modulePath;

		static_api_ptr_t<ui_control> *UIControlInstance;

	public:
		CManagedWrapper();
		~CManagedWrapper();
		!CManagedWrapper();
		
		// this loads all .netcomponents and calls their Create() method
		// the parameter is require for this to know the directory where are the .netcomponents
		void Start(String ^_modulePath);

		// creates UIControlInstance
		void OnInit();

		void AddService(IFoobarService ^a);

		// for singleton
		static CManagedWrapper ^getInstance();

		// this provides access to all loaded .netcomponents
		virtual System::Collections::IEnumerator ^GetClients() = System::Collections::IEnumerable::GetEnumerator;

		// general purpose utility functions
		static const char *ToCString(String ^a);
		// must be called on the string returned by ToCString when it's no longer needed
		static void FreeCString(const char *a);
		
		static _GUID ToGUID(Guid^ guid);
		static Guid ^FromGUID(const _GUID& guid);

		// returns the path to the directory where foobar is installed
		String ^GetFoobarDirectory();

		// TODO provide a better implementation
		static void DoMainMenuCommand(String ^name);
		bool IsFoobarActivated();
	};

};