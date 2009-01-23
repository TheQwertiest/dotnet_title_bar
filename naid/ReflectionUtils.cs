using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace naid {
    
    public static class ReflectionUtils {

        public static T ConstructParameterless<T>(Type type) {
            foreach (ConstructorInfo cons in type.GetConstructors()) {
                if (cons.GetParameters().Length == 0)
                    return (T)cons.Invoke(new object[] { });
            }

            throw new InvalidOperationException("No parameterless constructor found for class " + type.ToString());

        }

        public static T ConstructParameterless<T>() {
            return ConstructParameterless<T>(typeof(T));
        }
    }
}
