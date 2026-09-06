using System;
using Reminder.Models;

namespace Reminder.ViewModels
{
    public class AlarmItemViewModel : ReminderItemViewModel
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
            private set
            {
                if (RemainTime == value) return;
                _remainTime = value;
                OnPropertyChanged(nameof(RemainTime));
                OnPropertyChanged(nameof(ReminderDateTime));
            }
        }

        public DateTime NowDate
        {
            get { return _nowDate; }
            private set
            {
                if (NowDate == value) return;
                _nowDate = value;
                OnPropertyChanged(nameof(NowDate));
            }
        }

        public override ReminderItemType Type
        {
            get { return ReminderItemType.Alarm; }
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
            Update();
        }

        protected override DateTime CalculateReminderDateTime()
        {
            DateTime result = NowDate.AddHours(AlarmTime.Hours)
                .AddMinutes(AlarmTime.Minutes)
                .AddSeconds(AlarmTime.Seconds);
            UpdateRemainTime(result);
            return result;
        }

        public override PostponeItem GetDefaultPostponeItem()
        {
            return CompleteItem.Instance;
        }

        public override void OnPostponeAccepted(PostponeItem postpone) { }

        public void UpdateRemainTime()
        {
            UpdateRemainTime(ReminderDateTime);
        }

        private void UpdateRemainTime(DateTime reminderDateTime)
        {
            RemainTime = reminderDateTime - TimeProvider.Now;
        }

        public override void EndEdit()
        {
            base.EndEdit();
            AlarmItem.AlarmTimeMinutes = Convert.ToInt32(AlarmTime.TotalMinutes);
            NowDate = TimeProvider.Now;
            AlarmItem.NowDate = NowDate;
            AlarmItem.Ringtone = Ringtone;
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
            ReminderModel.Instance.Alarms.Add(AlarmItem);
        }
    }
}
