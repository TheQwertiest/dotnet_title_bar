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
#include "stdafx.h"
#include "component_version.h"
#include "ManagedWrapper.h"

VALIDATE_COMPONENT_FILENAME( "foo_managed_wrapper.dll" );

DECLARE_COMPONENT_VERSION(
// component name
"Managed Wrapper (.NET)",
// component version
FOO_MANAGED_WRAPPER_VERSION,
// component description
"foo_managed_wrapper\n"
".NET component loader for foo_title\n\n"
"Copyright( c ) 2005-2006 by Roman Plasil\n\n"
"Copyright( c ) 2016 by Miha Lepej\n"
"https://github.com/LepkoQQ/foo_title \n\n"
"Copyright( c ) 2017 by TheQwertiest\n"
"https://github.com/TheQwertiest/foo_title"
)

namespace
{

using namespace fooManagedWrapper;

class dontet_componentversion_impl : public componentversion
{
public: dontet_componentversion_impl()
{
} 
void get_file_name( pfc::string_base & out )
{
     out = core_api::get_my_file_name();
}	
void get_component_name( pfc::string_base & out )
{
     out = CManagedWrapper::StringToPfcString( CManagedWrapper::getInstance()->GetNetComponentName() );
}	
void get_component_version( pfc::string_base & out )
{
     out = CManagedWrapper::StringToPfcString( CManagedWrapper::getInstance()->GetNetComponentVersion() );
}	
void get_about_message( pfc::string_base & out )
{
     out = CManagedWrapper::StringToPfcString( CManagedWrapper::getInstance()->GetNetComponentDescription() );
}	
};

static service_factory_single_t<dontet_componentversion_impl> g_dotnet_componentversion_factory;
}
