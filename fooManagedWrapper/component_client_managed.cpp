/*
Part of the foobar2000 0.9 SDK
Copyright (c) 2001 - 2006, Peter Pawlowski
Modified by Roman Plasil for foo_managedWrapper and foo_title projects
*/
#include "Stdafx.h"
#include "common.h"

using namespace fooManagedWrapper;

// component_client internals
static HINSTANCE g_hIns;
static pfc::string_simple g_name,g_full_path;
static bool g_services_available = false, g_initialized = false;


// a global handle to the CManagedWrapper instance
static gcroot<CManagedWrapper ^> managedWrapper;


namespace core_api
{

	HINSTANCE get_my_instance()
	{
		return g_hIns;
	}

	HWND get_main_window()
	{
		return g_api->get_main_window();
	}
	pcchar get_my_file_name()
	{
		return g_name;
	}

	pcchar get_my_full_path()
	{
		return g_full_path;
	}

	bool are_services_available()
	{
		return g_services_available;
	}
	bool assert_main_thread()
	{
		return (g_services_available && g_api) ? g_api->assert_main_thread() : true;
	}

	void ensure_main_thread() {
		if (!assert_main_thread()) throw exception_wrong_thread();
	}

	bool is_main_thread()
	{
		return (g_services_available && g_api) ? g_api->is_main_thread() : true;
	}
	pcchar get_profile_path()
	{
		return (g_services_available && g_api) ? g_api->get_profile_path() : 0;
	}

	bool is_shutting_down()
	{
		return (g_services_available && g_api) ? g_api->is_shutting_down() : g_initialized;
	}
	bool is_initializing()
	{
		return (g_services_available && g_api) ? g_api->is_initializing() : !g_initialized;
	}
}

namespace {
	class foobar2000_client_impl : public foobar2000_client
	{
	public:
		t_uint32 get_version() {return FOOBAR2000_CLIENT_VERSION;}
		pservice_factory_base get_service_list() {
			return service_factory_base::__internal__list;
		}

		void get_config(stream_writer * p_stream,abort_callback & p_abort) {
			cfg_var::config_write_file(p_stream,p_abort);
		}

		void set_config(stream_reader * p_stream,abort_callback & p_abort) {
			cfg_var::config_read_file(p_stream,p_abort);
		}

		void set_library_path(const char * path,const char * name) {
			g_full_path = path;
			g_name = name;
		}

		void services_init(bool val) {
			if (val) g_initialized = true;
			g_services_available = val;
		}

		bool is_debug() {
#ifdef _DEBUG
			return true;
#else
			return false;
#endif
		}
		
	};
}

static foobar2000_client_impl g_client;


extern "C"
{
	__declspec(dllexport) foobar2000_client * _cdecl foobar2000_get_interface(foobar2000_api * p_api,HINSTANCE hIns)
	{
		g_hIns = hIns;
		g_api = p_api;

		// find out module path
		TCHAR *buf = new TCHAR[300];
		GetModuleFileName(hIns, buf, 300);
		String ^modulePath = gcnew String(buf);
		delete[] buf;

		// this is to allow creating of IFoobarService's before the component is initialized
		managedWrapper = gcnew CManagedWrapper();
		managedWrapper->Start(modulePath);

		return &g_client;
	}
}


#pragma unmanaged
BOOLEAN WINAPI TestEntry(IN HINSTANCE hDllHandle, IN DWORD     nReason,  IN LPVOID  Reserved ) {
	// this fixes a problem with drag'n'drop
	// thanks to FrozenSpoon for the solution :)
	// As of foobar 0.9.5.4 this seems to be no longer required and it conflicts with DirectSound in Vista
	//CoInitializeEx(0, COINIT_APARTMENTTHREADED);
	
	if (nReason == DLL_PROCESS_DETACH) {
		//_CrtDumpMemoryLeaks();
	} else if (nReason == DLL_PROCESS_ATTACH) {
		//_CrtSetBreakAlloc(200);
		_CrtSetDbgFlag ( _CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF );
	}
	return true;
}

#pragma managed


#if 0
BOOLEAN WINAPI DllMain(IN HINSTANCE hDllHandle, IN DWORD     nReason,  IN LPVOID  Reserved )
{
    BOOLEAN bSuccess = TRUE;

    switch ( nReason ) {
        case DLL_PROCESS_ATTACH:

            DisableThreadLibraryCalls( hDllHandle );

            break;

        case DLL_PROCESS_DETACH:

            break;
    }
	return TRUE;
}
#endif