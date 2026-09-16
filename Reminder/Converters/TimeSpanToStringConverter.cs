using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Reminder.Converters
{
    internal class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan span = (TimeSpan)value;
            string result = string.Empty;
            if (span.Hours != 0)
                result = $"{span.Hours} ч {span.Minutes} мин.";
            else
            {
                if (span.Minutes != 0)
                    result = $"{span.Minutes} мин {span.Seconds} сек.";
                else
                    result = span.TotalSeconds.ToString("0.##") + " сек.";
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
