using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Hyves.Api
{
    /// <summary>
    /// A helper class for enums.
    /// </summary>
    public static class EnumHelper
    {
        /// <typeparam name="TValue">usually int</typeparam>
        public static List<TValue> GetValues<TEnum, TValue>()
        {
            List<TValue> values = new List<TValue>();
            Array array = GetEnumValues(typeof(TEnum));
            foreach (TValue item in array)
            {
                values.Add(item);
            }

            return values;
        }

        /// <summary>
        /// Get the description of a <see cref="Enum" /> value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A description of the <see cref="Enum" /> value.</returns>
        public static string GetDescription(Enum value)
        {
            FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes =
                  (DescriptionAttribute[])fieldInfo.GetCustomAttributes(
                  typeof(DescriptionAttribute), false);

            return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
        }

        public static string GetAllValuesAsString<TEnum>()
        {
            StringBuilder result = new StringBuilder();
            Array array = GetEnumValues(typeof(TEnum));
            foreach (TEnum item in array)
            {
                FieldInfo fieldInfo = item.GetType().GetField(item.ToString());
                DescriptionAttribute[] attributes =
                      (DescriptionAttribute[])fieldInfo.GetCustomAttributes(
                      typeof(DescriptionAttribute), false);

                string description = (attributes.Length > 0) ? attributes[0].Description : item.ToString();

                result.Append(string.Format("{0},", description));
            }

            return result.ToString();
        }

        public static Array GetEnumValues(Type enumerationType)
        {
            if (!enumerationType.IsEnum)
                throw new Exception("GetEnumValues(enumerationType)");

            object valAux = Activator.CreateInstance(enumerationType);
            FieldInfo[] fieldInfoArray = enumerationType.GetFields(BindingFlags.Static | BindingFlags.Public);

            Array res = Array.CreateInstance(enumerationType, fieldInfoArray.Length);
            for (int i = 0; i < res.Length; i++)
                res.SetValue(fieldInfoArray[i].GetValue(valAux), i);
            return res;
        }
    }
}
