using Reminder.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Reminder.Converters
{
    internal class SecretKindToDataTemplateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SecretKind kind = (SecretKind)value;
            if (kind == SecretKind.String)
                return (DataTemplate)Application.Current.Resources["SecretString"];

            return (DataTemplate)Application.Current.Resources["SecretFile"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
