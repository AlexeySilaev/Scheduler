using Reminder.ViewModels;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Reminder.Converters
{
    internal class PostponeDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            OtherItem otherItem = value as OtherItem;
            if (value == otherItem)
            { 
                string result = string.Empty;
                if (otherItem.Time.Hours != 0)
                    result = $"{otherItem.Time.Hours} час ";
                if (otherItem.Time.Minutes != 0)
                    result += $"{otherItem.Time.Minutes} мин";
                return result;

            }
            return ((PostponeItem)value).Description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
