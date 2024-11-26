using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Scheduler.Converters
{
    internal class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visible = (bool)value;
            if (visible)
                return Visibility.Visible;
            bool setCollapsed = bool.Parse(parameter.ToString());
            return setCollapsed ? Visibility.Collapsed : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
