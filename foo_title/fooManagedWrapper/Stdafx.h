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

#define CRTDBG_MAP_ALLOC
#include <stdlib.h>
#include <crtdbg.h>

#pragma unmanaged
#include "..\..\foobar2000-sdk\sdk\foobar2000.h"
#include "..\..\foobar2000-sdk\sdk\component.h"
#include <vector>
#pragma managed


#include <vcclr.h>

#define FT_NULL_DELETE(p) { delete (p); (p) = NULL; }