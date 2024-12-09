using System;
using Scheduler.Models;

namespace Scheduler.ViewModels
{
    internal class AlarmItemViewModel : ReminderItemViewModel
    {
        private TimeSpan _alarmTime;
        private TimeSpan _remainTime;
        private DateTime _nowDate;

        private AlarmItem AlarmItem
        {
            get { return (AlarmItem)_item; }
        }

        public TimeSpan AlarmTime
        {
            get { return _alarmTime; }
            set
            {
                if (AlarmTime == value) return;
                _alarmTime = value;
                OnPropertyChanged(nameof(AlarmTime));
            }
        }

        /// <summary>
        /// Сколько времени осталось до срабатывания.
        /// </summary>
        public TimeSpan RemainTime
        {
            get { return _remainTime; }
            set
            {
                if (RemainTime == value) return;
                _remainTime = value;
                OnPropertyChanged(nameof(RemainTime));
            }
        }

        public DateTime NowDate
        {
            get { return _nowDate; }
            set
            {
                if (NowDate == value) return;
                _nowDate = value;
                OnPropertyChanged(nameof(NowDate));
            }
        }

        public override SchedulerItemType Type
        {
            get { return SchedulerItemType.Alarm; }
        }

        public override bool IsOneTimeReminder
        {
            get { return true; }
        }

        public override string PeriodicityDescription
        {
            get { return string.Empty; }
        }

        public AlarmItemViewModel(AlarmItem item) : base(item)
        {
            _alarmTime = TimeSpan.FromMinutes(item.AlarmTimeMinutes);
            _remainTime = TimeSpan.FromMinutes(item.AlarmTimeMinutes);
            _nowDate = item.NowDate;
            _ringtone = item.Ringtone;
            UpdateRemainTime();
        }

        public override DateTime GetReminderDateTime()
        {
            return NowDate.AddHours(AlarmTime.Hours)
                .AddMinutes(AlarmTime.Minutes)
                .AddSeconds(AlarmTime.Seconds);
        }

        public void UpdateRemainTime()
        {
            RemainTime = GetReminderDateTime() - DateTime.Now;
        }

        public override void EndEdit()
        {
            base.EndEdit();
            AlarmItem.AlarmTimeMinutes = Convert.ToInt32(AlarmTime.TotalMinutes);
            NowDate = DateTime.Now;
            AlarmItem.NowDate = NowDate;
            AlarmItem.Ringtone = Ringtone;
            UpdateRemainTime();
        }

        public override void CancelEdit()
        {
            base.CancelEdit();
            AlarmTime = TimeSpan.FromMinutes(AlarmItem.AlarmTimeMinutes);
            NowDate = AlarmItem.NowDate;
            Ringtone = AlarmItem.Ringtone;
        }

        public override void Save()
        {
            if (State == ReminderState.Active)
                SchedulerModel.Instance.Alarms.Add(AlarmItem);
        }
    }
}
