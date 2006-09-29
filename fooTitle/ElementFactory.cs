using System;
using System.Reflection;
using System.Collections;

namespace fooTitle {

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

    [AttributeUsage(AttributeTargets.Class)]
    public class ElementTypeAttribute : Attribute {
        public String Type;

        public ElementTypeAttribute(String type){
            Type = type;
        }
    }

    public class Element  {

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
                if (((ElementEntry)o).TypeName  == type)  
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
        public Element CreateElement(String type, int placeholder) {
            foreach (Object o in elementsDictionary) {
                if (((ElementEntry)o).TypeName  == type) {
                    // an element found
                    Type elementClass = ((ElementEntry)o).Class;
                    ConstructorInfo cons = elementClass.GetConstructor(new Type[]{typeof(int)});
                    if (cons != null) {
                        return (Element)cons.Invoke(new Object[]{placeholder});
                    }
                    throw new NoSuitableConstructorException(type, elementClass);
                }
            }

            throw new ElementTypeNotFoundException(type);
        }
    }

}
