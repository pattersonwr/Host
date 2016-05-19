using System;
using System.Globalization;
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

        public static bool HasDefaultParams(this ParameterInfo[] paramInfo)
        {
           foreach (var info in paramInfo)
           {
                if (info.HasDefaultValue)
                    return true;
           }

            return false;
        }
    }
}
