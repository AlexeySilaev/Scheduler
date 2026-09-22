using Reminder.Extensions;
using Reminder.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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

        private void ReminderWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this) == false)
                Keyboard.AddPreviewLostKeyboardFocusHandler(this, PreviewChangeFocus);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            DataContext.OnWindowClosed();
            Keyboard.RemovePreviewLostKeyboardFocusHandler(this, PreviewChangeFocus);
        }

        private void PreviewChangeFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            // не даем закрыть TimeTextBox, е`ли мышь проходит над другим элементом
            // выпадающего списка
            if (e.Source == ReminderItems && e.NewFocus is ComboBoxItem && e.OldFocus is TimeTextBox)
                e.Handled = true;
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
