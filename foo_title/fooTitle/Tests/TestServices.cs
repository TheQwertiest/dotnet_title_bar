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

using System;
using System.Collections.Generic;
using System.Text;

using fooManagedWrapper;

namespace fooTitle.Tests {
    public class TestServices {
        /// <summary>
        /// This value is set only when writing occurs, it is set to the value of futureValueOf_savedOnWrite,
        /// so that it can be compared on next start that it was saved properly.
        /// </summary>
        public CNotifyingCfgString savedOnWrite;
        public CCfgString futureValueOf_savedOnWrite;

        public CNotifyingCfgString testConfigStorage;

        public TestServices() {
            savedOnWrite = new CNotifyingCfgString(new Guid(457, 784, 488, 36, 48, 79, 54, 12, 36, 48, 1), "initial");
            futureValueOf_savedOnWrite = new CCfgString(new Guid(457, 784, 488, 36, 48, 79, 54, 12, 36, 48, 2), "initial");
            testConfigStorage = new CNotifyingCfgString(new Guid(457, 784, 488, 36, 48, 79, 54, 12, 36, 47, 12), "<config/>");
        }
    }
}
