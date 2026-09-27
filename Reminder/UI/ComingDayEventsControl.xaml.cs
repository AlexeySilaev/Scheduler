using Reminder.ViewModels;
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

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            Expander expander = (Expander)sender;
            ComingDayEventsViewModel viewModel = (ComingDayEventsViewModel)DataContext;
            viewModel.SetExpandedItem((DayAboutItemViewModel)expander.DataContext);
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            ComingDayEventsViewModel viewModel = (ComingDayEventsViewModel)DataContext;
            viewModel.SetExpandedItem(null);
        }
    }
}
