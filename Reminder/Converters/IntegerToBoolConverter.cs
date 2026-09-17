using System;
using System.Globalization;
using System.Windows.Data;

namespace Reminder.Converters
{
    internal class IntegerToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int? intValue = (int?)value;
            return intValue.HasValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolVal = (bool)value;
            string defaultValue = parameter.ToString();
            return boolVal ? (int?)int.Parse(defaultValue) : null;
        }
    }
}
