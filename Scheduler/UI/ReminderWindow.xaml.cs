using System;
using System.Windows;

namespace Scheduler.UI
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
            context.View = null;
        }
    }
}
