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

namespace fooTitle.Config {
    #region Exceptions
    /// <summary>
    /// Thrown when user attempts to create multiple values of the same name.
    /// </summary>
    public class ValueAlreadyExistsException : Exception {
        private readonly string _name;

        public override string Message => $"Value {_name} is already registered with the ConfValuesManager.";

        public ValueAlreadyExistsException(string name) {
            _name = name;
        }
    }

    #endregion

    #region ConfValuesManager
    /// <summary>
    /// This class stores the available configuration values, handles loading and saving and notifying 
    /// other classes when a value changes.
    /// </summary>
    public class ConfValuesManager {
        private readonly List<ConfValue> _values = new List<ConfValue>();
        private static ConfValuesManager _instance;
        private IConfigStorage _savedStorage;

        /// <summary>
        /// This method implements the singleton pattern.
        /// </summary>
        /// <returns>An instance of ConfValuesManager. Always returns the same instance.</returns>
        public static ConfValuesManager GetInstance() {
            if (_instance == null)
                _instance = new ConfValuesManager();
            
            return _instance;
        }

        /// <summary>
        /// Adds a value to this manager to handle.
        /// </summary>
        public void AddVal(ConfValue v) {
            if (GetValueByName(v.Name) != null)
                throw new ValueAlreadyExistsException(v.Name);

            _values.Add(v);

            OnValueCreated?.Invoke(v.Name);

            // register this as the receiver for v's OnChanged
            v.OnChanged += FireValueChanged;

            // if there is a storage set, load the value from it
            if (_savedStorage != null)
                v.LoadFrom(_savedStorage);
        }

        /// <summary>
        /// Removes a value from this manager's care.
        /// </summary>
        public void RemoveVal(ConfValue v) {
            _values.Remove(v);
        }

        /// <summary>
        /// Notifies registered classes that the value by the name has been changed.
        /// </summary>
        public void FireValueChanged(string name) {
            if (GetValueByName(name) == null)
                throw new InvalidOperationException(
                    $"Cannot fire the OnValueChanged event for value named {name}. There is no such variable registered.");

            // check if anyone's listening
            OnValueChanged?.Invoke(name);
        }

        public delegate void ValueChangedDelegate(string name);
        public event ValueChangedDelegate OnValueChanged;
        public event ValueChangedDelegate OnValueCreated;

        /// <summary>
        /// Sets a IConfigStorage instance to use to load values of newly created variables from.
        /// </summary>
        public void SetStorage(IConfigStorage a) {
            _savedStorage = a;
        }

        /// <summary>
        /// Saves all handled values to a config storage
        /// </summary>
        public void SaveTo(IConfigStorage to) {
            foreach (ConfValue v in _values) {
                v.SaveTo(to);
            }

            to.Save();
        }


        /// <summary>
        /// Loads all currently registered values from a config storage.
        /// </summary>
        public void LoadFrom(IConfigStorage from) {
            foreach (ConfValue v in _values) {
                v.LoadFrom(from);
            }
        }

        /// <summary>
        /// Finds a value by its name.
        /// </summary>
        /// <returns>The ConfValue if found, otherwise null.</returns>
        public ConfValue GetValueByName(string name) {
            foreach (ConfValue v in _values) {
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
            foreach (ConfValue v in _values) {
                v.Reset();
            }
        }

        public bool HasChanged() {
            foreach (ConfValue v in _values) {
                if (v.HasChanged()) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// If a value with the given name exists, returns a reference to it.
        /// Otherwise creates a new one. Should be used when it is expected
        /// that it will be called multiple times (one value can be created
        /// only once).
        /// </summary>
        public static ConfInt CreateIntValue(string name, int _def, int _min, int _max) {
            ConfInt existing = (ConfInt)GetInstance().GetValueByName(name);
            if (existing != null)
                return existing;

            return new ConfInt(name, _def, _min, _max);
        }

        /// <summary>
        /// <see cref="CreateIntValue"/>
        /// </summary>
        public static ConfEnum<T> CreateEnumValue<T>(string name, T _def) where T : IConvertible {
            ConfEnum<T> existing = (ConfEnum<T>)GetInstance().GetValueByName(name);
            if (existing != null)
                return existing;

            return new ConfEnum<T>(name, _def);
        }

        /// <summary>
        /// <see cref="CreateIntValue"/>
        /// </summary>
        public static ConfBool CreateBoolValue(string name, bool _def)
        {
            ConfBool existing = (ConfBool)GetInstance().GetValueByName(name);
            if (existing != null)
                return existing;

            return new ConfBool(name, _def);
        }
    }

    #endregion
}
