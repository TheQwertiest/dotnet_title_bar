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
        private readonly Type _onElement;

        public override string Message =>
            $"Can't have more than one ElementTypeAttribute on single class : {_onElement.ToString()} ";

        public MultipleElementTypesException(Type onElement) {
            _onElement = onElement;
        }
    }

    public class DuplicateElementTypeException : ApplicationException {
        private readonly string _elementType;
        private readonly Type _elementClass;
        private readonly Assembly _assembly;

        public override string Message =>
            $"Duplicate element type {_elementType.ToString()} found in assembly {_assembly.ToString()} on class {_elementClass.ToString()}.";

        public DuplicateElementTypeException(string elementType, Type elementClass, Assembly assembly) {
            _elementClass = elementClass;
            _elementType = elementType;
            _assembly = assembly;
        }
    }

    public class NoSuitableConstructorException : ApplicationException {
        private readonly string _elementType;
        private readonly Type _elementClass;
        private readonly string _auxMsg;

        public override string Message =>
            $"No suitable constructor for element type {_elementType} class {_elementClass.ToString()} found {_auxMsg}.";

        public NoSuitableConstructorException(string elementType, Type elementClass) {
            _elementType = elementType;
            _elementClass = elementClass;
        }

        public NoSuitableConstructorException(string elementType, Type elementClass, string auxMsg) {
            _elementType = elementType;
            _elementClass = elementClass;
            _auxMsg = auxMsg;
        }

        public NoSuitableConstructorException(string elementType, Type elementClass, string auxMsg, Exception inner)
            : base("", inner) {
            _elementType = elementType;
            _elementClass = elementClass;
            _auxMsg = auxMsg;
        }
    }

    public class ElementTypeNotFoundException : ApplicationException {
        private readonly string _elementType;
        private readonly string _auxMsg;

        public override string Message => $"Element of type {_elementType} not found {_auxMsg}.";

        public ElementTypeNotFoundException(string elementType) {
            _elementType = elementType;
        }

        public ElementTypeNotFoundException(string elementType, string auxMsg) {
            _elementType = elementType;
            _auxMsg = auxMsg;
        }

        public ElementTypeNotFoundException(string elementType, string auxMsg, Exception inner)
            : base("", inner) {
            _elementType = elementType;
            _auxMsg = auxMsg;
        }
    }

    #endregion

    [AttributeUsage(AttributeTargets.Class)]
    public class ElementTypeAttribute : Attribute {
        public string Type;

        public ElementTypeAttribute(string type) {
            Type = type;
        }
    }


    public class Element {
        public static XmlNode GetFirstChildByName(XmlNode where, string name) {
            foreach (XmlNode i in where)
                if (i.Name == name)
                    return i;
            throw new XmlException($"Node {name} not found under {@where.Name}");
        }

        public static XmlNode GetFirstChildByNameOrNull(XmlNode where, string name) {
            foreach (XmlNode i in where)
                if (i.Name == name)
                    return i;

            return null;
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
                if (IsExpression(val)) {
                    // a formatting expression
                    return float.Parse(Main.PlayControl.FormatTitle(Main.PlayControl.GetNowPlaying(), val));
                } else {
                    // just a plain number
                    return float.Parse(val, System.Globalization.NumberFormatInfo.InvariantInfo);
                }
            } catch (Exception e) {
                fooManagedWrapper.CConsole.Warning(e.ToString());
                fooManagedWrapper.CConsole.Warning(val);
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
            if (val == null || !IsExpression(val)) {
                return null;
            }

            return val;
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

        public static string GetStringFromExpression(string expr, string def) {
            if (expr == null)
                return def;

            if (!IsExpression(expr)) {
                return expr;
            }
            try {
                return Main.PlayControl.FormatTitle(Main.PlayControl.GetNowPlaying(), expr);
            } catch (Exception) {
                return def;
            }
        }

        public static bool IsExpression(string expr)
        {
            return (expr.IndexOfAny(new char[] {'%', '$'}) != -1);
        }
    }

    /// Keeps track of elements available in this and extension assemblies
    /// to be inherited and specialised
    public abstract class ElementFactory {

        /// represents a single entry in the dictionary of elements
        private struct ElementEntry {
            public readonly string TypeName;
            public readonly Type Class;

            public ElementEntry(string typeName, Type _class) {
                TypeName = typeName;
                Class = _class;
            }
        }

        private readonly ArrayList _elementsDictionary;
        /// the type of element to search for - set in inheriting classes
        protected Type elementType;
        /// the type of attribute to search for - set in inheriting classes
        protected Type elementTypeAttributeType;

        protected ElementFactory() {
            _elementsDictionary = new ArrayList();
        }

        /// finds all elements in assembly and adds them to list of elements which can be created later
        public void SearchAssembly(Assembly assembly) {
            Type[] types = assembly.GetTypes();

            foreach (Type t in types) {
                if (elementType.IsAssignableFrom(t)) {
                    Object[] attrs = t.GetCustomAttributes(elementTypeAttributeType, false);
                    if (attrs.Length == 1) {
                        string elementTypeName = ((ElementTypeAttribute)attrs[0]).Type;
                        AddElementEntry(new ElementEntry(elementTypeName, t), assembly);
                    } else if (attrs.Length > 1) {
                        throw new MultipleElementTypesException(t);
                    }
                }
            }
        }

        private bool ElementTypeExists(string type) {
            foreach (Object o in _elementsDictionary) {
                if (((ElementEntry)o).TypeName == type)
                    return true;
            }
            return false;
        }

        /// assembly parameter is required in order to be able to report errors
        private void AddElementEntry(ElementEntry e, Assembly assembly) {
            // check for presence first
            if (ElementTypeExists(e.TypeName)) {
                throw new DuplicateElementTypeException(e.TypeName, e.Class, assembly);
            }
            _elementsDictionary.Add(e);
        }

        /// creates an element specified by it's type
        protected Element CreateElement(string type, object[] parameters) {
            foreach (object o in _elementsDictionary) {
                if (((ElementEntry)o).TypeName == type) {
                    // an element found
                    Type elementClass = ((ElementEntry)o).Class;
                    return CreateElement(elementClass, type, parameters);
                }
            }

            throw new ElementTypeNotFoundException(type);
        }

        private Element CreateElement(Type elementClass, string type, object[] parameters) {
            // construct list of types of parameters going to the constructor
            Type[] paramTypes = new Type[parameters.Length];
            int i = 0;

            foreach (object par in parameters) {
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
