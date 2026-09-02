using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        public static bool In<T>(this T value, params T[] source)
        {
            return source.Contains(value);
        }

        public static bool In<T>(this T value, IEnumerable source)
        {
            T[] array = source.Cast<T>().ToArray();
            return value.In(array);
        }

        public static T FirstOrDefault<T>(this IEnumerable source)
        {
            foreach (var element in source)
                if (element is T result)
                    return result;
            return default(T);
        }

    }
}
