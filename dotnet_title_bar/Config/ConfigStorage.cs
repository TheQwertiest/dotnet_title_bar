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
using System.Linq;
using System.Xml.Linq;

// what is this good for ?
// - possibility to mantain variable amount of configuration data (as opposed to foobar's fixed configuration architecture)
// - easy binding to the configuration panel
// - maybe multiple config sources (for skin, for extension, ...)


namespace fooTitle.Config
{
    internal class UnsupportedTypeException : ApplicationException
    {
        private readonly Type _type;

        public override string Message => $"This type is not supported for reading : {_type}";

        public UnsupportedTypeException(Type t)
        {
            _type = t;
        }
    }

    /// <summary>
    /// This is the XML in foobar's configuration variables implementation of IConfigStorage.
    /// </summary>
    public class XmlConfigStorage : IConfigStorage
    {
        private readonly IConfigVar<string> _cfgEntry;
        private XDocument _xmlDocument;
        private XElement _configRoot;

        /// <summary>
        /// Creates an instance of XmlConfigStorage and prepares it for writing and reading
        /// </summary>
        public XmlConfigStorage(IConfigVar<string> cfgEntry)
        {
            _cfgEntry = cfgEntry;
        }

        public void Load()
        {
            _xmlDocument = XDocument.Parse(_cfgEntry.Get());
            _configRoot = _xmlDocument.Elements("config").First();
        }

        public void WriteVal(string name, object value)
        {
            XElement el = FindElementById(name);
            if (el == null)
            {
                el = new XElement("entry");
                _configRoot.Add(el);
            }
            el.SetAttributeValue("id", name);

            if (value != null)
            {
                if (value.GetType().IsSubclassOf(typeof(XNode)))
                {
                    el.Elements().Remove();

                    XElement importNode = new XElement((XElement)value);
                    el.Add(importNode);
                }
                else
                {
                    el.Value = value.ToString();
                }
            }
        }

        public object ReadVal<T>(string name)
        {
            XElement el = FindElementById(name);
            if (el == null)
            {
                return null;
            }

            if (el.Elements().Any())
            {
                return el.FirstNode;
            }

            return el.Value == "" ? null : StringToType<T>(el.Value);
        }

        public void Save()
        {
            _cfgEntry.Set(_xmlDocument.ToString());
        }

        public override string ToString()
        {
            return _xmlDocument.ToString();
        }

        protected static object StringToType<T>(string str)
        {
            var type = typeof(T);
            if (type == typeof(int))
            {
                return int.Parse(str);
            }

            if (type == typeof(string))
            {
                return str;
            }

            if (type == typeof(float))
            {
                return float.Parse(str);
            }

            throw new UnsupportedTypeException(type);
        }

        protected XElement FindElementById(string id)
        {
            foreach (XElement n in _configRoot.Elements("entry"))
            {
                if (fooTitle.Extending.Element.GetAttributeValue(n, "id", "") == id)
                {
                    return n;
                }
            }

            return null;
        }

    }
}