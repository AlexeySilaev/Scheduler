using Reminder.Models;
using Reminder.Resources.Cipher;
using Reminder.ViewModels;
using System;

namespace Reminder.UI
{
    public class ManagementViewModel : BaseViewModel
    {
        private readonly ISupportHostContent _secretsHostContent;
        private bool _canExecuteSecretsCommand;

        public bool ShowSecretsGroup 
        {
            get { return ReminderModel.Instance.Settings.UnlockSecretsKey != Guid.Empty; }
        }

        public bool AllowUnblock
        {
            get { return _secretsHostContent.IsHostContentAllowed; }
        }

        public Command LockOrUnlockSecrectsCommand { get; }

        public ManagementViewModel(ISupportHostContent secretsHostContent)
        { 
            _secretsHostContent = secretsHostContent;
            LockOrUnlockSecrectsCommand = new Command(OnLockOrUnlockSecrects, CanExecuteSecretsCommand);
        }

        internal void OnSecretsPasswordChanged(string secretsPassword)
        {
            _canExecuteSecretsCommand = ReminderModel.Instance.Settings.UnlockSecretsKey == secretsPassword.ToGuid();
            LockOrUnlockSecrectsCommand.RaiseCanExecuteChanged();
        }

        private bool CanExecuteSecretsCommand()
        {
            return _canExecuteSecretsCommand;
        }

        private void OnLockOrUnlockSecrects()
        {
            _secretsHostContent.SetHostContentAllowed(_secretsHostContent.IsHostContentAllowed);
            OnPropertyChanged(nameof(AllowUnblock));
        }
    }
}
