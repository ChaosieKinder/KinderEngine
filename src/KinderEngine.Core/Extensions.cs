using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinderEngine.Core
{
    public static class Extensions
    {
        /// <summary>
        /// Adds an item to a collection if it already does not exist in the collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="itemToAdd"></param>
        /// <returns>True if the item was added, else False.</returns>
        public static bool TryAddIfNotExists<T>(this ObservableCollection<T> collection, T itemToAdd)
        {
            if (collection.Contains(itemToAdd))
                return false;
            collection.Add(itemToAdd);
            return true;
        }

        public static void AddAll<T>(this ObservableCollection<T> collection, IEnumerable<T> itemToAdd)
        {
            if (itemToAdd == null)
                return;

            foreach (var n in itemToAdd)
                collection.Add(n);
        }
        
    }
}
