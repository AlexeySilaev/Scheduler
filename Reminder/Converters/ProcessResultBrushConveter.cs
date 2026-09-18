using Reminder.UI;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Reminder.Converters
{
    internal class ProcessResultBrushConveter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ProcessResult processResult = (ProcessResult)value;
            switch (processResult)
            {
                case ProcessResult.Ok:
                    return Brushes.Green;
                case ProcessResult.Error:
                    return Brushes.OrangeRed;
            }
            return SystemColors.WindowTextBrushKey;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
