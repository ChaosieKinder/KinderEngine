using System;
using System.Collections.ObjectModel;

namespace KinderEngine.Wpf.Collections
{
    /// <summary>
    /// Maps a collection of enums on an object to a compatable collection for use 
    /// in a checkbox combobox or checkbox list wpf object.
    /// 
    /// Items are added and removed from the source array as they get checked/unchecked
    /// 
    /// Note that binding is currently one-directional: Changes on object will not be reflected in control.
    /// </summary>
    /// <typeparam name="K">Class ObservableCollection resides on</typeparam>
    /// <typeparam name="T">Enum Type of member array</typeparam>
    public class EnumerationCheckList<K, T> : CollectionChecklist<K, T>
        where K : class
        where T : struct, IConvertible
    {
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj">Object containing the list we want to map an enumeration.</param>
        /// <param name="getter">Getter for the property we want to create a mapping for.</param>
        public EnumerationCheckList(K obj, Func<K, ObservableCollection<T>> getter,
            DefaultSelectionState checkState = DefaultSelectionState.Default)
            : base(obj, getter, checkState)
        {
            ItemsSource = ConstructItemsource();
            ReSyncInitalToCollection();
        }
        

        protected ObservableCollection<ChecklistItem<T>> ConstructItemsource()
        {
            var cache = new ObservableCollection<ChecklistItem<T>>();
            foreach (var e in Enum.GetValues(typeof(T)))
            {
                var checkedEnumMember = new ChecklistItem<T>()
                {
                    Description = Enum.GetName(typeof(T), e),
                    Value = (T)e,
                    Selected = GetInitialSelectedState(SelectedValuesArray.Contains((T)e))
                };
                IsSelectedCache.Add(checkedEnumMember.Value, checkedEnumMember.Selected);
                checkedEnumMember.PropertyChanged += CheckedEnumMember_PropertyChanged;
                cache.Add(checkedEnumMember);
            }
            return cache;
        }
    }
}
