using Scheduler.Models;
using System;
using System.Globalization;
using Scheduler.Converters;

namespace Scheduler.ViewModels
{
    internal class EventItemViewModel : ReminderItemViewModel
    {
        private DateTime _eventDate;
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
            get
            {
                EnumToStringConverter converter = new EnumToStringConverter();
                return converter.Convert(Periodicity, typeof(Periodicity), null, CultureInfo.InvariantCulture)
                    .ToString();
            }
        }

        public EventItemViewModel(EventItem item) : base(item)
        {
            _eventDate = item.EventDate;
            _periodicity = item.Periodicity;
        }

        public override DateTime GetReminderDateTime()
        {
            EventItem item = (EventItem)_item;
            return item.GetNearestEventDate();
        }

        public override void OnCurrentReminderIsExpired()
        {
            base.OnCurrentReminderIsExpired();
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
