using Reminder.Resources.ViewModels;
using System.Windows.Controls;

namespace Reminder.ViewModels
{
    public class HostContent
    {
        public UserControl UserControl { get; }
        public HostContentViewModel ViewModel { get; }

        public HostContent(UserControl userControl, HostContentViewModel viewModel)
        { 
            UserControl = userControl;
            ViewModel = viewModel;
            UserControl.DataContext = ViewModel;
        }
    }

    public abstract class HostContentViewModel : BaseViewModel
    {
        public abstract string Name { get; }
    }
}
