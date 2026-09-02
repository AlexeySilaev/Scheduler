using Reminder.Extensions;
using System;
using System.Windows;

namespace Reminder.ViewModels
{
    /// <summary>
    /// События, отслеживаемые при срабатывании таймера.
    /// </summary>
    public class TimerReminderItem : BaseViewModel, IComparable<TimerReminderItem>
    {
        private PostponeItem _postpone;
        private bool _isExpired;

        public ReminderItemViewModel Source { get; }
        public DateTime ReminderDateTime { get; }
        
        public bool IsExpired
        {
            get { return _isExpired; }
            private set
            {
                if (IsExpired == value) return;
                _isExpired = value;
                OnPropertyChanged(nameof(IsExpired));
                if (IsExpired)
                    Application.Current.Dispatcher.Invoke(() => Source.OnCurrentReminderIsExpired());
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
            get 
            {
                return Postpone.In(CompleteItem.Instance, BreakIntervalItem.Instance) == false;
            }
        }

        public TimerReminderItem(ReminderItemViewModel source)
        {
            _postpone = source.GetDefaultPostponeItem();
            _isExpired = source.State == ReminderState.Expired;
            Source = source;
            ReminderDateTime = source.GetReminderDateTime();
        }

        public bool CheckExpired(DateTime checkTime)
        {
            IsExpired = ReminderDateTime < checkTime;
            return IsExpired;
        }

        public void OnPostponeAccepted()
        {
            Source.OnPostponeAccepted(Postpone);
        }

        public override string ToString()
        {
            return Source.ToString();
        }

        private void OnPostponeChanged()
        {
            OnPropertyChanged(nameof(Postpone));
            OnPropertyChanged(nameof(AllowPostpone));
            if (AllowPostpone)
            {
                MainViewModel mainViewModel = MainViewModel.FindInstance();
                mainViewModel.Postpone(Source, Postpone);
            }
        }

        int IComparable<TimerReminderItem>.CompareTo(TimerReminderItem other)
        {
            return ReminderDateTime.CompareTo(other.ReminderDateTime);
        }
    }
}
