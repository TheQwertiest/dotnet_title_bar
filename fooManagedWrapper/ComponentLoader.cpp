#include "stdafx.h"

using namespace fooManagedWrapper;
using namespace System;
using namespace System::Reflection;
using namespace System::IO;


IComponentClient ^ComponentLoader::LoadComponent(System::String ^assemblyName) {
	Type ^iclient = fooManagedWrapper::IComponentClient::typeid;
	Assembly ^a = Assembly::LoadFrom(assemblyName);

	array<Type ^> ^types = a->GetTypes();
	for each (Type ^t in types) {
		if (iclient->IsAssignableFrom(t)) {
			return createInstance(t);
		}
	}

	return nullptr;
};

IComponentClient ^ComponentLoader::createInstance(Type ^type) {
	ConstructorInfo ^conInfo = type->GetConstructor(Type::EmptyTypes);
	IComponentClient ^cl = dynamic_cast<IComponentClient ^>(conInfo->Invoke(nullptr));
	return cl;
}

TComponentClients ^ComponentLoader::LoadComponentsInDir(System::String ^dirName, System::String ^filePrefix) {
	try {
		DirectoryInfo ^di = gcnew DirectoryInfo(dirName);
		array<FileInfo^> ^files = di->GetFiles(filePrefix + "*.dll");

		TComponentClients ^res = gcnew TComponentClients();
		for each (FileInfo^ f in files) {
			try {
				res->Add(LoadComponent(f->FullName));
			} catch (Exception ^e) {
				String ^msg = gcnew String("Error loading .NET component ");
				msg += f->FullName;
				msg += " error message : ";
				msg += e->Message;
				fooManagedWrapper::Console::Error(msg);
				// a problem occured, ok, don't load this component
			}
		}
		return res;
	} catch (Exception ^e) {
		String ^msg = gcnew String("Error loading .NET components from directory ");
		msg += dirName;
		msg += " error message : ";
		msg += e->Message;
		msg += ". You should probably run foobar2000 from the directory containing the components directory.";
		fooManagedWrapper::Console::Error(msg);
		return gcnew TComponentClients(); // don't cause crash
	}
}
