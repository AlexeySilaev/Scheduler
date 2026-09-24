using System;
using System.ComponentModel;
using Reminder.Models;

namespace Reminder.ViewModels
{
    public enum ReminderState
    {
        [Description("Активно")] Active,
        [Description("Просрочено")] Expired,
        [Description("Отключено")] Disabled
    }

    public abstract class ReminderItemViewModel : SchedulerItemViewModel
    {
        private ReminderState _state;
        protected ReminderRingtone _ringtone;

        public ReminderState State
        {
            get { return _state; }
            protected set
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

        public abstract bool IsOneTimeReminder { get; }
        public abstract string PeriodicityDescription { get; }

        public DateTime ReminderDateTime { get; private set; }
        public virtual bool AllowAutoDeleteOnExpired { get { return IsOneTimeReminder; }}

        protected ReminderItemViewModel(ReminderItem item) : base(item)
        {
            _state = ReminderState.Active;
            _ringtone = ReminderRingtone.AsIs;
        }

        public abstract PostponeItem GetDefaultPostponeItem();
        public abstract void OnPostponeAccepted(PostponeItem postpone);

        public virtual void OnCurrentReminderIsExpired()
        {
            if (IsOneTimeReminder)
                State = ReminderState.Expired;
        }

        protected override bool CanExecuteEdit()
        {
            return State != ReminderState.Expired;
        }

        protected abstract DateTime CalculateReminderDateTime();

        public override string Validate(bool isNew)
        {
            string result = base.Validate(isNew);
            if (string.IsNullOrEmpty(result))
            {
                if (CanExecuteEdit() == false)
                    result = "Сохранение неактивных напоминаний запрещено.";
            }
            return result;
        }

        public void Update()
        {
            ReminderDateTime = CalculateReminderDateTime();
        }
    }
}
