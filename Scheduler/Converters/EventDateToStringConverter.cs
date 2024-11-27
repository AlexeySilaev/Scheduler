using Scheduler.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;
using Scheduler.Extensions;

namespace Scheduler.Converters
{
    internal class EventDateToStringConverter : IMultiValueConverter
    {
        public object Convert(
            object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            ExpiredType expiredType = (ExpiredType)values[0];
            if (expiredType != ExpiredType.Active)
                return expiredType.GetDescription().ToLower();

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
