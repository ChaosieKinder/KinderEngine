using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace KinderEngine.Wpf.Collections
{
    public enum DefaultSelectionState
    {
        Default,
        NoneChecked,
        AllChecked
    }
    /// <summary>
    /// Maps a collection on an object to a compatable collection for use 
    /// in a checkbox combobox or checkbox list wpf object.
    /// 
    /// Items are added and removed from the source array as they get checked/unchecked
    /// 
    /// Note that binding is currently one-directional: Changes on object will not be reflected in control.
    /// </summary>
    /// <typeparam name="K">Class ObservableCollection resides on</typeparam>
    /// <typeparam name="T">Value Type of member array</typeparam>
    public class CollectionChecklist<K, T>
        where K : class
    {
        protected DefaultSelectionState DefaultCheckState;

        /// <summary>
        /// Reference to array on object of Type K that we are updating when items in the checkbox change
        /// </summary>
        public ObservableCollection<T> SelectedValuesArray;

        /// <summary>
        /// Used as internal cache of current values for performance
        /// </summary>
        protected Dictionary<T, bool> IsSelectedCache;

        /// <summary>
        /// Itemsource for binding to checkboxcontrol
        /// </summary>
        public ObservableCollection<ChecklistItem<T>> ItemsSource { get; set; }

        /// <summary>
        /// Protected constructor sets up the important shared things. 
        /// </summary>
        /// <param name="obj">Object containing the list we want to map an enumeration.</param>
        /// <param name="getter">Getter for the property we want to create a mapping for.</param>
        protected CollectionChecklist(K obj,
            Func<K, ObservableCollection<T>> getter,
            DefaultSelectionState defaultCheckState)
        {
            DefaultCheckState = defaultCheckState;
            SelectedValuesArray = getter.Invoke(obj);
            IsSelectedCache = new Dictionary<T, bool>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj">Object containing the list we want to map an enumeration.</param>
        /// <param name="getter">Getter for the property we want to create a mapping for.</param>
        /// <param name="sourceArray"></param>
        public CollectionChecklist(K obj,
            Func<K, ObservableCollection<T>> getter,
            IEnumerable<T> sourceArray,
            DefaultSelectionState defaultCheckState = DefaultSelectionState.Default)
            : this(obj, getter, defaultCheckState)
        {
            ItemsSource = ConstructItemsource(sourceArray);
            ReSyncInitalToCollection();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj">Object containing the list we want to map an enumeration.</param>
        /// <param name="getter">Getter for the property we want to create a mapping for.</param>
        /// <param name="sourceArray">uses the Key of a dictionary for the name of values.</param>
        public CollectionChecklist(K obj,
            Func<K, ObservableCollection<T>> getter,
            IDictionary<string, T> sourceArray,
            DefaultSelectionState defaultCheckState = DefaultSelectionState.Default)
            : this(obj, getter, defaultCheckState)
        {
            ItemsSource = ConstructItemsource(sourceArray);
            ReSyncInitalToCollection();
        }

        protected ObservableCollection<ChecklistItem<T>> ConstructItemsource(IEnumerable<T> sourceArray)
        {
            var cache = new ObservableCollection<ChecklistItem<T>>();
            foreach (var e in sourceArray)
            {
                var checkedEnumMember = new ChecklistItem<T>()
                {
                    Description = e.ToString(),
                    Value = e,
                    Selected = GetInitialSelectedState(SelectedValuesArray.Contains(e))
                };
                IsSelectedCache.Add(checkedEnumMember.Value, checkedEnumMember.Selected);
                checkedEnumMember.PropertyChanged += CheckedEnumMember_PropertyChanged;
                cache.Add(checkedEnumMember);
            }
            return cache;
        }

        protected bool GetInitialSelectedState(bool varState)
        {
            switch(DefaultCheckState)
            {
                case DefaultSelectionState.Default:
                    return varState;
                case DefaultSelectionState.AllChecked:
                    return true;
                case DefaultSelectionState.NoneChecked:
                default:
                    return false;
            }
        }

        protected void ReSyncInitalToCollection()
        {
            if (DefaultCheckState == DefaultSelectionState.AllChecked)
                foreach (var o in ItemsSource)
                    if(!SelectedValuesArray.Contains(o.Value))
                        SelectedValuesArray.Add(o.Value);
            else if (DefaultCheckState == DefaultSelectionState.NoneChecked)
                SelectedValuesArray.Clear();
        }

        protected ObservableCollection<ChecklistItem<T>> ConstructItemsource(IDictionary<string, T> sourceArray)
        {
            var cache = new ObservableCollection<ChecklistItem<T>>();
            foreach (var e in sourceArray)
            {
                var checkedEnumMember = new ChecklistItem<T>()
                {
                    Description = e.Key,
                    Value = e.Value,
                    Selected = SelectedValuesArray.Contains(e.Value)
                };
                IsSelectedCache.Add(checkedEnumMember.Value, checkedEnumMember.Selected);
                checkedEnumMember.PropertyChanged += CheckedEnumMember_PropertyChanged;
                cache.Add(checkedEnumMember);
            }
            return cache;
        }

        protected void CheckedEnumMember_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ChecklistItem<T>.SELECTED_FIELD_NAME)
            {
                var item = (ChecklistItem<T>)sender;
                var previousValue = IsSelectedCache[item.Value];
                if (previousValue != item.Selected)
                {
                    IsSelectedCache[item.Value] = item.Selected;
                    if (item.Selected)
                        SelectedValuesArray.Add(item.Value);
                    else
                        SelectedValuesArray.Remove(item.Value);
                }
            }
        }
    }
}
