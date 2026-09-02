using Reminder.Extensions;
using Reminder.ViewModels;
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
        internal new ReminderWindowViewModel DataContext 
        { 
            get { return (ReminderWindowViewModel)base.DataContext; } 
        }

        public ReminderWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            DataContext.OnWindowClosed();
        }

        private void OnBeforeMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount > 1)
            {
                Application.Current.MainWindow.ShowMainWindow();
                e.Handled = true;
            }
        }

        private void OnTimerReminderItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                FrameworkElement itemTemplate = (FrameworkElement)sender;
                DataContext.ReminderItemUnderMouse = (TimerReminderItem)itemTemplate.DataContext;
            }
        }
    }
}
