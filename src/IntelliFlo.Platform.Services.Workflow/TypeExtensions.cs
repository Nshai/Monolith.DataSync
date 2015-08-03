using System;

namespace IntelliFlo.Platform.Services.Workflow
{
    public static class TypeExtensions
    {
        public static bool IsNumeric(this object value)
        {
            if (value == null)
                return false;

            return value.GetType().IsNumeric();
        }

        public static bool IsNumeric(this Type type)
        {
            if (type == null)
                return false;

            return
                type == typeof(int) || type == typeof(int?) ||
                type == typeof(long) || type == typeof(long?) ||
                type == typeof(decimal) || type == typeof(decimal?) ||
                type == typeof(double) || type == typeof(double?) ||
                type == typeof(float) || type == typeof(float?) ||
                type == typeof(short) || type == typeof(short?) ||
                type == typeof(byte) || type == typeof(byte?) ||
                type == typeof(uint) || type == typeof(uint?) ||
                type == typeof(ulong) || type == typeof(ulong?) ||
                type == typeof(ushort) || type == typeof(ushort?);

        }
    }
}
