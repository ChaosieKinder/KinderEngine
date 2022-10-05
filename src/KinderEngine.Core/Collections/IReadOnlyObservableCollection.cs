using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace KinderEngine.Lib.Core.Generic
{
    public interface IReadOnlyObservableCollection<T> : IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
    }
}
