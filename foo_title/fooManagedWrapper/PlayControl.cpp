#include "..\..\foobar2000\sdk\foobar2000.h"
#include <windows.h>
#include <_vcclrit.h>
#include <vcclr.h>
#include "exported.h"

using namespace fooTitle;
using namespace fooConnector;
using namespace System;

String *CPlayControl::FormatTitle(fooServices::MetaDBHandle *handle, String *spec) {
	metadb_handle * handle_c = handle->GetHandle();
	const char* spec_c = (const char*)(System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(spec)).ToPointer();
	string8 out;

	play_control::get()->playback_format_title_ex(handle_c, out, spec_c, NULL, true, true);

	String *res = new String(out.get_ptr(), 0, out.length(), new System::Text::UTF8Encoding());
	System::Runtime::InteropServices::Marshal::FreeHGlobal(IntPtr((void*)spec_c));
	return res;
}

