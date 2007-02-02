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

// what is this good for ?
// - possibility to mantain variable amount of configuration data (as opposed to foobar's fixed configuration architecture)
// - easy binding to the configuration panel
// - maybe multiple config sources (for skin, for extension, ...)


namespace fooTitle.Config {
    #region Exceptions
    class UnsupportedTypeException : ApplicationException {
        protected Type type;

        public override string Message {
            get {
                return String.Format("This type is not supported for reading : {0}", type.ToString());
            }
        }

        public UnsupportedTypeException(Type _t) {
            type = _t;
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
        protected string xmlData;
        protected fooManagedWrapper.CCfgString cfgEntry;
        protected XmlDocument xmlDocument;
        protected XmlElement configRoot;
        
        /// <summary>
        /// Creates an instance of XmlConfigStorage and prepares it for writing and reading
        /// </summary>
        public XmlConfigStorage() {
            cfgEntry = new fooManagedWrapper.CCfgString(new Guid(457, 784, 488, 36, 48, 79, 54, 12, 12, 1, 1), "<config/>");
            xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(cfgEntry.GetVal());
            configRoot = (XmlElement)xmlDocument.GetElementsByTagName("config")[0];
        }

        #region IConfig Members
        public void WriteVal(string name, object value) {
            XmlNode el = findElementById(name);
            if (el == null) {
                el = xmlDocument.CreateElement("entry");
                configRoot.AppendChild(el);
            }
            ((XmlElement)el).SetAttribute("id", name);
            el.InnerText = value.ToString();
        }

        public object ReadVal<T>(string name) {
            XmlNode el = findElementById(name);
            if (el == null)
                return null;

            return stringToType<T>(el.InnerText);

        }

        public void Save() {
            StringBuilder stringBuilder = new StringBuilder();
            XmlWriter writer = XmlTextWriter.Create(stringBuilder);
            xmlDocument.Save(writer);
            cfgEntry.SetVal(stringBuilder.ToString());
        }
        #endregion

        public override string ToString() {
            StringBuilder stringBuilder = new StringBuilder();
            XmlWriter writer = XmlTextWriter.Create(stringBuilder);
            xmlDocument.Save(writer);
            return stringBuilder.ToString();
        }

        protected object stringToType<T>(string str) {
            if (typeof(T) == typeof(int)) {
                return Int32.Parse(str);
            } else if (typeof(T) == typeof(string)) {
                return str;
            } else if (typeof(T) == typeof(float)) {
                return float.Parse(str);
            } else
                throw new UnsupportedTypeException(typeof(T));
        }

        protected XmlNode findElementById(string id) {
            foreach (XmlNode n in configRoot.GetElementsByTagName("entry")) {
                if (fooTitle.Extending.Element.GetAttributeValue(n, "id", "") == id) {
                    return n;
                }
            }

            return null;
        }

    }
    #endregion
}
