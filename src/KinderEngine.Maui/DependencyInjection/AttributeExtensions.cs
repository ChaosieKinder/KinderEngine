using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KinderEngine.Maui.DependencyInjection
{
    public class AttributeRecord<T>
    {
        public T Attribute { get; init; }
        public Type ClassType { get; init; }
    }

    public static class AttributeExtensions
    {
        public static IEnumerable<AttributeRecord<T>> GetTypesWithAttribute<T>(this Assembly assembly)
            where T : Attribute
        {
            foreach (Type type in assembly.GetTypes())
            {
                var attributes = type.GetCustomAttributes<T>(true);
                foreach(var attribute in attributes)
                    yield return new AttributeRecord<T>()
                    {
                        Attribute = attribute,
                        ClassType = type
                    };
            }
        }
    }
}
