using System;
using System.Globalization;
using System.Windows.Data;

namespace Reminder.Converters
{
    public class SubstringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value.ToString().Replace(Environment.NewLine, string.Empty);
            int length = int.Parse(parameter.ToString());
            if(text.Length > length)
                text = text.Substring(0, length);
            return text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
