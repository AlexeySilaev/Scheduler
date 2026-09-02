using Reminder.Models;
using Reminder.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Reminder.Converters
{
    internal class DaysRangeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool canSet = (bool)value;
            const int defaultDaysInRange = 5;
            return canSet ? new DaysRangeViewModel(new DaysRange { Days = defaultDaysInRange }) : null;
        }
    }
}
