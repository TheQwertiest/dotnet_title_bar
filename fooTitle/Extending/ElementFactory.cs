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
using System.Reflection;
using System.Collections;
using System.Xml;

namespace fooTitle.Extending {

    #region Exceptions
    public class MultipleElementTypesException : ApplicationException {
        protected Type onElement;

        public override String Message {
            get {
                return String.Format("Can't have more than one ElementTypeAttribute on single class : {0} ", onElement.ToString());
            }
        }

        public MultipleElementTypesException(Type _onElement) {
            onElement = _onElement;
        }
    }

    public class DuplicateElementTypeException : ApplicationException {
        protected String elementType;
        protected Type elementClass;
        protected Assembly assembly;

        public override String Message {
            get {
                return String.Format("Duplicate element type {0} found in assembly {1} on class {2}.",
                        elementType.ToString(), assembly.ToString(), elementClass.ToString());
            }
        }

        public DuplicateElementTypeException(String _elementType, Type _elementClass, Assembly _assembly) {
            elementClass = _elementClass;
            elementType = _elementType;
            assembly = _assembly;
        }
    }

    public class NoSuitableConstructorException : ApplicationException {
        protected String elementType;
        protected Type elementClass;
        protected String auxMsg;

        public override String Message {
            get {
                return String.Format("No suitable constructor for element type {0} class {1} found {2}.",
                        elementType, elementClass.ToString(), auxMsg);
            }
        }

        public NoSuitableConstructorException(String _elementType, Type _elementClass) {
            elementType = _elementType;
            elementClass = _elementClass;
        }

        public NoSuitableConstructorException(String _elementType, Type _elementClass, String _auxMsg) {
            elementType = _elementType;
            elementClass = _elementClass;
            auxMsg = _auxMsg;
        }

        public NoSuitableConstructorException(String _elementType, Type _elementClass, String _auxMsg, Exception _inner)
            : base("", _inner) {
            elementType = _elementType;
            elementClass = _elementClass;
            auxMsg = _auxMsg;
        }
    }

    public class ElementTypeNotFoundException : ApplicationException {
        protected String elementType;
        protected String auxMsg;

        public override String Message {
            get {
                return String.Format("Element of type {0} not found {1}.",
                        elementType, auxMsg);
            }
        }

        public ElementTypeNotFoundException(String _elementType) {
            elementType = _elementType;
        }

        public ElementTypeNotFoundException(String _elementType, String _auxMsg) {
            elementType = _elementType;
            auxMsg = _auxMsg;
        }

        public ElementTypeNotFoundException(String _elementType, String _auxMsg, Exception _inner)
            : base("", _inner) {
            elementType = _elementType;
            auxMsg = _auxMsg;
        }
    }

    #endregion

    [AttributeUsage(AttributeTargets.Class)]
    public class ElementTypeAttribute : Attribute {
        public String Type;

        public ElementTypeAttribute(String type) {
            Type = type;
        }
    }


    public class Element {
        public static XmlNode GetFirstChildByName(XmlNode where, string name) {
            foreach (XmlNode i in where)
                if (i.Name == name)
                    return i;
            throw new System.Xml.XmlException(String.Format("Node {0} not found under {1}", name, where.Name));
        }

        public static string GetNodeValue(XmlNode a) {
            return a.InnerText.Trim(new char[] { ' ', '\n', '\r', '\t' });
        }

        public static string GetAttributeValue(XmlNode where, string name, string def) {
            if (where.Attributes.GetNamedItem(name) != null)
                return where.Attributes.GetNamedItem(name).Value;

            return def;
        }

        /// <summary>
        /// If the attribute contains an expression, it is evaluated and the result is returned. 
        /// If the attribute is a number, the number is returned
        /// </summary>
        /// <param name="where">the node</param>
        /// <param name="name">name of the attribute</param>
        /// <param name="def">if the attribute doesn't exist, def is read</param>
        /// <returns>Evaluated expression or number (depending on what's in the attribute)</returns>
        public static float GetNumberFromAttribute(XmlNode where, string name, string def) {
            string val = GetAttributeValue(where, name, def);
            try {
                if (val.IndexOfAny(new char[] { '%', '$' }) != -1) {
                    // a formatting expression
                    return float.Parse(Main.PlayControl.FormatTitle(Main.PlayControl.GetNowPlaying(), val));
                } else {
                    // just a plain number
                    return float.Parse(val);
                }
            } catch (Exception e) {
                fooManagedWrapper.CConsole.Warning(e.ToString());
                return float.Parse(def);
            }
        }

