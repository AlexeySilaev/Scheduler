using Scheduler.Models;
using System;
using Scheduler.Extensions;

namespace Scheduler.ViewModels
{
    internal class EventItemViewModel : ReminderItemViewModel
    {
        private DateTime _eventDate;
        private TimeSpan _eventTime;
        private Periodicity _periodicity;

        private EventItem EventItem
        {
            get { return (EventItem)_item; }
        }

        public DateTime EventDate
        {
            get { return _eventDate; }
            set
            {
                if (EventDate == value) return;
                _eventDate = value;
                OnPropertyChanged(nameof(EventDate));
            }
        }

        public TimeSpan EventTime
        {
            get { return _eventTime; }
            set
            {
                if (EventTime == value) return;
                _eventTime = value;
                OnPropertyChanged(nameof(EventTime));
            }
        }

        public Periodicity Periodicity
        {
            get { return _periodicity; }
            set
            {
                if (Periodicity == value) return;
                _periodicity= value;
                OnPropertyChanged(nameof(Periodicity));
            }
        }
        
        public DateTime NearestEventDate
        {
            get
            {
                return GetReminderDateTime();
            }
        }

        public override SchedulerItemType Type
        {
            get { return SchedulerItemType.Event; }
        }

        public override bool IsOneTimeReminder
        {
            get { return Periodicity == Periodicity.Once; }
        }

        public override string PeriodicityDescription
        {
            get { return Periodicity.GetDescription(); }
        }

        public DateTime FullEventDate
        {
            get
            {
                return EventDate.AddHours(EventTime.Hours)
                    .AddMinutes(EventTime.Minutes).AddSeconds(EventTime.Seconds);
            }
        }

        public EventItemViewModel(EventItem item) : base(item)
        {
            // для удобства работы в интерфейсе дату поделим на две части
            _eventDate = item.EventDate.Date;
            _eventTime = item.EventDate.TimeOfDay;
            _periodicity = item.Periodicity;
            _ringtone = item.Ringtone;
        }

        public override DateTime GetReminderDateTime()
        {
            EventItem item = new EventItem { EventDate = FullEventDate, Periodicity = Periodicity };
            return item.GetNearestEventDate();
        }

        public override void OnCurrentReminderIsExpired(bool initializing)
        {
            base.OnCurrentReminderIsExpired(initializing);
            OnPropertyChanged(nameof(NearestEventDate)); // для периодических
        }

        public override string Validate()
        {
            string result = base.Validate();
            if (string.IsNullOrEmpty(result))
            {
                if (FullEventDate < DateTime.Now)
                    result = "Дата события меньше текущей.";
            }
            return result;
        }

        public override void EndEdit()
        {
            base.EndEdit();
            EventItem.EventDate = FullEventDate;
            EventItem.Periodicity = Periodicity;
            EventItem.Ringtone = Ringtone;
        }

        public override void CancelEdit()
        {
            base.CancelEdit();
            EventDate = EventItem.EventDate.Date;
            EventTime = EventItem.EventDate.TimeOfDay;
            Ringtone = EventItem.Ringtone;
        }

        public override void Save()
        {
            if (State == ReminderState.Active)
                SchedulerModel.Instance.Events.Add(EventItem);
        }
    }
}
