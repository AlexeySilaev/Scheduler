using System.Windows;
using Reminder.Launch;

namespace Reminder.Extensions
{
    internal static class WindowExtensions
    {
        public static void ShowMainWindow(this Window window)
        {
            IReminderClientProcess win = Application.Current.MainWindow as IReminderClientProcess;
            win?.ShowMainWindow();
        }
    }
}
