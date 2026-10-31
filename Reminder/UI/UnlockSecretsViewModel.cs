using Reminder.ViewModels;
using System.Windows.Input;

namespace Reminder.UI
{
    internal class UnlockSecretsViewModel : HostContentViewModel
    {
        public override string Name { get { return "Секреты"; } }
        public ICommand ManagementCommand { get; }

        public UnlockSecretsViewModel()
        {
            ManagementCommand = MainViewModel.FindInstance().ManagementCommand;
        }
    }
}
