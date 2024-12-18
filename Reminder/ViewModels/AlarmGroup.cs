using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Reminder.Models;

namespace Reminder.ViewModels
{
    public class AlarmGroup : ReminderGroup
    {
        private List<AlarmItemViewModel> _allItems;
        private ObservableCollection<AlarmItemViewModel> _items;

        public override string Name
        {
            get { return "Напоминания"; }
        }

        public override string ImagePath
        {
            get { return "/Resources/alarm32x32.png"; }
        }

        public override IEnumerable Items
        {
            get { return _items; }
        }

        public override void Initialize()
        {
            _allItems = ReminderModel.Instance.Alarms.Select(a => new AlarmItemViewModel(a))
                .OrderBy(a => a.AlarmTime)
                .ToList();
            _items = new ObservableCollection<AlarmItemViewModel>(_allItems);
        }

        public override void Save()
        {
            ReminderModel.Instance.Alarms.Clear();
            _allItems.ForEach(item => item.Save());
        }

        public override void Filter(string searchText)
        {
            var filtered = _allItems.Where(item => item.Text.ToLower().Contains(searchText.ToLower()));
            _items = new ObservableCollection<AlarmItemViewModel>(filtered);
            OnPropertyChanged(nameof(Items));
        }

        public override void Remove(SchedulerItemViewModel item)
        {
            Debug.Assert(item.Type == ReminderItemType.Alarm);
            AlarmItemViewModel alarmItem = (AlarmItemViewModel)item;
            _allItems.Remove(alarmItem);
            _items.Remove(alarmItem);
        }

        public override SchedulerItemViewModel CreateNewItem()
        {
            return new AlarmItemViewModel(new AlarmItem { Text = string.Empty });
        }

        public override void CheckAddNew(SchedulerItemViewModel item)
        {
            Debug.Assert(item.Type == ReminderItemType.Alarm);
            if (_allItems.Contains(item) == false)
                _allItems.Add((AlarmItemViewModel)item);
            _allItems.Sort(this);
        }

        public override List<TimerReminderItem> GetTimerReminderItems()
        {
            return _allItems
                .Where(a => a.State == ReminderState.Active)
                .Select(a => new TimerReminderItem(a))
                .ToList();
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
                    break;
                case ReminderItemType.Event:
                    EventItemViewModel postponedEvent = (EventItemViewModel)postponed;
                    if (postponedEvent.IsOneTimeReminder == false)
                        throw new InvalidOperationException();
                    break;
                default:
                    throw new NotSupportedException();
            }

            AlarmItem item = new AlarmItem();
            item.AlarmTimeMinutes = postponeMinutes;
            item.Text = postponed.Text;
            item.Ringtone = postponed.Ringtone;
            _allItems.Add(new AlarmItemViewModel(item));
            UpdateRemainTime();
        }

        // сортировка по возрастанию времени срабатывания
        protected override int Compare(SchedulerItemViewModel x, SchedulerItemViewModel y)
        {
            AlarmItemViewModel x1 = (AlarmItemViewModel)x;
            AlarmItemViewModel y1 = (AlarmItemViewModel)y;
            return x1.RemainTime.CompareTo(y1.RemainTime);
        }

    }
}
