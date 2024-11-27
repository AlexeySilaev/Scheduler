using Scheduler.Models;
using System;
using System.ComponentModel;

namespace Scheduler.ViewModels
{
    public enum ExpiredType
    {
        [Description("Активно")] Active,
        [Description("Устарело")] Obsolete,
        [Description("Истекло время")] Expired
    }

    public abstract class ReminderItemViewModel : SchedulerItemViewModel
    {
        private ExpiredType _expiredType;
        private PostponeItem _postpone;

        public ExpiredType ExpiredType
        {
            get { return _expiredType; }
            private set
            {
                if (ExpiredType == value) return;
                _expiredType = value;
                OnPropertyChanged(nameof(ExpiredType));
            }
        }

        public PostponeItem Postpone
        {
            get { return _postpone; }
            set
            {
                if (Postpone == value) return;
                _postpone = value;
                OnPostponeChanged();
            }
        }

        public bool AllowPostpone
        {
            get { return Postpone != CompleteItem.Instance; }
        }
        public string PostponeDescription
        {
            get { return Postpone.Description; }
        }

        public abstract bool IsOneTimeReminder { get; }
        public abstract string PeriodicityDescription { get; }

        protected ReminderItemViewModel(SchedulerItem item) : base(item)
        {
            _expiredType = ExpiredType.Active;
            _postpone = CompleteItem.Instance;
        }

        public abstract DateTime GetReminderDateTime();

        public virtual void OnCurrentReminderIsExpired(bool initializing)
        {
            if(IsOneTimeReminder)
                ExpiredType = initializing ? ExpiredType.Obsolete : ExpiredType.Expired;
        }

        private void OnPostponeChanged()
        {
            OnPropertyChanged(nameof(Postpone));
            OnPropertyChanged(nameof(AllowPostpone));
            OnPropertyChanged(nameof(PostponeDescription));
            if (AllowPostpone)
            {
                MainViewModel mainViewModel = MainViewModel.FindInstance();
                mainViewModel.Postpone(this, Postpone.Minutes);
            }
        }

    }
}
