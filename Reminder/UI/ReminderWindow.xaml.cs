using Reminder.Extensions;
using System;
using System.Windows;
using System.Windows.Input;

namespace Reminder.UI
{
    /// <summary>
    /// Interaction logic for ReminderWindow.xaml
    /// </summary>
    public partial class ReminderWindow : Window
    {
        public ReminderWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            ReminderWindowViewModel context = (ReminderWindowViewModel)DataContext;
            context.OnWindowClosed();
        }

        private void OnBeforeMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount > 1)
            {
                Application.Current.MainWindow.ShowMainWindow();
                e.Handled = true;
            }
        }
    }
}
