using Reminder.ViewModels;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Reminder.Converters
{
    internal class FileSecretPresentationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            SecretFileViewModel file = (SecretFileViewModel)value;
            return $"{file.Name} - {GetFileLengthPresentation(file.Length)}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        private static string GetFileLengthPresentation(double bytes)
        {
            if (bytes == 0) return "0 б";

            string[] suffixes = { "б", "Кб", "Мб", "Гб", "Тб" };
            int i = 0;

            // Находим подходящую единицу измерения
            while (bytes >= 1024 && i < suffixes.Length)
            {
                bytes /= 1024.0;
                i++;
            }

            // Форматируем: убираем лишние нули после запятой, но сохраняем другие цифры
            return bytes.ToString("0.##") + " " + suffixes[i];
        }
    }
}
