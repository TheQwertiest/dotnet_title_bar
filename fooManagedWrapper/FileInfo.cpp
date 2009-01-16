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
#include "utils.h"
#include "FileInfo.h"

using namespace fooManagedWrapper;

FileInfo::FileInfo(const file_info &src) {
	fileInfo = new file_info_impl(src);
}

FileInfo::!FileInfo() {
	if (fileInfo != NULL) {
		delete fileInfo;
		fileInfo = NULL;
	}
}

bool FileInfo::IsMetaEqual(FileInfo ^info1, FileInfo ^info2) {
	return file_info::g_is_meta_equal(*info1->fileInfo, *info2->fileInfo);
}