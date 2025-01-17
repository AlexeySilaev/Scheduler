using System;
using System.Globalization;
using System.Threading;
using System.Windows.Data;

namespace Reminder.Converters
{
    public class DateTimeMethodToValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var methodName = parameter as string;
            if (value == null || methodName == null)
                return value;
            var methodInfo = value.GetType().GetMethod(methodName, Type.EmptyTypes);
            if (methodInfo == null)
                return value;
            culture = Thread.CurrentThread.CurrentCulture;
            DateTime methodValue = (DateTime)methodInfo.Invoke(value, Array.Empty<object>());
            return methodValue.ToString("dd.MM HH:mm", culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
