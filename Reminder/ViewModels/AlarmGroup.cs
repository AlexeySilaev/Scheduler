using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Reminder.Models;

namespace Reminder.ViewModels
{
    public class AlarmGroup : ReminderGroup<AlarmItemViewModel>
    {
        public override string Name
        {
            get { return "Напоминания"; }
        }

        public override string ImagePath
        {
            get { return "/Resources/sandclock32x32.png"; }
        }

        public override string Description
        {
            get { return "Напоминание - вид уведомления, работающий по принципу таймера. Максимум интервала - сутки. Сортируются по времени срабатывания."; }
        }


        public override void Save()
        {
            ReminderModel.Instance.Alarms.Clear();
            base.Save();
        }

        public override void Remove(SchedulerItemViewModel item)
        {
            Debug.Assert(item.Type == ReminderItemType.Alarm);
            base.Remove(item);
        }

        public override SchedulerItemViewModel CreateNewItem()
        {
            return new AlarmItemViewModel(new AlarmItem { Text = string.Empty });
        }

        public override void CheckAddNew(SchedulerItemViewModel item)
        {
            Debug.Assert(item.Type == ReminderItemType.Alarm);
            base.CheckAddNew(item);
        }

        public void UpdateRemainTime()
        {
            _allItems.ForEach(item => item.UpdateRemainTime());
        }

        public void AddPostponed(ReminderItemViewModel postponed, int postponeMinutes)
        {
            switch (postponed.Type)
            {
                case ReminderItemType.Alarm:
                case ReminderItemType.Event:
                    break;
                default:
                    throw new NotSupportedException();
            }

            AlarmItem item = new AlarmItem();
            item.AlarmTimeMinutes = postponeMinutes;
            item.Text = postponed.Text;
            item.Ringtone = postponed.Ringtone;
            CheckAddNew(new AlarmItemViewModel(item));
            UpdateRemainTime();
        }

        protected override IEnumerable<AlarmItemViewModel> FilterExpiredItems()
        {
            return FilterExpiredItems(ReminderModel.Instance.Alarms, item => new AlarmItemViewModel(item));
        }

        protected override IOrderedEnumerable<AlarmItemViewModel> OrderItems(IEnumerable<AlarmItemViewModel> alarms)
        {
            return alarms.OrderBy(a => a.AlarmTime);
        }

        // сортировка по возрастанию времени срабатывания
        protected override int Compare(AlarmItemViewModel x, AlarmItemViewModel y)
        {
            return x.RemainTime.CompareTo(y.RemainTime);
        }

    }
}
