using namespace fooTitle;
using namespace fooServices;
using namespace System;

namespace fooConnector {

	public __gc class CPlayControl : public fooServices::IPlayControl {
	public:
		String *FormatTitle(fooServices::MetaDBHandle *handle, String *spec);
	};
};