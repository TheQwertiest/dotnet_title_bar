/*
*  This file is part of foo_title.
*  Copyright 2005 - 2006 Roman Plasil (http://foo-title.sourceforge.net)
*  Copyright 2016 Miha Lepej (https://github.com/LepkoQQ/foo_title)
*  Copyright 2017 TheQwertiest (https://github.com/TheQwertiest/foo_title)
*
*  This library is free software; you can redistribute it and/or
*  modify it under the terms of the GNU Lesser General Public
*  License as published by the Free Software Foundation; either
*  version 2.1 of the License, or (at your option) any later version.
*
*  This library is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
*
*  See the file COPYING included with this distribution for more
*  information.
*/

#pragma once

#include "ComponentLoader.h"
#include "fooServices.h"

using namespace System::Collections;
using namespace System::IO;

namespace fooManagedWrapper {
	// this is the main class of the plugin - created in the entrypoint
	public ref class CManagedWrapper : System::Collections::IEnumerable {
	protected:
		// for singleton
		static CManagedWrapper ^instance;

          bool isStarted = false;

		CComponentLoader ^componentLoader;
		// loaded .NET clients
		TComponentClients ^componentClients;

		// list of created services
		// it's used to prevent the services from being destroyed by the GC before the component quits
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

          String ^ GetNetComponentName();
          String ^ GetNetComponentVersion();
          String ^ GetNetComponentDescription();

		// creates UIControlInstance
		void OnInit();

		void AddService(IFoobarService ^a);

		// for singleton
		static CManagedWrapper ^getInstance();

		// this provides access to all loaded .netcomponents
		virtual System::Collections::IEnumerator ^GetClients() = System::Collections::IEnumerable::GetEnumerator;

		static pfc::string8 StringToPfcString(String ^a);
		static String ^PfcStringToString(const pfc::string8 &stringToConvert);

          static std::string ToStdString( String^ string );

		static _GUID ToGUID(Guid^ guid);
		static Guid ^FromGUID(const _GUID& guid);

		String ^GetProfilePath();
		String ^GetModuleDirectory();

		// TODO provide a better implementation
		static void DoMainMenuCommand(String ^name);
		bool IsFoobarActivated();
		Icon ^GetMainIcon();
		String ^GetAllCommands();
	};
};