using Scheduler.Models;
using System;
using System.Globalization;
using Scheduler.Converters;
using Scheduler.Extensions;

namespace Scheduler.ViewModels
{
    internal class EventItemViewModel : ReminderItemViewModel
    {
        private DateTime _eventDate;
        private TimeSpan _eventTime;
        private Periodicity _periodicity;

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

        public EventItemViewModel(EventItem item) : base(item)
        {
            _eventDate = item.EventDate;
            _eventTime = item.EventDate.TimeOfDay;
            _periodicity = item.Periodicity;
        }

        public override DateTime GetReminderDateTime()
        {
            EventItem item = (EventItem)_item;
            return item.GetNearestEventDate();
        }

        public override void OnCurrentReminderIsExpired(bool initializing)
        {
            base.OnCurrentReminderIsExpired(initializing);
            OnPropertyChanged(nameof(NearestEventDate)); // для периодических
        }

        public override void Save()
        {
            base.Save();
            EventItem item = (EventItem)_item;
            item.EventDate = EventDate;
            item.Periodicity = Periodicity;
        }
    }
}
