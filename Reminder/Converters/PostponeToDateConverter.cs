using Reminder.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Reminder.Converters
{
    internal class PostponeToDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PostponeItem postpone = (PostponeItem)value;
            if (postpone == CompleteItem.Instance)
                return null;

            ProjectEndDateItem endDateItem = postpone as ProjectEndDateItem;
            if(endDateItem == null) return null;
            return endDateItem.EndDate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime? date = (DateTime?)value;
            if (date.HasValue)
                return new ProjectEndDateItem(date.Value);
            return CompleteItem.Instance;
        }
    }
}
