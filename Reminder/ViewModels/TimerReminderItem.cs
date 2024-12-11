using System;
using System.Windows;

namespace Reminder.ViewModels
{
    /// <summary>
    /// События, отслеживаемые при срабатывании таймера.
    /// </summary>
    public class TimerReminderItem : IComparable<TimerReminderItem>
    {
        public ReminderItemViewModel Source { get; }
        public DateTime ReminderDateTime { get; }
        public bool IsExpired { get; private set; }

        public TimerReminderItem(ReminderItemViewModel source)
        {
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

        int IComparable<TimerReminderItem>.CompareTo(TimerReminderItem other)
        {
            return ReminderDateTime.CompareTo(other.ReminderDateTime);
        }
    }
}
