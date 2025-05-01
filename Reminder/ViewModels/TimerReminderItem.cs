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

        public ReminderItemViewModel Source { get; }
        public DateTime ReminderDateTime { get; }
        public bool IsExpired { get; private set; }

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


        public TimerReminderItem(ReminderItemViewModel source)
        {
            _postpone = CompleteItem.Instance;
            Source = source;
            ReminderDateTime = source.GetReminderDateTime();
        }

        public bool CheckExpired(DateTime checkTime, bool initializing)
        {
            IsExpired = ReminderDateTime < checkTime;
            if (IsExpired)
                Application.Current.Dispatcher.Invoke(() => Source.OnCurrentReminderIsExpired(initializing));
            return IsExpired;
        }

        public override string ToString()
        {
            return Source.ToString();
        }

        private void OnPostponeChanged()
        {
            OnPropertyChanged(nameof(Postpone));
            OnPropertyChanged(nameof(AllowPostpone));
            OnPropertyChanged(nameof(PostponeDescription));
            if (AllowPostpone)
            {
                MainViewModel mainViewModel = MainViewModel.FindInstance();
                mainViewModel.Postpone(Source, Postpone.Minutes);
            }
        }

        int IComparable<TimerReminderItem>.CompareTo(TimerReminderItem other)
        {
            return ReminderDateTime.CompareTo(other.ReminderDateTime);
        }
    }
}
