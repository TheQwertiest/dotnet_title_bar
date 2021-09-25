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
using Qwr.ComponentInterface;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace fooTitle.Config
{

    public abstract class ControlWrapper : IConfigValueVisitor
    {
        protected ConfValue value;
        private readonly string _valueName;
        private readonly IPreferencesPageCallback _parentCallback;

        protected ControlWrapper(string valueName, IPreferencesPageCallback parentCallback)
        {
            _valueName = valueName;
            _parentCallback = parentCallback;

            AssociateValue();
            //ConfValuesManager.GetInstance().OnValueChanged += new ConfValuesManager.ValueChangedDelegate(onValueChanged);
            if (value == null)
                ConfValuesManager.GetInstance().OnValueCreated += value_OnCreated;
        }

        private void AssociateValue()
        {
            if (value != null)
                return;

            value = ConfValuesManager.GetInstance().GetValueByName(_valueName);

            if (value != null)
            {
                value.OnChanged += value_OnChanged;
            }
        }

        private void value_OnChanged(string name)
        {
            value.ReadVisit(this);
            _parentCallback.OnStateChanged();
        }

        private void value_OnCreated(string name)
        {
            if (name != _valueName)
                return;

            AssociateValue();
            value.ReadVisit(this);
        }

        #region IConfigValueVisitor Members

        public virtual void ReadInt(ConfInt val) { }
        public virtual void ReadString(ConfString val) { }
        public virtual void WriteInt(ConfInt val) { }
        public virtual void WriteString(ConfString val) { }

        #endregion
    }

    public class TextBoxWrapper : ControlWrapper
    {
        private readonly TextBox _textBox;

        public TextBoxWrapper(TextBox textBox, string valueName, IPreferencesPageCallback parentCallback) : base(valueName, parentCallback)
        {
            _textBox = textBox;

            // register onChange event
            _textBox.TextChanged += textBox_TextChanged;

            value?.ReadVisit(this);
        }

        void textBox_TextChanged(object sender, EventArgs e)
        {
            if (value == null)
                return;

            if (_textBox.Text.Length == 0)
                return;

            value.WriteVisit(this);
        }

        #region IConfigValueVisitor Members

        public override void ReadInt(ConfInt val)
        {
            _textBox.Text = val.Value.ToString();
        }

        public override void ReadString(ConfString val)
        {
            _textBox.Text = val.Value;
        }

        public override void WriteInt(ConfInt val)
        {
            val.Value = Int32.Parse(_textBox.Text);
        }

        public override void WriteString(ConfString val)
        {
            val.Value = _textBox.Text;
        }

        #endregion
    }

    public class TrackBarWrapper : ControlWrapper
    {
        private readonly TrackBar _trackBar;

        public TrackBarWrapper(TrackBar trackBar, string valueName, IPreferencesPageCallback parentCallback) : base(valueName, parentCallback)
        {
            _trackBar = trackBar;
            _trackBar.ValueChanged += trackBar_ValueChanged;

            value?.ReadVisit(this);
        }

        void trackBar_ValueChanged(object sender, EventArgs e)
        {
            value?.WriteVisit(this);
        }


        /// <summary>
        /// This method sets tick frequency and large/small change values of the trackbar
        /// so that there is a consistent look and feel for any min/max range.
        /// </summary>
        private void AdjustTickFrequency()
        {
            const int tickCount = 20;
            const int largeChangeCount = 10;
            const int smallChangeCount = 100;
            _trackBar.TickFrequency = (_trackBar.Maximum - _trackBar.Minimum) / tickCount;
            _trackBar.LargeChange = (_trackBar.Maximum - _trackBar.Minimum) / largeChangeCount;
            _trackBar.SmallChange = (_trackBar.Maximum - _trackBar.Minimum) / smallChangeCount;
        }

        #region IConfigValueVisitor Members

        public override void ReadInt(ConfInt val)
        {
            _trackBar.Minimum = val.Min;
            _trackBar.Maximum = val.Max;
            _trackBar.Value = val.Value;

            AdjustTickFrequency();
        }

        public override void WriteInt(ConfInt val)
        {
            val.Value = _trackBar.Value;
        }

        #endregion
    }

    public class NumericUpDownWrapper : ControlWrapper
    {
        private readonly NumericUpDown _numericUpDown;

        public NumericUpDownWrapper(NumericUpDown numericUpDown, string valueName, IPreferencesPageCallback parentCallback) : base(valueName, parentCallback)
        {
            _numericUpDown = numericUpDown;
            _numericUpDown.ValueChanged += numericUpDown_ValueChanged;

            value?.ReadVisit(this);
        }

        void numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            value?.WriteVisit(this);
        }

        #region IConfigValueVisitor Members

        public override void ReadInt(ConfInt val)
        {
            _numericUpDown.Minimum = val.Min;
            _numericUpDown.Maximum = val.Max;
            _numericUpDown.Value = val.Value;
        }

        public override void WriteInt(ConfInt val)
        {
            val.Value = (int)_numericUpDown.Value;
        }

        #endregion
    }

    /// <summary>
    /// This class binds a group of radio buttons to an integer value. The radio buttons
    /// need to be in a group and only one of them may be checked at a time. Use the AddRadioButton
    /// method to add a radioButton to this wrapper.
    /// </summary>
    public class RadioGroupWrapper : ControlWrapper
    {
        private readonly List<RadioButton> _radioButtons = new List<RadioButton>();
        private int _checkedIndex = 0;

        public RadioGroupWrapper(string valueName, IPreferencesPageCallback parentCallback) : base(valueName, parentCallback) { }

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
        public void AddRadioButton(RadioButton a)
        {
            _radioButtons.Add(a);

            a.CheckedChanged += OnCheckedChanged;

            // try to update the value
            if (value != null)
            {
                try
                {
                    value.ReadVisit(this);
                }
                catch (InvalidOperationException)
                {
                    // don't worry
                }
            }
        }

        private void OnCheckedChanged(object sender, EventArgs e)
        {
            // only check for checked on
            if (!((RadioButton)sender).Checked)
                return;

            _checkedIndex = GetCheckedIndex(sender);
            value.WriteVisit(this);
        }

        private int GetCheckedIndex(object sender)
        {
            for (int i = 0; i < _radioButtons.Count; i++)
            {
                if (_radioButtons[i] == sender)
                {
                    // this one is true now
                    return i;
                }
            }

            throw new Exception("Got an OnCheckedChanged event for a radiobutton that is not in the group.");
        }

        public override void WriteInt(ConfInt val)
        {
            val.Value = _checkedIndex;
        }

        public override void ReadInt(ConfInt val)
        {
            if ((val.Value >= _radioButtons.Count) || (val.Value < 0))
                throw new InvalidOperationException("The associated value is larger than the number of radiobuttons registered.");

            _radioButtons[val.Value].Checked = true;
        }
    }

    public class CheckBoxWrapper : ControlWrapper
    {
        private readonly CheckBox _checkBox;

        public CheckBoxWrapper(CheckBox checkBox, string valueName, IPreferencesPageCallback parentCallback) : base(valueName, parentCallback)
        {
            _checkBox = checkBox;

            // register onChange event
            _checkBox.CheckedChanged += checkBox_CheckedChanged;

            value?.ReadVisit(this);
        }

        void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            value?.WriteVisit(this);
        }

        public override void ReadInt(ConfInt val)
        {
            _checkBox.Checked = (val.Value != 0);
        }

        public override void WriteInt(ConfInt val)
        {
            val.Value = _checkBox.Checked ? 1 : 0;
        }
    }

    public class LabelWrapper : ControlWrapper
    {
        private readonly Label _label;

        public LabelWrapper(Label label, string valueName, IPreferencesPageCallback parentCallback) : base(valueName, parentCallback)
        {
            _label = label;

            value?.ReadVisit(this);
        }

        #region IConfigValueVisitor Members

        public override void ReadInt(ConfInt val)
        {
            _label.Text = val.Value.ToString();
        }

        public override void ReadString(ConfString val)
        {
            _label.Text = val.Value;
        }

        public override void WriteInt(ConfInt val) { }

        public override void WriteString(ConfString val) { }

        #endregion
    }

    public class AutoWrapperCreator
    {
        private ContainerControl _instance;
        private readonly List<ControlWrapper> _controlWrappers = new List<ControlWrapper>();

        public void CreateWrappers(ContainerControl instance, IPreferencesPageCallback parentCallback)
        {
            _instance = instance;
            Type formType = _instance.GetType();

            MemberInfo[] controls = formType.FindMembers(MemberTypes.Field, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance,
                ControlsMemberFilter, null);

            foreach (MemberInfo m in controls)
            {
                FieldInfo f = (FieldInfo)m;

                Control value = (Control)f.GetValue(_instance);
                if (value?.Tag == null)
                    continue;
                CreateWrapper(f, value, parentCallback);
            }
        }

        private void CreateWrapper(FieldInfo f, Control value, IPreferencesPageCallback parentCallback)
        {
            if (f.FieldType == typeof(TextBox))
            {
                _controlWrappers.Add(new TextBoxWrapper((TextBox)value, (string)value.Tag, parentCallback));
            }
            else if (f.FieldType == typeof(TrackBar))
            {
                _controlWrappers.Add(new TrackBarWrapper((TrackBar)value, (string)value.Tag, parentCallback));
            }
            else if (f.FieldType == typeof(CheckBox))
            {
                _controlWrappers.Add(new CheckBoxWrapper((CheckBox)value, (string)value.Tag, parentCallback));
            }
            else if (f.FieldType == typeof(Label))
            {
                _controlWrappers.Add(new LabelWrapper((Label)value, (string)value.Tag, parentCallback));
            }
            else if (f.FieldType == typeof(NumericUpDown))
            {
                _controlWrappers.Add(new NumericUpDownWrapper((NumericUpDown)value, (string)value.Tag, parentCallback));
            }
        }

        private bool ControlsMemberFilter(MemberInfo m, object filterCriteria)
        {
            return typeof(Control).IsAssignableFrom(((FieldInfo)m).FieldType);
        }
    }
}