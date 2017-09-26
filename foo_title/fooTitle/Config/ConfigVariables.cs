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

namespace fooTitle.Config {

    #region IConfigValueVisitor
    /// <summary>
    /// By implementing this interface and calling the ReadVisit and WriteVisit methods
    /// on the ConfValue class users can distinguish between different types even when they
    /// only have a base ConfValue reference.
    /// </summary>
    public interface IConfigValueVisitor {
        /// <summary>
        /// Used to read a value from the ConfInt instance into the visitor
        /// </summary>
        void ReadInt(ConfInt val);
        /// <summary>
        /// Used to read a value from the ConfString instance
        /// </summary>
        void ReadString(ConfString val);


        /// <summary>
        /// Used to store an int to an instance of ConfInt
        /// </summary>
        void WriteInt(ConfInt val);

        /// <summary>
        /// Used to store a string to an instance of ConfString
        /// </summary>
        void WriteString(ConfString val);
    }
    #endregion

    #region ConfValue
    /// <summary>
    /// An abstract base class for all configuration values
    /// </summary>
    public abstract class ConfValue {
        /// <summary>
        /// Gets the name of this variable. The name is a unique string identifier.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Registers this value with the ConfValuesManager
        /// </summary>
        protected ConfValue(string name) {
            this.Name = name;
        }


        /// <summary>
        /// Initializes the value - registers with the manager which sends events about updating. This should be
        /// called when the value is done setting up and is fully usable.
        /// </summary>
        protected void Init() {
            ConfValuesManager.GetInstance().AddVal(this);
            ConfValuesManager.GetInstance().FireValueChanged(Name);
        }

        /// <summary>
        /// Unregisters this instance from the ConfValuesManager, so that the instance may be freed when no longer used
        /// </summary>
        public virtual void Unregister() {
            ConfValuesManager.GetInstance().RemoveVal(this);
            OnChanged = null;
        }

        /// <summary>
        /// Invoked when the value changes
        /// </summary>
        public event ConfValuesManager.ValueChangedDelegate OnChanged;

        /// <summary>
        /// Helper function to fire the OnChanged event. Should be preffered over invoking the event directly
        /// </summary>
        protected void FireOnChanged() {
            OnChanged?.Invoke(Name);
        }

        /// <summary>
        /// Implement this method to provide a way to save the value to a IConfigStorage
        /// </summary>
        /// <param name="to">The storage to write this value to </param>
        public abstract void SaveTo(IConfigStorage to);

        /// <summary>
        /// Implement this method to provide a way to load the value of this class from a storage
        /// </summary>
        /// <param name="from">The storage to read this value from</param>
        public abstract void LoadFrom(IConfigStorage from);

        /// <summary>
        /// This method should call an appropriate method of the IConfigValueVisitor according to the type of the implementing class
        /// </summary>
        public abstract void ReadVisit(IConfigValueVisitor visitor);

        /// <summary>
        /// This method should call an appropriate method of the IConfigValueVisitor according to the type of the implementing class
        /// </summary>
        /// <param name="visitor"></param>
        public abstract void WriteVisit(IConfigValueVisitor visitor);

        /// <summary>
        /// Resets the value of this variable to the default
        /// </summary>
        public abstract void Reset();

        public abstract bool HasChanged();
    }
    #endregion


    /// <summary>
    /// Stores an integer (Int32). Supports a minimum and a maximum value to be specified. Setting the value out of the
    /// bounds will not generate an exception, but the value will stay on the maximum/minimum.
    /// </summary>
    public class ConfInt : ConfValue {
        protected int val;
        protected int min, max;
        private int _saved;
        private readonly int _def;

        public virtual int Value {
            set {
                if (SetValue(value))
                    FireOnChanged();
            }
            get {
                return val;
            }
        }

        /// <summary>
        /// Sets the new value and returns true if update event should be raised.
        /// </summary>
        private bool SetValue(int value) {
            if (((value > max) && (val == max)) || ((value < min) && (val == min)) || (value == val))
                return false; // nothing new

            if (value > max)
                val = max;
            else if (value < min)
                val = min;
            else
                val = value;

            return true;
        }

        public void ForceUpdate(int newVal) {
            SetValue(newVal);
            FireOnChanged();
        }

        /// <summary>
        /// Gets the maximum possible value.
        /// </summary>
        public int Max => max;

        /// <summary>
        /// Gets the minimum possible value.
        /// </summary>
        public int Min => min;

        public ConfInt(string name, int def) : base(name) {
            _def = def;
            val = _def;

            min = int.MinValue;
            max = int.MaxValue;

            Init();
        }

        public ConfInt(string name, int def, int _min, int _max) : base(name) {
            _def = def;
            val = _def;

            min = _min;
            max = _max;

            Init();
        }

        public override void SaveTo(IConfigStorage to) {
            to.WriteVal(Name, val);
            _saved = val;
        }

        public override void LoadFrom(IConfigStorage from) {
            object res = from.ReadVal<int>(Name);
            if (res != null) {
                Value = (int)res;
                _saved = Value;
            }
        }

        public override void ReadVisit(IConfigValueVisitor visitor) {
            visitor.ReadInt(this);
        }

        public override void WriteVisit(IConfigValueVisitor visitor) {
            visitor.WriteInt(this);
        }

        public override void Reset() {
            Value = _def;
        }

        public override bool HasChanged() {
            return Value != _saved;
        }
    }

    public class ConfString : ConfValue {
        private readonly string _def;
        private string _val;
        private string _saved;

        public ConfString(string name, string def) : base(name) {
            _def = def;
            _val = def;

            Init();
        }

        public string Value {
            get {
                return _val;
            }
            set {
                if (value != _val) {
                    _val = value;

                    FireOnChanged();
                }
            }
        }

        public void ForceUpdate(string newVal) {
            _val = newVal;
            FireOnChanged();
        }

        public override void SaveTo(IConfigStorage to) {
            to.WriteVal(Name, Value);
            _saved = Value;
        }

        public override void LoadFrom(IConfigStorage from) {
            object res = from.ReadVal<string>(Name);
            if (res != null) {
                Value = (string)res;
                _saved = Value;
            }
        }

        public override void ReadVisit(IConfigValueVisitor visitor) {
            visitor.ReadString(this);
        }

        public override void WriteVisit(IConfigValueVisitor visitor) {
            visitor.WriteString(this);
        }

        public override void Reset() {
            Value = _def;
        }

        public override bool HasChanged() {
            return Value != _saved;
        }
    }

    public class ConfEnum<T> : ConfInt where T : IConvertible {
        public ConfEnum(string name, T def) : base(name, Convert.ToInt32(def)) {
            min = 0;
        }

        public new T Value {
            set => base.Value = Convert.ToInt32(value);
            get => (T)(object)val;
        }
    }

    public class ConfBool : ConfInt {
        public ConfBool(string name, bool def) : base(name, def ? 1 : 0) {
            min = 0;
            max = 1;
        }

        public new bool Value {
            set => base.Value = value ? 1 : 0;
            get => base.Value != 0;
        }
    }
}
