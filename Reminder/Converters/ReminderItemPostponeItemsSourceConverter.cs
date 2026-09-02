using Reminder.Extensions;
using Reminder.UI;
using Reminder.ViewModels;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Reminder.Converters
{
    internal class ReminderItemPostponeItemsSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ReminderWindow reminderWindow = App.Current.Windows.FirstOrDefault<ReminderWindow>();
            if (reminderWindow == null) return DependencyProperty.UnsetValue;

            ReminderWindowViewModel dataContext = reminderWindow.DataContext;
            ReminderItemViewModel reminderItem = (ReminderItemViewModel)value;
            if (reminderItem.GetDefaultPostponeItem() == BreakIntervalItem.Instance)
                return dataContext.IntervalItems;
            
            return dataContext.PostponeItems;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
