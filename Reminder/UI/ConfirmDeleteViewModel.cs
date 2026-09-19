using System;
using System.Windows.Input;
using Reminder.ViewModels;
using Reminder.Extensions;
using Reminder.Resources.ViewModels;

namespace Reminder.UI
{
    public class ConfirmDeleteViewModel : BaseViewModel
    {
        private readonly SchedulerItemViewModel _schedulerItem;
        private readonly Action<bool> _deleteMethod;

        public string Question
        {
            get { return $"Удалить элемент «{_schedulerItem.Type.GetDescription()}»?"; }
        }

        public string ReminderText
        {
            get { return _schedulerItem.Text; }
        }

        public ICommand YesCommand { get; }
        public ICommand NoCommand { get; }

        public ConfirmDeleteViewModel(SchedulerItemViewModel schedulerItem, Action<bool> deleteMethod)
        {
            _schedulerItem = schedulerItem;
            _deleteMethod = deleteMethod;
            YesCommand = new Command(OnYes);
            NoCommand = new Command(OnNo);
        }

        private void OnYes()
        {
            _deleteMethod(true);
        }

        private void OnNo()
        {
            _deleteMethod(false);
        }
    }
}
