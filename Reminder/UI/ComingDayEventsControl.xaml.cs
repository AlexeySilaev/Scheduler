using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Reminder.UI
{
    /// <summary>
    /// Interaction logic for ComingDayEventsControl.xaml
    /// </summary>
    public partial class ComingDayEventsControl : UserControl
    {
        public ComingDayEventsControl()
        {
            InitializeComponent();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Popup popup =  Parent as Popup;
            if (popup != null)
                popup.IsOpen = false;
        }
    }
}
