#include "stdafx.h"
#include <vcclr.h>


using namespace fooManagedWrapper;
using namespace System;
using namespace std;

namespace fooManagedWrapper {

	CCfgString::CCfgString(Guid^ _guid, String^ _def) {
		const char *_defString = CManagedWrapper::ToCString(_def);
		wrapper = new CCfgStringWrapper(CManagedWrapper::ToGUID(_guid), _defString);
		CManagedWrapper::FreeCString(_defString);
	};

	CCfgString::!CCfgString() {
		delete wrapper;
	};

	CCfgString::~CCfgString() {
		this->!CCfgString();
	}

	void CCfgString::SetVal(String ^a) {
		const char *_defString = CManagedWrapper::ToCString(a);
		wrapper->val = _defString;
		CManagedWrapper::FreeCString(_defString);
	};


};