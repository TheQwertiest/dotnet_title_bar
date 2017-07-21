using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace naid {
        public class ExtendedCollection<T> : Collection<T> {

        public object Find(Predicate<T> predicate) {
            if (predicate == null)
                throw new NullReferenceException();

            foreach (T i in this) {
                if (predicate(i))
                    return i;
            }

            return default(T);
        }

        public List<T> FindAll(Predicate<T> predicate) {
            if (predicate == null)
                throw new NullReferenceException();
            
            List<T> res = new List<T>();
            
            foreach (T i in this) {
                if (predicate(i))
                    res.Add(i);
            }

            return res;
        }
    }
}
