using Reminder.Extensions;
using Reminder.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Reminder.Converters
{
    internal class DaysRangeControlVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Periodicity periodicity = (Periodicity)value;
            if (periodicity.In(Periodicity.Monthly, Periodicity.Quarterly, Periodicity.EverySixMonths, Periodicity.Annually))
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
