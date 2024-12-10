using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Reminder.Converters
{
    internal class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var attribute =
                fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as
                    DescriptionAttribute;

            return attribute != null ? attribute.Description : value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
