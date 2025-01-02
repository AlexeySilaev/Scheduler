using System;
using System.Collections.Generic;

namespace Reminder.Extensions
{
    internal static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T element in source)
                action(element);
        }

        public static bool IsEmpty<T>(this T[] source)
        {
            if (source == null) return true;
            return source.Length == 0;
        }
    }
}
