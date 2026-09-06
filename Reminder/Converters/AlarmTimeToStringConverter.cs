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
            if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
                return null;

            ReminderState state = (ReminderState)values[0];
            if (state != ReminderState.Active)
            {
                string result = state.GetDescription();
                if (state == ReminderState.Expired)
                {
                    TimeSpan expired = (TimeSpan)values[1];
                    result += ConvertToTime(expired, " на ");
                }
                return result;
            }

            const string prefix = "Сработает через ";
            TimeSpan ts = (TimeSpan)values[1];
            if (ts.TotalSeconds < 61.0)
            {
                if (parameter != null)
                {
                    string slowTimerInterval = parameter.ToString();
                    if (double.TryParse(slowTimerInterval, out var secsElapsed) && secsElapsed < 61.0)
                        if (ts.TotalSeconds < secsElapsed)
                        {
                            if (ts.TotalSeconds < 1.0)
                                return "Сработает сейчас";
                            // развлечем пользователя, покажем до срабатывания каждую секунду
                            return $"{prefix}{System.Convert.ToInt32(ts.TotalSeconds)} сек";
                        }
                }
                return "Сработает в течение минуты";
            }

            return ConvertToTime(ts, prefix);
        }

        private static string ConvertToTime(TimeSpan ts, string prefix)
        {
            string result = prefix;
            if(ts.Days != 0)
                result +=  $"{Math.Abs(ts.Days)} дн  ";
            if (ts.Hours != 0)
                result +=  $"{Math.Abs(ts.Hours)} ч  {Math.Abs(ts.Minutes)} мин";
            else 
                result += $"{Math.Abs(ts.Minutes)} мин";
            return result;
        }

        public object[] ConvertBack(
            object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { DependencyProperty.UnsetValue, DependencyProperty.UnsetValue };
        }
    }
}
