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

namespace fooTitle.Tests {
    class test_ManagedWrapper : TestFramework{
        [TestMethod]
        public void testNotifyingString() {
            TestServices testServices = Main.GetInstance().TestServicesInstance;
            AssertEquals(testServices.savedOnWrite.GetVal(), testServices.futureValueOf_savedOnWrite.GetVal());

            // prepare a random value that will be stored to savedOnWrite on write
            Random r = new Random();
            testServices.futureValueOf_savedOnWrite.SetVal(r.Next().ToString());

            fooManagedWrapper.CNotifyingCfgString.BeforeWritingDelegate tester = delegate(fooManagedWrapper.CNotifyingCfgString sender) {
                sender.SetVal(testServices.futureValueOf_savedOnWrite.GetVal());
            };
            //testServices.savedOnWrite.SetVal(testServices.futureValueOf_savedOnWrite.GetVal());
            testServices.savedOnWrite.BeforeWriting += tester;

        }

        [TestMethod]
        public void testMainMenuPopupIterator() {
            bool view = false, file = false, edit = false, playback = false, library=false, help=false;
            foreach (fooManagedWrapper.CManagedMainMenuGroupPopup i in new fooManagedWrapper.CMainMenuGroupPopupEnumerator()) {
                if (i.Name == "File")
                    file = true;
                if (i.Name == "Edit")
                    edit = true;
                if (i.Name == "View")
                    view = true;
                if (i.Name == "Playback")
                    playback = true;
                if (i.Name == "Help")
                    help = true;
                if (i.Name == "Library")
                    library = true;
            }

            AssertEquals(view && file && help && library && edit && playback, true);
        }
    }
}
