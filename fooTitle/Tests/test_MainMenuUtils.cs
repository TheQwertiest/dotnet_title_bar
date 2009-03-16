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
    class test_MainMenuUtils : TestFramework {

        [TestMethod]
        public void testFindCommandByPath() {
            CMainMenuCommands cmds;
            uint index;

            AssertEquals(MainMenuUtils.FindCommandByPath("Playback/Order/Random", out cmds, out index), true);
            AssertEquals(MainMenuUtils.FindCommandByPath("nocommand/nocommand/nocommand", out cmds, out index), false);
            AssertEquals(MainMenuUtils.FindCommandByPath("Help/About", out cmds, out index), true);
            AssertEquals(MainMenuUtils.FindCommandByPath("File/Preferences", out cmds, out index), true);
            AssertEquals(MainMenuUtils.FindCommandByPath("File/nocommand", out cmds, out index), false);
            AssertEquals(MainMenuUtils.FindCommandByPath("Playback/Order/Random/nocommand", out cmds, out index), false);
            AssertEquals(MainMenuUtils.FindCommandByPath("Playback/Order/nocommand", out cmds, out index), false);
        }

        [TestMethod]
        public void testContextMenuItemEnum() {
            bool found = false;
            foreach (CContextMenuItem item in new CContextMenuItemEnumerator()) {
                for (uint i = 0; i < item.Count; i++) {
                    if (item.GetName(i) == "Properties")
                        found = true;
                }
            }

            AssertEquals(true, found);
        }
    }
}
