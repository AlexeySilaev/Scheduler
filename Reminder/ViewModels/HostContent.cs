using System.Windows.Controls;

namespace Reminder.ViewModels
{
    public class HostContent
    {
        public UserControl UserControl { get; }
        public BaseViewModel ViewModel { get; }

        public HostContent(UserControl userControl, BaseViewModel viewModel)
        { 
            UserControl = userControl;
            ViewModel = viewModel;
            UserControl.DataContext = ViewModel;
        }
    }
}
