using Scheduler.Converters;
using System;
using System.Globalization;

namespace Scheduler.Extensions
{
    internal static class EnumExtensions
    {
        public static string GetDescription(this Enum enumMember)
        {
            EnumToStringConverter converter = new EnumToStringConverter();
            return converter.Convert(enumMember, enumMember.GetType(), null, CultureInfo.InvariantCulture)
                .ToString();

        }
    }
}
