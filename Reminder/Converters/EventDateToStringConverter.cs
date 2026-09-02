using Reminder.Extensions;
using Reminder.ViewModels;
using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Data;

namespace Reminder.Converters
{
    internal class EventDateToStringConverter : IMultiValueConverter
    {
        public object Convert(
            object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            ReminderState state = (ReminderState)values[0];
            if (state != ReminderState.Active)
                return state.GetDescription();

            DateTime eventDate = (DateTime)values[1];
            culture = Thread.CurrentThread.CurrentCulture;
            return "Сработает: " + eventDate.ToString("dd.MM.yyyy HH:mm", culture);
        }

        public object[] ConvertBack(
            object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { DependencyProperty.UnsetValue, DependencyProperty.UnsetValue };
        }
    }
}
