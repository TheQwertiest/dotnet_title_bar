using System;
using System.Collections.Generic;
using System.Text;

namespace naid {
    /// <summary>
    /// This delegate is used for binary searching in an array. It informs the search
    /// algorithm about the relation between current item in the array and the item
    /// the user needs to find.
    /// </summary>
    /// <returns>
    /// This delegate returns 0 if 'item' is equal to the item that is being searched
    /// for. Returns a negative value if 'item' is less than the item that is being
    /// searched and a positive value otherwise.
    /// </returns>
    public delegate int CustomCompareDelegate<T>(T item);

    public static class ListAlgorithms<T> {

        public static T BinarySearch(IList<T> list, CustomCompareDelegate<T> compare_fn) {
            int low = 0;
            int high = list.Count - 1;

            while (low <= high) {
                int mid = (low + high) / 2;
                int cmp = compare_fn(list[mid]);
                if (cmp > 0) {
                    high = mid - 1;
                } else if (cmp < 0) {
                    low = mid + 1;
                } else {
                    return list[mid]; // found
                }
            }
            return default(T); // not found
        }
    }
}
