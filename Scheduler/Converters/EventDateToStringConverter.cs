using System;
using System.Globalization;
using System.Windows.Data;

namespace Scheduler.Converters
{
    internal class EventDateToStringConverter : IMultiValueConverter
    {
        public object Convert(
            object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool expired = (bool)values[0];
            if (expired)
                return "истекло время";

            DateTime eventDate = (DateTime)values[1];
            return eventDate.ToString("dd.MM.yyyy HH:mm", new CultureInfo("RU-ru"));
        }

        public object[] ConvertBack(
            object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
