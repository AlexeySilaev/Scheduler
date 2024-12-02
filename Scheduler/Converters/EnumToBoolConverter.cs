using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace Scheduler.Converters
{
    internal class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Enum member = (Enum)parameter;
            return member.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.Equals(false))
                return DependencyProperty.UnsetValue;
            else
                return parameter;
        }
    }
}
