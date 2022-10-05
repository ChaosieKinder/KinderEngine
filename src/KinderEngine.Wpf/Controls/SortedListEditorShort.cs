using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KinderEngine.Wpf.Controls
{
    public class SortedListEditorShort : Control
    {
        static SortedListEditorShort()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SortedListEditorShort), new FrameworkPropertyMetadata(typeof(SortedListEditorShort)));
        }

        public ObservableCollection<short> Values { get; set; }

        public SortedListEditorShort()
        {
            Values = new ObservableCollection<short>();
        }
    }
}
