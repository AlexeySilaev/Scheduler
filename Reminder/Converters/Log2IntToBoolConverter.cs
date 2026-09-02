using System;
using System.Globalization;
using System.Windows.Data;

namespace Reminder.Converters
{
    internal class Log2IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int number = (int)value;
            int power = (int)parameter;
            int raisedInPower = System.Convert.ToInt32(Math.Pow(2, power));
            return (number & raisedInPower) != 0;
        }

        /// <summary>
        /// Вернет 2 в степени parameter, это число будет участвовать в побитовой операции.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool raisedInPower = (bool)value;
            int power = (int)parameter;
            return System.Convert.ToInt32(Math.Pow(2, power)) * (raisedInPower ? 1 : -1);
        }
    }
}
