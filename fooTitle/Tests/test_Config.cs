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
using fooTitle.Config;

namespace fooTitle.Tests {
    class test_Config : TestFramework {
        [TestMethod]
        public void storageTest() {
            IConfigStorage cfg = Main.GetInstance().Config;
            cfg.WriteVal("test/Bear", "Teddy");
            AssertEquals(cfg.ReadVal<string>("test/Bear"), "Teddy");

            cfg.WriteVal("test/MouseSize", 12);
            AssertEquals(cfg.ReadVal<int>("test/MouseSize"), 12);

            cfg.WriteVal("test/Bear", "Grizzly");
            AssertEquals(cfg.ReadVal<string>("test/Bear"), "Grizzly");
            fooManagedWrapper.Console.Write(cfg.ToString());
        }


        enum LunchType {
            luBreakfast,
            luMeal,
            luDinner
        }

        ConfInt interval = new ConfInt("test/confInt", 15);
        //ConfEnum<LunchType> lunch = new ConfEnum<LunchType>("test/lunchType", LunchType.luMeal, new string[] {"breakfastRadio", "mealRadio", "dinnerRadio" });
        ConfString skinName = new ConfString("test/skinName", "white");

        [TestMethod]
        public void typedTest() {
            // test default value
            AssertEquals(interval.Value, 15);

            // test double assigment
            interval.Value = 34;
            AssertEquals(interval.Value, 34);
            interval.Value = 0;
            AssertEquals(interval.Value, 0);

            // now string
            AssertEquals(skinName.Value, "white");

            skinName.Value = "abc";
            AssertEquals(skinName.Value, "abc");

        }
        
        [TestMethod]
        public void testWrongType() {
            IConfigStorage cfg = Main.GetInstance().Config;
            AssertExceptionThrown<UnsupportedTypeException>(delegate(){ cfg.ReadVal<System.Drawing.Point>("test/Bear"); });
        }

        [TestMethod]
        public void testIntBounds() {
            ConfInt a = new ConfInt("test/temp", 15, 10, 20);
            bool eventFired = false;

            ConfValuesManager.ValueChangedDelegate tester = delegate(string name) {
                if (name == "test/temp")
                    eventFired = true;
            };

            ConfValuesManager.GetInstance().OnValueChanged += tester;
            a.Value = 100;
            AssertEquals(eventFired, true); // first time, changing from 15 to 20
            eventFired = false;

            a.Value = 200;
            AssertEquals(eventFired, false); 
            eventFired = false;

            a.Value = 1;
            AssertEquals(eventFired, true); 
            eventFired = false;

            a.Value = 2;
            AssertEquals(eventFired, false);
            eventFired = false;
            
            a.Unregister();
        }
        

        ConfInt paperCount = new ConfInt("test/paperCount", 10);
        ConfString bearName = new ConfString("test/bearName", "Teddy");
        ConfEnum<LunchType> lunchType = new ConfEnum<LunchType>("test/lunchType", LunchType.luBreakfast);

        [TestMethod]
        public void testSavingThroughEvents() {
            AssertEquals(paperCount.Value, 10);
            AssertEquals(lunchType.Value, LunchType.luBreakfast);

            IConfigStorage temp = Main.GetInstance().Config;
            paperCount.Value = 2;
            lunchType.Value = LunchType.luMeal;
            ConfValuesManager.GetInstance().SaveTo(temp);
            paperCount.Value = 3;
            lunchType.Value = LunchType.luDinner;
            ConfValuesManager.GetInstance().LoadFrom(temp);
            AssertEquals(paperCount.Value, 2);
            AssertEquals(lunchType.Value, LunchType.luMeal);


            AssertEquals(bearName.Value, "Teddy");
        }

        ConfInt aCount = new ConfInt("test/aCount", 0, -10, 20);
        
        test_ConfigGUI configGui = new test_ConfigGUI();
        [TestMethod]
        public void testTextBox() {
            configGui.Show();
            AssertEquals(configGui.textBox1.Text, "0");  // initial value

            configGui.textBox2.Text = "12";
            AssertEquals(aCount.Value, 12);
        }

        ConfInt bCount = new ConfInt("test/bCount", 100, 0, 100);
        [TestMethod]
        public void testTrackbar() {
            configGui.Show();
            AssertEquals(configGui.trackBar1.Value, 100);

            configGui.trackBar1.Value = 50;
            AssertEquals(configGui.trackBar1.Value, 50);

            AssertEquals(configGui.trackBar1.Maximum, bCount.Max);
            AssertEquals(configGui.trackBar1.Minimum, bCount.Min);

            bCount.Value = 1000; // test overflow

            // test automatic tick frequency and so on
            AssertEquals(configGui.trackBar1.TickFrequency, 5);
            AssertEquals(configGui.trackBar1.LargeChange, 10);
            AssertEquals(configGui.trackBar1.SmallChange, 1);
        }

        ConfInt cCount = new ConfInt("test/cCount", 0, 0, 4);
        [TestMethod]
        public void testRadioGroup() {
            AssertEquals(configGui.radioButton1.Checked, true);
            AssertEquals(configGui.radioButton2.Checked, false);
            AssertEquals(configGui.radioButton3.Checked, false);

            cCount.Value = 1;
            AssertEquals(configGui.radioButton1.Checked, false);
            AssertEquals(configGui.radioButton2.Checked, true);
            AssertEquals(configGui.radioButton3.Checked, false);


            configGui.radioButton3.Checked = true;
            AssertEquals(cCount.Value, 2);

            // test overflow
            AssertExceptionThrown<InvalidOperationException>(delegate() { cCount.Value = 4; });

        }

        ConfInt dCount = new ConfInt("test/dCount", 20, 10, 30);
        [TestMethod]
        public void testAutoWrapperCreating() {
            AssertEquals(configGui.textBox3.Text, "20");
            AssertEquals(configGui.trackBar2.Value, 20);
        }

        [TestMethod]
        public void testMultipleValues() {
            ConfInt eCount = new ConfInt("test/eCount", 1);
            AssertExceptionThrown<ValueAlreadyExistsException>(delegate() { new ConfInt("test/eCount", 1); });
            eCount.Unregister();
        }

        [TestMethod]
        public void testCreationSequence() {
            ConfInt fValue = new ConfInt("test/fValue", 10, 10, 100);
            test_ConfigGUI gui2 = new test_ConfigGUI();
            TrackBarWrapper wrapper3 = new TrackBarWrapper(gui2.trackBar3, "test/fValue");
            gui2.Show();

            AssertEquals(gui2.trackBar3.Value, fValue.Value);

            TextBoxWrapper wrapper = new TextBoxWrapper(gui2.textBox4, "test/aValue");
            ConfString aValue = new ConfString("test/aValue", "aaa");

            AssertEquals(gui2.textBox4.Text, "aaa");

            ConfString bValue = new ConfString("test/bValue", "bbb");
            TextBoxWrapper wrapper2 = new TextBoxWrapper(gui2.textBox5, "test/bValue");

            AssertEquals(gui2.textBox5.Text, "bbb");

            fValue.Unregister();
            aValue.Unregister();
            bValue.Unregister();

            gui2.Dispose();
        }

        [TestMethod]
        public void testBool() {
            ConfBool aBool = new ConfBool("test/aBool", true);
            AssertEquals(configGui.checkBox1.Checked, true);

            aBool.Value = false;
            AssertEquals(configGui.checkBox1.Checked, false);

            configGui.checkBox1.Checked = true;
            AssertEquals(aBool.Value, true);

        }
    }




}
