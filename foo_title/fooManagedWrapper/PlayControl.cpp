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

#include "stdafx.h"
#include "PlayControl.h"
#include "ManagedWrapper.h"


namespace fooManagedWrapper
{

String ^CPlayControl::FormatTitle( CMetaDBHandle ^handle, String ^spec )
{
     if ( handle == nullptr )
          throw gcnew System::ArgumentNullException( "Null CMetaDBHandle supplied to FormatTitle" );

     std::string spec_c( CManagedWrapper::ToStdString( spec ) );

     static_api_ptr_t<titleformat_compiler> titlecompiler;
     service_ptr_t<titleformat_object> compiledScript;
     bool bRet = titlecompiler->compile( compiledScript, spec_c.c_str() );
     if ( !bRet )
          throw gcnew System::ApplicationException( "Script compilation failed" );

     static_api_ptr_t<play_control> pc;
     metadb_handle_ptr tmpHandle = handle->GetHandle();
     if ( tmpHandle.is_empty() )
     {// Fake handle, workaround recommended by foobar2000
          playable_location_impl l;
          static_api_ptr_t<metadb>()->handle_create( tmpHandle, l );
     }

     pfc::string8 out;
     bRet = pc->playback_format_title_ex( tmpHandle, NULL, out, compiledScript, NULL, playback_control::display_level_all );
     if ( !bRet )
          throw gcnew System::ApplicationException( "Script evaluation failed" );

     String ^res = CManagedWrapper::PfcStringToString( out );
     return res;
}

CMetaDBHandle ^CPlayControl::GetNowPlaying()
{
     static_api_ptr_t<play_control> pc;
     metadb_handle_ptr out;
     pc->get_now_playing( out );

     CMetaDBHandle ^res = gcnew CMetaDBHandle( out.get_ptr() );
     return res;
}

double CPlayControl::PlaybackGetPosition()
{
     static_api_ptr_t<play_control> pc;
     return pc->playback_get_position();
}

bool CPlayControl::IsPlaying()
{
     static_api_ptr_t<play_control> pc;
     return pc->is_playing();
}

bool CPlayControl::IsPaused()
{
     static_api_ptr_t<play_control> pc;
     return pc->is_paused();
}

} // namespace fooManagedWrapper