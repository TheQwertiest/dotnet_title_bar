/*
    Copyright 2005 - 2006 Roman Plasil
	http://foo-title.sourceforge.net
    This file is part of foo_title.

    foo_title is free software; you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation; either version 2.1 of the License, or
    (at your option) any later version.

    foo_title is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with foo_title; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

#pragma once

#include "stdafx.h"
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

		CComponentLoader ^componentLoader;
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
		static pfc::string8 StringToPfcString(String ^a);
		
		static _GUID ToGUID(Guid^ guid);
		static Guid ^FromGUID(const _GUID& guid);

		// returns the path to the directory where foobar is installed
		String ^GetFoobarDirectory();
		String ^GetProfilePath();

		// TODO provide a better implementation
		static void DoMainMenuCommand(String ^name);
		bool IsFoobarActivated();
	};

};