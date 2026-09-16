using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Reminder.Converters
{
    internal class StringIsNotNullOrEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;
            string text = (string)value;
            return string.IsNullOrEmpty(text) == false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
