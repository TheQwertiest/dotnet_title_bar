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
using System.Collections;
using System.Reflection;

namespace fooTitle.Extending
{

    public class MultipleElementTypesException : ApplicationException
    {
        private readonly Type _onElement;

        public override string Message =>
            $"Can't have more than one ElementTypeAttribute on single class : {_onElement} ";

        public MultipleElementTypesException(Type onElement)
        {
            _onElement = onElement;
        }
    }

    public class DuplicateElementTypeException : ApplicationException
    {
        private readonly string _elementType;
        private readonly Type _elementClass;
        private readonly Assembly _assembly;

        public override string Message =>
            $"Duplicate element type {_elementType} found in assembly {_assembly} on class {_elementClass}.";

        public DuplicateElementTypeException(string elementType, Type elementClass, Assembly assembly)
        {
            _elementClass = elementClass;
            _elementType = elementType;
            _assembly = assembly;
        }
    }

    public class NoSuitableConstructorException : ApplicationException
    {
        private readonly string _elementType;
        private readonly Type _elementClass;
        private readonly string _auxMsg;

        public override string Message =>
            $"No suitable constructor for element type {_elementType} class {_elementClass} found {_auxMsg}.";

        public NoSuitableConstructorException(string elementType, Type elementClass)
        {
            _elementType = elementType;
            _elementClass = elementClass;
        }

        public NoSuitableConstructorException(string elementType, Type elementClass, string auxMsg)
        {
            _elementType = elementType;
            _elementClass = elementClass;
            _auxMsg = auxMsg;
        }

        public NoSuitableConstructorException(string elementType, Type elementClass, string auxMsg, Exception inner)
            : base("", inner)
        {
            _elementType = elementType;
            _elementClass = elementClass;
            _auxMsg = auxMsg;
        }
    }

    public class ElementTypeNotFoundException : ApplicationException
    {
        private readonly string _elementType;
        private readonly string _auxMsg;

        public override string Message => $"Element of type {_elementType} not found {_auxMsg}.";

        public ElementTypeNotFoundException(string elementType)
        {
            _elementType = elementType;
        }

        public ElementTypeNotFoundException(string elementType, string auxMsg)
        {
            _elementType = elementType;
            _auxMsg = auxMsg;
        }

        public ElementTypeNotFoundException(string elementType, string auxMsg, Exception inner)
            : base("", inner)
        {
            _elementType = elementType;
            _auxMsg = auxMsg;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ElementTypeAttribute : Attribute
    {
        public readonly string Type;

        public ElementTypeAttribute(string type)
        {
            Type = type;
        }
    }

    /// Keeps track of elements available in this and extension assemblies
    /// to be inherited and specialised
    public abstract class ElementFactory
    {

        /// represents a single entry in the dictionary of elements
        private struct ElementEntry
        {
            public readonly string TypeName;
            public readonly Type Class;

            public ElementEntry(string typeName, Type _class)
            {
                TypeName = typeName;
                Class = _class;
            }
        }

        private readonly ArrayList _elementsDictionary = new();
        /// the type of element to search for - set in inheriting classes
        protected Type elementType;
        /// the type of attribute to search for - set in inheriting classes
        protected Type elementTypeAttributeType;

        /// finds all elements in assembly and adds them to list of elements which can be created later
        public void SearchAssembly(Assembly assembly)
        {
            Type[] types = assembly.GetTypes();

            foreach (Type t in types)
            {
                if (!elementType.IsAssignableFrom(t))
                {
                    continue;
                }

                object[] attrs = t.GetCustomAttributes(elementTypeAttributeType, false);
                if (attrs.Length > 1)
                {
                    throw new MultipleElementTypesException(t);
                }

                if (attrs.Length == 1)
                {
                    string elementTypeName = ((ElementTypeAttribute)attrs[0]).Type;
                    AddElementEntry(new ElementEntry(elementTypeName, t), assembly);
                }
            }
        }

        private bool ElementTypeExists(string type)
        {
            foreach (object o in _elementsDictionary)
            {
                if (((ElementEntry)o).TypeName == type)
                {
                    return true;
                }
            }
            return false;
        }

        /// assembly parameter is required in order to be able to report errors
        private void AddElementEntry(ElementEntry e, Assembly assembly)
        {
            // check for presence first
            if (ElementTypeExists(e.TypeName))
            {
                throw new DuplicateElementTypeException(e.TypeName, e.Class, assembly);
            }
            _elementsDictionary.Add(e);
        }

        /// creates an element specified by it's type
        protected Element CreateElement(string type, object[] parameters)
        {
            foreach (object o in _elementsDictionary)
            {
                if (((ElementEntry)o).TypeName == type)
                {
                    // an element found
                    Type elementClass = ((ElementEntry)o).Class;
                    return CreateElement(elementClass, type, parameters);
                }
            }

            throw new ElementTypeNotFoundException(type);
        }

        private static Element CreateElement(Type elementClass, string type, object[] parameters)
        {
            // construct list of types of parameters going to the constructor
            Type[] paramTypes = Array.ConvertAll(parameters, p => p.GetType());

            // find the appropriate constructor
            ConstructorInfo cons = elementClass.GetConstructor(paramTypes);
            if (cons == null)
            {
                throw new NoSuitableConstructorException(type, elementClass);
            }

            return (Element)cons.Invoke(parameters);
        }
    }

}