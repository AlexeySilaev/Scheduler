using Reminder.ViewModels;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Reminder.Converters
{
    internal class ReminderItemTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ReminderItemType type = (ReminderItemType)value;
            ReminderItemType[] allowedTypes = (ReminderItemType[])parameter;
            return allowedTypes.Contains(type) ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