        /// <summary>
        /// Reads an expression from XML node attribute. If the node contains no expression, returns null
        /// (to indicate that it's not an expression and no evaluation should be done)
        /// </summary>
        /// <param name="where">The node to extract the attribute from</param>
        /// <param name="name">The name of the attribute</param>
        /// <param name="def">The default value if the attribute is not present</param>
        /// <returns>the expression if it's present or null if there's just a string or a number</returns>
        public static string GetExpressionFromAttribute(XmlNode where, string name, string def) {
            string val = GetAttributeValue(where, name, def);
            if ((val.IndexOfAny(new char[] { '%', '$' })) != -1) {
                return val;
            } else {
                return null;
            }
        }

        /// <summary>
        /// Calculates the expression using the currently playing song. If the expression is null, returns the default valued
        /// </summary>
        /// <param name="expr">the expression to evaluate</param>
        /// <param name="def">the default value which is evaluated if expr is null</param>
        /// <returns>An integer result</returns>
        public static int GetValueFromExpression(string expr, int def) {
            if (expr == null)
                return def;
            try {
                return int.Parse(Main.PlayControl.FormatTitle(Main.PlayControl.GetNowPlaying(), expr));
            } catch (Exception) {
                return def;
            }
        }
    }

    /// Keeps track of elements available in this and extension assemblies
    /// to be inherited and specialised
    public abstract class ElementFactory {

        /// represents a single entry in the dictionary of elements
        protected struct ElementEntry {
            public String TypeName;
            public Type Class;

            public ElementEntry(String _typeName, Type _class) {
                TypeName = _typeName;
                Class = _class;
            }
        }

        protected ArrayList elementsDictionary;
        /// the type of element to search for - set in inheriting classes
        protected Type elementType;
        /// the type of attribute to search for - set in inheriting classes
        protected Type elementTypeAttributeType;

        public ElementFactory() {
            elementsDictionary = new ArrayList();
        }

        /// finds all elements in assembly and adds them to list of elements which can be created later
        public void SearchAssembly(Assembly assembly) {
            Type[] types = assembly.GetTypes();

            foreach (Type t in types) {
                if (elementType.IsAssignableFrom(t)) {
                    Object[] attrs = t.GetCustomAttributes(elementTypeAttributeType, false);
                    if (attrs.Length == 1) {
                        String elementTypeName = ((ElementTypeAttribute)attrs[0]).Type;
                        addElementEntry(new ElementEntry(elementTypeName, t), assembly);
                    } else if (attrs.Length > 1) {
                        throw new MultipleElementTypesException(t);
                    }
                }
            }
        }

        protected bool elementTypeExists(String type) {
            foreach (Object o in elementsDictionary) {
                if (((ElementEntry)o).TypeName == type)
                    return true;
            }
            return false;
        }

        /// assembly parameter is required in order to be able to report errors
        protected void addElementEntry(ElementEntry e, Assembly assembly) {
            // check for presence first
            if (elementTypeExists(e.TypeName)) {
                throw new DuplicateElementTypeException(e.TypeName, e.Class, assembly);
            } else {
                elementsDictionary.Add(e);
            }
        }

        /// creates an element specified by it's type
        protected Element CreateElement(String type, object[] parameters) {
            foreach (Object o in elementsDictionary) {
                if (((ElementEntry)o).TypeName == type) {
                    // an element found
                    Type elementClass = ((ElementEntry)o).Class;
                    return createElement(elementClass, type, parameters);
                }
            }

            throw new ElementTypeNotFoundException(type);
        }

        private Element createElement(Type elementClass, String type, object[] parameters) {
            // construct list of types of parameters going to the constructor
            Type[] paramTypes = new Type[parameters.Length];
            int i = 0;

            foreach (Object par in parameters) {
                paramTypes[i] = par.GetType();
                i++;

            }

            // find the appropriate constructor
            ConstructorInfo cons = elementClass.GetConstructor(paramTypes);
            if (cons != null) {
                return (Element)cons.Invoke(parameters);
            }

            throw new NoSuitableConstructorException(type, elementClass);
        }
    }

}
