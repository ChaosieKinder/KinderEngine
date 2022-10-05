using KinderEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinderEngine.Wpf.Comparers
{
    public class EnumDescriptionComparer : IComparer<Enum>
    {
        public int Compare(Enum x, Enum y)
        {
            return x.GetEnumDescription().CompareTo(y.GetEnumDescription());
        }
    }
}
