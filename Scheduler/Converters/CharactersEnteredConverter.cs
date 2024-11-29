using System;
using System.Globalization;
using System.Windows.Data;

namespace Scheduler.Converters
{
    internal class CharactersEnteredConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = (string)(value ?? string.Empty);
            int maxLength = int.Parse(parameter.ToString());
            return $"{text.Length} из {maxLength}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
