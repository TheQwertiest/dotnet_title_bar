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
using System.Xml;

using fooTitle.Tests;


namespace fooTitle.Config {
    #region Exceptions
    /// <summary>
    /// Thrown when user attempts to create multiple values of the same name.
    /// </summary>
    public class ValueAlreadyExistsException : Exception {
        protected string name;

        public override string Message {
            get {
                return String.Format("Value {0} is already registered with the ConfValuesManager.");
            }
        }

        public ValueAlreadyExistsException(string _name) {
            name = _name;
        }
    }

    #endregion

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

    #region ConfValuesManager
    /// <summary>
    /// This class stores the available configuration values, handles loading and saving and notifying 
    /// other classes when a value changes.
    /// </summary>
    class ConfValuesManager {
        protected List<ConfValue> values = new List<ConfValue>();
        protected static ConfValuesManager instance;


        /// <summary>
        /// This method implements the singleton pattern.
        /// </summary>
        /// <returns>An instance of ConfValuesManager. Always returns the same instance.</returns>
        public static ConfValuesManager GetInstance() {
            if (instance != null) {
                return instance;
            } else {
                instance = new ConfValuesManager();
                return instance;
            }
        }

        /// <summary>
        /// Adds a value to this manager to handle.
        /// </summary>
        public void AddVal(ConfValue v) {
            if (GetValueByName(v.Name) != null)
                throw new ValueAlreadyExistsException(v.Name);

            values.Add(v);
        }

        /// <summary>
        /// Removes a value from this manager's care.
        /// </summary>
        public void RemoveVal(ConfValue v) {
            values.Remove(v);
        }

        /// <summary>
        /// Notifies registered classes that the value by the name has been changed.
        /// </summary>
        public void FireValueChanged(string name) {
            if (GetValueByName(name) == null) 
                throw new InvalidOperationException(String.Format("Cannot fire the OnValueChanged event for value named {0}. There is no such variable registered.", name));

            // check if anyone's listening
            if (OnValueChanged != null)
                OnValueChanged(name);
        }

        public delegate void ValueChangedDelegate(string name);
        public event ValueChangedDelegate OnValueChanged;

        /// <summary>
        /// Saves all handled values to a config storage
        /// </summary>
        public void SaveTo(IConfigStorage to) {
            foreach (ConfValue v in values) {
                v.SaveTo(to);
            }

            to.Save();
        }


        /// <summary>
        /// Loads all currently registered values from a config storage.
        /// </summary>
        public void LoadFrom(IConfigStorage from) {
            foreach (ConfValue v in values) {

                v.LoadFrom(from);
            }
        }

        /// <summary>
        /// Finds a value by its name.
        /// </summary>
        /// <returns>The ConfValue if found, otherwise null.</returns>
        public ConfValue GetValueByName(string name) {
            foreach (ConfValue v in values) {
                if (v.Name == name) {
                    return v;
                }
            }

            return null;
        }

    }

    #endregion

    #region ConfValue
    /// <summary>
    /// An abstract base class for all configuration values
    /// </summary>
    public abstract class ConfValue {
        protected string name;

        /// <summary>
        /// Gets the name of this variable. The name is a unique string identifier.
        /// </summary>
        public string Name {
            get {
                return name;
            }
        }

        /// <summary>
        /// Registers this value with the ConfValuesManager
        /// </summary>
        public ConfValue(string _name) {
            name = _name;
        }


        /// <summary>
        /// Initializes the value - registers with the manager which sends events about updating. This should be
        /// called when the value is done setting up and can is fully usable.
        /// </summary>
        protected void init() {
            ConfValuesManager.GetInstance().AddVal(this);
            ConfValuesManager.GetInstance().FireValueChanged(Name);
        }

        /// <summary>
        /// Unregisters this instance from the ConfValuesManager, so that the instance may be freed when no longer used
        /// </summary>
        public virtual void Unregister() {
            ConfValuesManager.GetInstance().RemoveVal(this);
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
    }
    #endregion



    /// <summary>
    /// Stores an integer (Int32). Supports a minimum and a maximum value to be specified. Setting the value out of the
    /// bounds will not generate an exception, but the value will stay on the maximum/minimum.
    /// </summary>
    public class ConfInt : ConfValue {
        protected int val;
        protected int def;
        protected int min, max;

        public virtual int Value {
            set {
                if (((value > max) && (val == max)) || ((value < min) && (val == min)) ||
                (value == val))
                    return; // nothing new

                if (value > max)
                    val = max;
                else if (value < min)
                    val = min;
                else
                    val = value;

                ConfValuesManager.GetInstance().FireValueChanged(Name);
            }
            get {
                return val;
            }
        }

        /// <summary>
        /// Gets the maximum possible value.
        /// </summary>
        public int Max {
            get {
                return max;
            }
        }

        /// <summary>
        /// Gets the minimum possible value.
        /// </summary>
        public int Min {
            get {
                return min;
            }
        }

        public ConfInt(string _name, int _def)
            : base(_name) {
            def = _def;
            val = def;

            min = Int32.MinValue;
            max = Int32.MaxValue;

            init();
        }

        public ConfInt(string _name, int _def, int _min, int _max)
            : base(_name) {
            def = _def;
            val = def;

            min = _min;
            max = _max;

            init();
        }

        public override void SaveTo(IConfigStorage to) {
            to.WriteVal(name, val);
        }

        public override void LoadFrom(IConfigStorage from) {
            Value = (int)from.ReadVal<int>(name);
        }

        public override void ReadVisit(IConfigValueVisitor visitor) {
            visitor.ReadInt(this);
        }

        public override void WriteVisit(IConfigValueVisitor visitor) {
            visitor.WriteInt(this);
        }
    }

    public class ConfString : ConfValue {
        protected string def;
        protected string val;

        public ConfString(string _name, string _def)
            : base(_name) {
            def = _def;
            val = _def;

            init();
        }

        public string Value {
            get {
                return val;
            }
            set {
                if (value != val) {
                    val = value;

                    ConfValuesManager.GetInstance().FireValueChanged(Name);
                }
            }
        }

        public override void SaveTo(IConfigStorage to) {
            to.WriteVal(Name, Value);
        }

        public override void LoadFrom(IConfigStorage from) {
            Value = (string)from.ReadVal<string>(Name);
        }

        public override void ReadVisit(IConfigValueVisitor visitor) {
            visitor.ReadString(this);
        }

        public override void WriteVisit(IConfigValueVisitor visitor) {
            visitor.WriteString(this);
        }
    }

    public class ConfEnum<T> : ConfInt where T : IConvertible {
        public ConfEnum(string _name, T _def)
            : base(_name, System.Convert.ToInt32(_def)) {

        }

        public new T Value {
            set {
                base.Value = System.Convert.ToInt32(value);
            }
            get {
                return (T)(object)val;
            }
        }
    }
}
