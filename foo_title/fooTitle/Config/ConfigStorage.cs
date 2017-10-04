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
using System.Linq;
using System.Xml.Linq;

// what is this good for ?
// - possibility to mantain variable amount of configuration data (as opposed to foobar's fixed configuration architecture)
// - easy binding to the configuration panel
// - maybe multiple config sources (for skin, for extension, ...)


namespace fooTitle.Config {
    #region Exceptions

    internal class UnsupportedTypeException : ApplicationException {
        private readonly Type _type;

        public override string Message => $"This type is not supported for reading : {_type.ToString()}";

        public UnsupportedTypeException(Type t) {
            _type = t;
        }
    }

    #endregion

    #region IConfigStorage
    /// <summary>
    /// This is the interface for configuration database storage implementors
    /// </summary>
    public interface IConfigStorage {
        /// <summary>
        /// Writes a value under a name. The value can be of any type as it's ToString() method is used to serialize.
        /// However, only the basic types are supported for reading.
        /// </summary>
        /// <param name="name">The name under which the value should be stored. '/' characters are used for 
        /// building a tree structure.</param>
        /// <param name="value">The boxed value to store.</param>
        void WriteVal(string name, object value);

        /// <summary>
        /// Reads a value by it's name 
        /// </summary>
        /// <typeparam name="T">The type that the stored value should be converted to.</typeparam>
        /// <param name="name">The name of the value under which it is stored.</param>
        /// <returns>The boxed value if present in the storage or null if it can't be found.</returns>
        /// <exception cref="UnsupportedTypeException">Throws if an unsupported type is required in T.</exception>
        object ReadVal<T>(string name);


        /// <summary>
        /// Invoke explicit save. Implementors of this interface may save the contents implicitly, or only when this method is invoked.
        /// </summary>
        void Save();

        /// <summary>
        /// Invokes loading from the storage.
        /// </summary>
        void Load();

        /// <summary>
        /// Used mostly for debugging purposes.
        /// </summary>
        /// <returns>Returns a string representation of the stored data.</returns>
        string ToString();
    }
    #endregion

    #region XmlConfigStorage
    /// <summary>
    /// This is the XML in foobar's configuration variables implementation of IConfigStorage.
    /// </summary>
    public class XmlConfigStorage : IConfigStorage {
        private readonly fooManagedWrapper.CNotifyingCfgString _cfgEntry;
        private XDocument _xmlDocument;
        private XElement _configRoot;
        
        /// <summary>
        /// Creates an instance of XmlConfigStorage and prepares it for writing and reading
        /// </summary>
        public XmlConfigStorage(fooManagedWrapper.CNotifyingCfgString cfgEntry) {
             _cfgEntry = cfgEntry;
        }

        #region IConfig Members
        public void Load() {
            _xmlDocument = XDocument.Parse(_cfgEntry.GetVal());
            _configRoot = _xmlDocument.Elements("config").First();
        }

        public void WriteVal(string name, object value) {
            XElement el = FindElementById(name);
            if (el == null) {
                el = new XElement("entry");
                _configRoot.Add(el);
            }
            el.SetAttributeValue("id", name);

            if (value != null) {
                if (value.GetType().IsSubclassOf(typeof(XNode)) ) {
                    el.Elements().Remove();

                    XElement importNode = new XElement((XElement)value);
                    el.Add(importNode);
                }
                else {
                    el.Value = value.ToString();
                }
            }
        }

        public object ReadVal<T>(string name)
        {
            XElement el = FindElementById(name);
            if (el == null)
                return null;

            if (el.Elements().Count() != 0)
                return el.FirstNode;

            return el.Value == "" ? null : StringToType<T>(el.Value);
        }

        public void Save()
        {
            _cfgEntry.SetVal(_xmlDocument.ToString());
        }
        #endregion

        public override string ToString() {
            return _xmlDocument.ToString();
        }

        protected object StringToType<T>(string str) {
            if (typeof(T) == typeof(int))
                return int.Parse(str);
            if (typeof(T) == typeof(string)) 
                return str;
            if (typeof(T) == typeof(float)) 
                return float.Parse(str);
            throw new UnsupportedTypeException(typeof(T));
        }

        protected XElement FindElementById(string id) {
            foreach (XElement n in _configRoot.Elements("entry")) {
                if (fooTitle.Extending.Element.GetAttributeValue(n, "id", "") == id) {
                    return n;
                }
            }

            return null;
        }

    }
    #endregion
}
