using Reminder.Extensions;
using Reminder.Models;
using System;
using System.Diagnostics;

namespace Reminder.ViewModels
{
    public class EventItemViewModel : ReminderItemViewModel
    {
        static readonly int[] _rangeDays = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

        private DateTime _eventDate;
        private TimeSpan _eventTime;
        private Periodicity _periodicity;
        private int _weekDays;
        private DaysRangeViewModel _range;

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
                ValidateWeekDays();
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
                ValidateWeekDays();
            }
        }

        public int WeekDays
        {
            get { return _weekDays; }
            set
            {
                if (WeekDays == value) return;
                // оперируем как с набором флагов, поступающих из Log2IntToBoolConverter или при отмене редактирования
                if (value > 0)
                    _weekDays |= value;
                else if (value < 0)
                    _weekDays &= ~(-value);
                else
                    _weekDays = 0;
                OnPropertyChanged(nameof(WeekDays));
            }
        }

        public DaysRangeViewModel Range
        {
            get { return _range; }
            set
            {
                if (Range == value) return;
                _range = value;
                Range?.SetOwner(this);
                OnPropertyChanged(nameof(Range));
            }
        }

        public int[] RangeDays { get { return _rangeDays; } }

        public DateTime NearestEventDate
        {
            get { return GetReminderDateTime(); }
        }

        public override ReminderItemType Type
        {
            get { return ReminderItemType.Event; }
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
            _weekDays = item.WeekDays;
            _range = item.Range != null ? new DaysRangeViewModel(item.Range) : null;
            _ringtone = item.Ringtone;
            Range?.SetOwner(this);
        }

        public override DateTime GetReminderDateTime()
        {
            return this.GetNearestEventDate(FullEventDate, WeekDays, Range);
        }

        public override PostponeItem GetDefaultPostponeItem()
        {
            if (Range != null)
                return BreakIntervalItem.Instance;
            return CompleteItem.Instance;
        }

        public override void OnPostponeAccepted(PostponeItem postpone) 
        {
            if (Range != null && postpone == BreakIntervalItem.Instance)
                Range.State = DaysRangeState.Broken;
        }

        public override void OnCurrentReminderIsExpired()
        {
            base.OnCurrentReminderIsExpired();
            OnPropertyChanged(nameof(NearestEventDate)); // для периодических
        }

        public override string Validate(bool isNew)
        {
            string result = base.Validate(isNew);
            if (string.IsNullOrEmpty(result))
            {
                // у существующих событий нужно дать редактировать в том числе текст и дни недели
                if (FullEventDate < DateTime.Now && isNew)
                    result = "Дата события меньше текущей.";
            }
            return result;
        }

        public override void EndEdit()
        {
            base.EndEdit();
            EventItem.EventDate = FullEventDate;
            EventItem.Periodicity = Periodicity;
            EventItem.WeekDays = WeekDays;
            EventItem.Range = Range?.ToDaysRange();
            EventItem.Ringtone = Ringtone;
        }

        public override void CancelEdit()
        {
            base.CancelEdit();
            EventDate = EventItem.EventDate.Date;
            EventTime = EventItem.EventDate.TimeOfDay;
            WeekDays = EventItem.WeekDays;
            Range = EventItem.Range != null ? new DaysRangeViewModel(EventItem.Range) : null;
            Ringtone = EventItem.Ringtone;
        }

        public override void Save()
        {
            ReminderModel.Instance.Events.Add(EventItem);
        }

        private void ValidateWeekDays()
        {
            WeekDays = 0;
            if (Periodicity == Periodicity.Weekly)
                WeekDays = Convert.ToInt32(Math.Pow(2, Convert.ToInt32(EventDate.DayOfWeek)));
        }
    }

    [DebuggerDisplay("Days = {Days}, State = {State}")]
    public class DaysRangeViewModel : BaseViewModel
    {
        private EventItemViewModel _owner;
        private int _days;
        private DaysRangeState _state;
        public int _remainActiveDays;

        public DaysRangeViewModel(DaysRange range)
        {
            _days = range.Days;
            _state = range.State;
        }

        public int Days
        {
            get { return _days; }
            set
            {
                if (Days == value) return;
                _days = value;
                OnPropertyChanged(nameof(Days));
            }
        }

        public DaysRangeState State
        {
            get { return _state; }
            set
            {
                if (State == value) return;
                _state = value;
                _owner.EndEdit();
                ReminderModel.Instance.Save();
                OnPropertyChanged(nameof(State));
            }
        }

        public int RemainActiveDays
        {
            get { return _remainActiveDays; }
            private set
            {
                if (RemainActiveDays == value) return;
                _remainActiveDays = value;
                OnPropertyChanged(nameof(RemainActiveDays));
            }
        }

        public void SetOwner(EventItemViewModel owner)
        {
            _owner = owner;
        }

        public DaysRange ToDaysRange()
        { 
            return new DaysRange { Days = Days, State = State };
        }

        public void Clear()
        {
            State = DaysRangeState.Dormant;
            RemainActiveDays = 0;
        }

        public DateTime CheckApplyRange(DateTime reminderDate, DateTime now, bool checkPrevMonth)
        {
            (DateTime IntervalTime, bool InInterval, int RemainActiveDays) checkResult = CheckInterval(reminderDate, now);
            if (checkResult.InInterval && State == DaysRangeState.Active)
            {
                reminderDate = checkResult.IntervalTime;
            }
            else if (checkPrevMonth)
            {
                // можем ли мы перейти через месяц промежутком Days?
                DateTime previousMonthDate = reminderDate.AddMonths(-1);
                checkResult = CheckInterval(previousMonthDate, now);
                if (checkResult.InInterval && State == DaysRangeState.Active)
                {
                    reminderDate = checkResult.IntervalTime;
                }
            }

            if (checkResult.InInterval == false)
                State = DaysRangeState.Dormant;
            RemainActiveDays = checkResult.RemainActiveDays;
            return reminderDate;
        }

        private (DateTime, bool, int) CheckInterval(DateTime from, DateTime now)
        {
            if (from < now && from.AddDays(Days) >= now)
            {
                TimeSpan span = from.AddDays(Days) - now;
                DateTime result = from.AddDays(Days - span.Days);
                if (State != DaysRangeState.Broken)
                    State = DaysRangeState.Active;
                span = from.AddDays(Days) - result;
                return (result, true, span.Days + 1);
            }
            return (from, false, 0);
        }
    }
}
