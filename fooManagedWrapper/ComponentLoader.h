#pragma once
#include "stdafx.h"

using namespace System;
using namespace System::Collections::Generic;

namespace fooManagedWrapper {
	
	typedef List<IComponentClient^> TComponentClients;

	public ref class ComponentLoader {
	public:

		IComponentClient ^LoadComponent(String ^assemblyName);
		TComponentClients ^LoadComponentsInDir(String ^dirName, String ^filePrefix);
		
	protected:
		IComponentClient ^createInstance(Type ^type);

	};

}