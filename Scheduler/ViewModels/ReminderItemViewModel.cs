using Scheduler.Models;
using System;
using System.ComponentModel;

namespace Scheduler.ViewModels
{
    public enum ReminderState
    {
        [Description("Активно")] Active,
        [Description("Устарело")] Obsolete,
        [Description("Выполнено")] Completed
    }

    public abstract class ReminderItemViewModel : SchedulerItemViewModel
    {
        private ReminderState _state;
        private PostponeItem _postpone;
        protected ReminderRingtone _ringtone;

        public ReminderState State
        {
            get { return _state; }
            private set
            {
                if (State == value) return;
                _state = value;
                OnPropertyChanged(nameof(State));
                Edit.RaiseCanExecuteChanged();
            }
        }

        public ReminderRingtone Ringtone
        {
            get { return _ringtone; }
            set
            {
                if (Ringtone == value) return;
                _ringtone = value;
                OnPropertyChanged(nameof(Ringtone));
                OnPropertyChanged(nameof(IsSettingsRingtone));
            }
        }

        public bool IsSettingsRingtone
        {
            get { return Ringtone == ReminderRingtone.AsIs; }
            set
            {
                if (value)
                    Ringtone = ReminderRingtone.AsIs;
                else
                    Ringtone = ReminderRingtone.Default;
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
            _state = ReminderState.Active;
            _ringtone = ReminderRingtone.AsIs;
            _postpone = CompleteItem.Instance;
        }

        public abstract DateTime GetReminderDateTime();

        public virtual void OnCurrentReminderIsExpired(bool initializing)
        {
            if(IsOneTimeReminder)
                State = initializing ? ReminderState.Obsolete : ReminderState.Completed;
        }

        protected override bool CanExecuteEdit()
        {
            return State == ReminderState.Active;
        }

        public override string Validate()
        {
            string result = base.Validate();
            if (string.IsNullOrEmpty(result))
            {
                if (CanExecuteEdit() == false)
                    result = "Сохранение неактивных напоминаний запрещено.";
            }
            return result;
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
