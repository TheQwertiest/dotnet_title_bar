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
using System.Windows.Forms;
using System.Collections.Generic;
using System.Reflection;


using fooTitle.Config;

namespace fooTitle.Config {

    public abstract class ControlWrapper : IConfigValueVisitor {
        protected string valueName;
        protected ConfValue value;

        public ControlWrapper(string _valueName) {
            valueName = _valueName;

            associateValue();
            //ConfValuesManager.GetInstance().OnValueChanged += new ConfValuesManager.ValueChangedDelegate(onValueChanged);
            if (value == null)
                ConfValuesManager.GetInstance().OnValueCreated += new ConfValuesManager.ValueChangedDelegate(onValueCreated);
        }

        protected void associateValue() {
            if (value != null)
                return;

            value = ConfValuesManager.GetInstance().GetValueByName(valueName);

            if (value != null) {
                value.OnChanged += new ConfValuesManager.ValueChangedDelegate(onValueChanged);
            }
        }

        protected virtual void onValueChanged(string name) {
            value.ReadVisit(this);
        }

        protected void onValueCreated(string name) {
            if (name != valueName)
                return;

            associateValue();
            value.ReadVisit(this);
        }

        #region IConfigValueVisitor Members

        public virtual void ReadInt(ConfInt val) { }
        public virtual void ReadString(ConfString val) { }
        public virtual void WriteInt(ConfInt val) { }
        public virtual void WriteString(ConfString val) { }

        #endregion
    }

    public class TextBoxWrapper : ControlWrapper, IConfigValueVisitor {
        protected TextBox textBox;

        public TextBoxWrapper(TextBox _textBox, string _valueName)
            : base(_valueName) {
            textBox = _textBox;

            // register onChange event
            textBox.TextChanged += new EventHandler(textBox_TextChanged);

            if (value != null)
                value.ReadVisit(this);
        }

        void textBox_TextChanged(object sender, EventArgs e) {
            if (value == null)
                return;

            if (textBox.Text.Length == 0)
                return;

            value.WriteVisit(this);
        }

        #region IConfigValueVisitor Members

        public override void ReadInt(ConfInt val) {
            textBox.Text = val.Value.ToString();
        }

        public override void ReadString(ConfString val) {
            textBox.Text = val.Value;
        }

        public override void WriteInt(ConfInt val) {
            val.Value = Int32.Parse(textBox.Text);
        }

        public override void WriteString(ConfString val) {
            val.Value = textBox.Text;
        }

        #endregion
    }

    public class TrackBarWrapper : ControlWrapper, IConfigValueVisitor {
        protected TrackBar trackBar;

        public TrackBarWrapper(TrackBar _trackBar, string _valueName)
            : base(_valueName) {
            trackBar = _trackBar;
            trackBar.ValueChanged += new EventHandler(trackBar_ValueChanged);

            if (value != null)
                value.ReadVisit(this);
        }

        void trackBar_ValueChanged(object sender, EventArgs e) {
            if (value == null)
                return;

            value.WriteVisit(this);
        }
        

        /// <summary>
        /// This method sets tick frequency and large/small change values of the trackbar
        /// so that there is a consistent look and feel for any min/max range.
        /// </summary>
        protected void adjustTickFrequency() {
            const int TICK_COUNT = 20;
            const int LARGE_CHANGE_COUNT = 10;
            const int SMALL_CHANGE_COUNT = 100;
            trackBar.TickFrequency = (trackBar.Maximum - trackBar.Minimum) / TICK_COUNT;
            trackBar.LargeChange = (trackBar.Maximum - trackBar.Minimum) / LARGE_CHANGE_COUNT;
            trackBar.SmallChange = (trackBar.Maximum - trackBar.Minimum) / SMALL_CHANGE_COUNT;
        }

        #region IConfigValueVisitor Members

        public override void ReadInt(ConfInt val) {
            trackBar.Minimum = val.Min;
            trackBar.Maximum = val.Max;
            trackBar.Value = val.Value;

            adjustTickFrequency();
        }

        public override void WriteInt(ConfInt val) {
            val.Value = trackBar.Value;
        }

        #endregion
    }

    /// <summary>
    /// This class binds a group of radio buttons to an integer value. The radio buttons
    /// need to be in a group and only one of them may be checked at a time. Use the AddRadioButton
    /// method to add a radioButton to this wrapper.
    /// </summary>
    public class RadioGroupWrapper : ControlWrapper {
        protected List<RadioButton> radioButtons = new List<RadioButton>();
        protected int checkedIndex = 0;

        public RadioGroupWrapper(string _valueName)
            : base(_valueName) {

        }

