#pragma once
#include "stdafx.h"
#include <vcclr.h>


using namespace System;
using namespace fooManagedWrapper;

namespace fooManagedWrapper {

	public ref class CCfgVar abstract : public IFoobarService {
	public:
		CCfgVar() {
			CManagedWrapper::getInstance()->AddService(this);
		}
	};

	template<typename CFG_TYPE, typename NATIVE_TYPE>
	class CCfgGenericWrapper {
	public:
		CFG_TYPE val;

		CCfgGenericWrapper(const GUID &_guid, NATIVE_TYPE _def) : val(_guid, _def) {};
	};

	typedef CCfgGenericWrapper<cfg_int, int> CCfgIntWrapper;
	typedef CCfgGenericWrapper<cfg_uint, unsigned int> CCfgUIntWrapper;
	typedef CCfgGenericWrapper<cfg_bool, bool> CCfgBoolWrapper;
	typedef CCfgGenericWrapper<cfg_string, const char *> CCfgStringWrapper;

	// these are the configuration classes, they must be created in the component's Create method
	// (like any other service)
	public ref class CCfgInt : public CCfgVar {
	protected:
		CCfgIntWrapper *wrapper;

	public:
		// each variable must have a guid
		// _def is the default value
		CCfgInt(Guid^ _guid, int _def) {
			wrapper = new CCfgIntWrapper(CManagedWrapper::ToGUID(_guid), _def);
		};

		!CCfgInt() {
			delete wrapper;
		};

		virtual ~CCfgInt() {
			this->!CCfgInt();
		}

		int GetVal() {
			return wrapper->val;
		};

		void SetVal(int a) {
			wrapper->val = a;
		};
	};

	public ref class CCfgUInt : public CCfgVar {
	protected:
		CCfgUIntWrapper *wrapper;

	public:
		CCfgUInt(Guid^ _guid, unsigned int _def) {
			wrapper = new CCfgUIntWrapper(CManagedWrapper::ToGUID(_guid), _def);
		};

		!CCfgUInt() {
			delete wrapper;
		};

		virtual ~CCfgUInt(){
			this->!CCfgUInt();
		}

		unsigned int GetVal() {
			return wrapper->val;
		};
	
		void SetVal(unsigned int a) {
			wrapper->val = a;
		};
	};

	public ref class CCfgBool : public CCfgVar {
	protected:
		CCfgBoolWrapper *wrapper;

	public:
		CCfgBool(Guid^ _guid, bool _def) {
			wrapper = new CCfgBoolWrapper(CManagedWrapper::ToGUID(_guid), _def);
		};

		!CCfgBool() {
			delete wrapper;
		};

		virtual ~CCfgBool() {
			this->!CCfgBool();
		}

		bool GetVal() {
			return wrapper->val;
		};
	
		void SetVal(bool a) {
			wrapper->val = a;
		};
	};


	public ref class CCfgString : public CCfgVar {
	protected:
		CCfgStringWrapper *wrapper;

	public:
		CCfgString(Guid^ _guid, String^ _def);
		virtual ~CCfgString();
		!CCfgString();

		String ^GetVal() {
			return gcnew String(wrapper->val);
		};

		void SetVal(String ^a);
	};


};