/*
  
  fooServices.h


  This file provides basic handling of foobar services. Services are classes that derive from service_base and 
  are created using some factory, like in this example:

  static preferences_page_factory_t<PrefPageImpl> prefPage;

  The component must create them all during initialisation, that is in IComponentClient::Create() method. Creating
  them later throws ServicesDoneException. Also the services shouldn't be freed before the component is unloaded.
  That's why they're registered - to keep them from being collected by the GC.

  All CLI classes that wrap services must follow these rules:
    1. Must implement the IFoobarService interface.
	2. Must register themselves using CManagedWrapper::getInstance()->AddService(this) as the first thing that 
	   is done in their constructor in order to check whether it can be created and to register to the 
	   CManagedWrapper's services list.

*/


#pragma once
#include "Stdafx.h"

using namespace System;
using namespace System::Collections::Generic;

namespace fooManagedWrapper {

	public ref class ServicesDoneException : public Exception {
	public:
		property String ^ Message {
			virtual String ^get() override {
				return "Services have been already initialized, you cannot create any more now.\n"
					"Create your services in the IComponentClient.Create() method.";
			}
		}
	};

	/// <remarks>
	/// Constructor must call CManagedWrapper::getInstance()->AddService(this) as the first thing it does
	/// to ensure checking for the ServicesDoneException
	/// </remarks>
	public interface class IFoobarService {
		
	};

};
