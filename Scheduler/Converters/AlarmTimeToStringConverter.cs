using System;
using System.Globalization;
using System.Windows.Data;

namespace Scheduler.Converters
{
    internal class AlarmTimeToStringConverter : IMultiValueConverter
    {
        public object Convert(
            object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool expired = (bool)values[0];
            if (expired)
                return "истекло время";

            TimeSpan ts = (TimeSpan)values[1];
            if (ts.TotalSeconds < 60)
                return "в течение минуты";
            if (ts.Hours > 0)
                return $"{ts.Hours} ч  {ts.Minutes} мин";
            return $"{ts.Minutes} мин";
        }

        public object[] ConvertBack(
            object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
