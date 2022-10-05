using System;
using System.ComponentModel;
using System.Reflection;

namespace KinderEngine.Core
{
    /// <summary> Enum Extension Methods </summary>
    /// <typeparam name="T"> type of Enum </typeparam>
    public static class EnumExtensions
    {
        public static int TotalValues(this Enum enumInstance)
        {
            var type = enumInstance.GetType();

            if (!type.IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            return Enum.GetNames(type).Length;
        }

        public static string GetEnumDescription(this Enum enumInstance)
        {
            FieldInfo fi = enumInstance.GetType().GetField(enumInstance.ToString());
            DescriptionAttribute[] attributes =
              (DescriptionAttribute[])fi.GetCustomAttributes
              (typeof(DescriptionAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Description : enumInstance.ToString();
        }

        public static string GetEnumName<T>(string description)
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");
            System.Type value = typeof(T);
            FieldInfo[] fis = value.GetFields();
            foreach (FieldInfo fi in fis)
            {
                DescriptionAttribute[] attributes =
                  (DescriptionAttribute[])fi.GetCustomAttributes
                  (typeof(DescriptionAttribute), false);
                if (attributes.Length > 0)
                {
                    if (attributes[0].Description == description)
                    {
                        return fi.Name;
                    }
                }
            }
            return description;
        }

    }
}
