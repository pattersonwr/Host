using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Host.Extensions
{
    public static class HostExtensions
    {
        // Change type links:
        // https://msdn.microsoft.com/en-us/library/system.convert.changetype.aspx
        // http://www.siepman.nl/blog/post/2012/03/06/Convert-to-unknown-generic-type-ChangeType-T.aspx

        // Would Type Switch be useful?
        // https://blogs.msdn.microsoft.com/jaredpar/2008/05/16/switching-on-types/
        // https://gist.github.com/Virtlink/8722649

        public static T ChangeType<T>(this object value, CultureInfo cultureInfo)
        {
            var toType = typeof(T);

            if (value == null) return default(T);

            if (value is string)
            {
                if (toType == typeof(Guid))
                {
                    return ChangeType<T>(new Guid(Convert.ToString(value, cultureInfo)), cultureInfo);
                }

                if ((string)value == string.Empty && toType != typeof(string))
                {
                    return ChangeType<T>(null, cultureInfo);
                }
            }

            else
            {
                if (typeof(T) == typeof(string))
                {
                    return ChangeType<T>(Convert.ToString(value, cultureInfo), cultureInfo);
                }
            }

            if (toType.IsGenericType &&
                toType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                toType = Nullable.GetUnderlyingType(toType); ;
            }

            bool canConvert = toType is IConvertible || (toType.IsValueType && !toType.IsEnum);

            if (canConvert)
            {
                return (T)Convert.ChangeType(value, toType, cultureInfo);
            }

            return (T)value;
        }

        public static T ChangeTypeList<T>(this object values, CultureInfo cultureInfo)
        {
            // Get the internal type of the list
            var toTypeInternal = typeof(T).GenericTypeArguments[0];
            Type toListType = typeof(List<>).MakeGenericType(new[] { toTypeInternal });
            IList toList = (IList)Activator.CreateInstance(toListType);

            // split the values
            var strings = values.ToString().Split(',');

            var fromList = ((IEnumerable)strings).Cast<object>().Select(x => x == null ? x : x.ToString()).ToArray();

            foreach (var item in fromList)
            {
                MethodInfo method = typeof(HostExtensions).GetMethod("ChangeType");
                MethodInfo generic = method.MakeGenericMethod(toTypeInternal);
                var value = generic.Invoke(null, new object[] { item, CultureInfo.CurrentCulture });

                toList.Add(value);
            }

            return (T)toList;
        }

        public static bool HasAttribute<T>(this MethodInfo method)
        {
            var attribute = method.GetCustomAttribute(typeof(T), false);

            return ( attribute != null ? true : false );
        }

        public static bool HasParameterType<T>(this IEnumerable<ParameterInfo> parameters)
        {
            var type = parameters.Where(x => x.ParameterType.Equals(typeof(T))).FirstOrDefault();

            return ( type != null ? true : false );
        }
    }
}
