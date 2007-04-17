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

    #region ConfValuesManager
    /// <summary>
    /// This class stores the available configuration values, handles loading and saving and notifying 
    /// other classes when a value changes.
    /// </summary>
    public class ConfValuesManager {
        protected List<ConfValue> values = new List<ConfValue>();
        protected static ConfValuesManager instance;
        protected IConfigStorage savedStorage;

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

            if (OnValueCreated != null)
                OnValueCreated(v.Name);

            // register this as the receiver for v's OnChanged
            v.OnChanged += new ValueChangedDelegate(FireValueChanged);

            // if there is a storage set, load the value from it
            if (savedStorage != null)
                v.LoadFrom(savedStorage);
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
        public event ValueChangedDelegate OnValueCreated;

        /// <summary>
        /// Sets a IConfigStorage instance to use to load values of newly created variables from.
        /// </summary>
        public void SetStorage(IConfigStorage a) {
            savedStorage = a;
        }

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


        /// <summary>
        /// Resets all values to default
        /// </summary>
        public void Reset() {
            foreach (ConfValue v in values) {
                v.Reset();
            }
        }
    }

    #endregion
}