        /// <summary>
        /// Adds a radiobutton to the group. The order in which radiobuttons are added matters
        /// they map to the integer or enum value in the order they were added.
        /// </summary>
        /// <example>
        /// The following code creates a RadioGroupWrapper for an enumeration value.
        /// <code>
        /// enum WindowState {
        ///     Minimized,
        ///     Normal,
        ///     Maximized
        /// }
        /// 
        /// ConfEnum&lt;WindowState&gt; state = new ConfEnum&lt;WindowState&gt;("state", WindowState.Normal);
        /// RadioGroupWrapper wrapper = new RadioGroupWrapper("state");
        /// wrapper.AddRadioButton(minimizedRadioButton);
        /// wrapper.AddRadioButton(normalRadioButton);
        /// wrapper.AddRadioButton(maximizedRadioButton);
        /// </code>
        /// </example>
        public void AddRadioButton(RadioButton a) {
            radioButtons.Add(a);

            a.CheckedChanged += new EventHandler(onCheckedChanged);

            // try to update the value
            if (value != null) {
                try {
                    value.ReadVisit(this);
                } catch (InvalidOperationException) {
                    // don't worry
                }
            }
        }

        protected void onCheckedChanged(object sender, EventArgs e) {
            // only check for checked on
            if (!((RadioButton)sender).Checked)
                return;

            checkedIndex = getCheckedIndex(sender);
            value.WriteVisit(this);
        }

        protected int getCheckedIndex(object sender) {
            for (int i = 0; i < radioButtons.Count; i++) {
                if (radioButtons[i] == sender) {
                    // this one is true now
                    return i;
                }
            }

            throw new Exception("Got an OnCheckedChanged event for a radiobutton that is not in the group.");
        }

        public override void WriteInt(ConfInt val) {
            val.Value = checkedIndex;
        }

        public override void ReadInt(ConfInt val) {
            if ((val.Value >= radioButtons.Count) || (val.Value < 0))
                throw new InvalidOperationException("The associated value is larger than the number of radiobuttons registered.");

            radioButtons[val.Value].Checked = true;
        }
    }

    public class CheckBoxWrapper : ControlWrapper {
        protected CheckBox checkBox;

        public CheckBoxWrapper(CheckBox _checkBox, string _valueName)
            : base(_valueName) {
            checkBox = _checkBox;

            // register onChange event
            checkBox.CheckedChanged += new EventHandler(checkBox_CheckedChanged);

            if (value != null)
                value.ReadVisit(this);
        }

        void checkBox_CheckedChanged(object sender, EventArgs e) {
            if (value == null)
                return;

            value.WriteVisit(this);
        }

        public override void ReadInt(ConfInt val) {
            checkBox.Checked = (val.Value != 0);
        }

        public override void WriteInt(ConfInt val) {
            val.Value = checkBox.Checked ? 1 : 0;
        }
    }

    public class LabelWrapper : ControlWrapper {
        protected Label label;

        public LabelWrapper(Label _label, string _valueName)
            : base(_valueName) {
            label = _label;

            if (value != null)
                value.ReadVisit(this);
        }

        #region IConfigValueVisitor Members

        public override void ReadInt(ConfInt val) {
            label.Text = val.Value.ToString();
        }

        public override void ReadString(ConfString val) {
            label.Text = val.Value;
        }

        public override void WriteInt(ConfInt val) {
        }

        public override void WriteString(ConfString val) {
        }

        #endregion
    }

    public class AutoWrapperCreator {
        protected object instance;
        protected List<ControlWrapper> controlWrappers = new List<ControlWrapper>();

        public AutoWrapperCreator() {

        }

        public void CreateWrappers(object _instance) {
            instance = _instance;
            Type formType = instance.GetType();

            MemberInfo[] controls = formType.FindMembers(MemberTypes.Field, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance,
                new MemberFilter(controlsMemberFilter), null);

            foreach (MemberInfo m in controls) {
                FieldInfo f = (FieldInfo)m;

                Control value = (Control)f.GetValue(instance);
                if (value == null)
                    continue;
                if (value.Tag == null)
                    continue;
                createWrapper(f, value);
            }
        }

        protected void createWrapper(FieldInfo f, Control value) {

            if (f.FieldType.Equals(typeof(TextBox))) {
                controlWrappers.Add(new TextBoxWrapper((TextBox)value, (string)value.Tag));
            } else if (f.FieldType.Equals(typeof(TrackBar))) {
                controlWrappers.Add(new TrackBarWrapper((TrackBar)value, (string)value.Tag));
            } else if (f.FieldType.Equals(typeof(CheckBox))) {
                controlWrappers.Add(new CheckBoxWrapper((CheckBox)value, (string)value.Tag));
            } else if (f.FieldType.Equals(typeof(Label))) {
                controlWrappers.Add(new LabelWrapper((Label)value, (string)value.Tag));
            }
        }

        protected bool controlsMemberFilter(MemberInfo m, object filterCriteria) {
            return typeof(Control).IsAssignableFrom(((FieldInfo)m).FieldType);
        }
    }
}
