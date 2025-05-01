using Reminder.Extensions;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Reminder.ViewModels;

namespace Reminder.Converters
{
    internal class AlarmTimeToStringConverter : IMultiValueConverter
    {
        public object Convert(
            object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue)
                return null;

            ReminderState state = (ReminderState)values[0];
            if (state != ReminderState.Active)
                return state.GetDescription().ToLower();

            TimeSpan ts = (TimeSpan)values[1];
            if (ts.TotalSeconds < 60)
                return "несколько десятков секунд";
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
