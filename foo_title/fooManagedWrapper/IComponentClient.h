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

#include "FileInfo.h"
#include "PlayControl.h"


namespace fooManagedWrapper
{

// this is the main entry point for each dotnet_ component - one class must implement it
public interface class IComponentClient {

     String ^ GetName();
     String ^ GetVersion();
     String ^ GetDescription();

     // the component class must create all services in this method
     void Create();

     // this also gives the component an IPlayControl implementation
     void OnInit(IPlayControl ^a);
     void OnQuit();

     // these are the play callbacks
     void OnPlaybackNewTrack(CMetaDBHandle ^h);
     void OnPlaybackTime(double time);
     void OnPlaybackPause(bool state);
     void OnPlaybackStop(IPlayControl::StopReason reason);
     void OnPlaybackDynamicInfoTrack(FileInfo ^fileInfo);
};

}
