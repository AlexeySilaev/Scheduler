using System;
using System.Globalization;
using System.Windows.Data;

namespace Scheduler.Converters
{
    internal class StaticResourceToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int result = int.Parse(parameter.ToString());
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
