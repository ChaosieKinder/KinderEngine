using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KinderEngine.Wpf.Collections
{
    public class ChecklistItem<T> : INotifyPropertyChanged
    {
        public const string DESCRIPTION_FIELD_NAME = "Description";
        public const string SELECTED_FIELD_NAME = "Selected";
        public const string VALUE_FIELD_NAME = "Value";

        private bool _selected;
        private T _value;
        private string _description;

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                OnPropertyChanged();
            }
        }

        public ChecklistItem()
        {
            _selected = false;
            _description = string.Empty;
            _value = default;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
