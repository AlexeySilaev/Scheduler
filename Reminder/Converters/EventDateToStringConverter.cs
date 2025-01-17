using System;
using System.Globalization;
using System.Windows.Data;
using Reminder.ViewModels;
using Reminder.Extensions;
using System.Threading;

namespace Reminder.Converters
{
    internal class EventDateToStringConverter : IMultiValueConverter
    {
        public object Convert(
            object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            ReminderState state = (ReminderState)values[0];
            if (state != ReminderState.Active)
                return state.GetDescription().ToLower();

            DateTime eventDate = (DateTime)values[1];
            culture = Thread.CurrentThread.CurrentCulture;
            return eventDate.ToString("dd.MM.yyyy HH:mm", culture);
        }

        public object[] ConvertBack(
            object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
