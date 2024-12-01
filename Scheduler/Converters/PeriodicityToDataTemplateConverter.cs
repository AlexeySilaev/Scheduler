using System.Windows;
using System.Windows.Data;
using Scheduler.Models;
using System;
using System.Globalization;

namespace Scheduler.Converters
{
    internal class PeriodicityToDataTemplateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Periodicity periodicity = (Periodicity)value;
            if (periodicity == Periodicity.Once)
                return (DataTemplate)Application.Current.Resources["PeriodicityOnce"];

            return (DataTemplate)Application.Current.Resources["PeriodicityRepeated"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
