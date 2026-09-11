using Reminder.ViewModels;
using System.Windows.Input;

namespace Reminder.UI
{
    internal class UnlockSecretsViewModel : BaseViewModel
    {
        public ICommand ManagementCommand { get; }

        public UnlockSecretsViewModel()
        {
            ManagementCommand = MainViewModel.FindInstance().ManagementCommand;
        }
    }
}
