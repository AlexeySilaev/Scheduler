using System;

namespace Scheduler.ViewModels
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

        public bool CheckExpired(DateTime checkTime)
        {
            IsExpired = ReminderDateTime < checkTime;
            if (IsExpired)
                Source.OnCurrentReminderIsExpired();
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
