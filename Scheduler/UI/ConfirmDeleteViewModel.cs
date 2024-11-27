using System;
using System.Windows.Input;
using Scheduler.ViewModels;

namespace Scheduler.UI
{
    public class ConfirmDeleteViewModel : BaseViewModel
    {
        private readonly Action<bool> _deleteMethod;

        public SchedulerItemViewModel SchedulerItem { get; }
        public ICommand YesCommand { get; }
        public ICommand NoCommand { get; }

        public ConfirmDeleteViewModel(SchedulerItemViewModel schedulerItem, Action<bool> deleteMethod)
        {
            SchedulerItem = schedulerItem;
            YesCommand = new Command(OnYes);
            NoCommand = new Command(OnNo);
            _deleteMethod = deleteMethod;
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
