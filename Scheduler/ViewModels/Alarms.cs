using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Scheduler.Models;

namespace Scheduler.ViewModels
{
    public class Alarms : ReminderGroup
    {
        private List<AlarmItemViewModel> _allItems;
        private ObservableCollection<AlarmItemViewModel> _items;

        public override string Name
        {
            get { return "Будильники"; }
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
            _allItems = SchedulerModel.Instance.Alarms.Select(a => new AlarmItemViewModel(a))
                .OrderBy(a => a.AlarmTime)
                .ToList();
            _items = new ObservableCollection<AlarmItemViewModel>(_allItems);
        }

        public override void Filter(string searchText)
        {
            var filtered = _allItems.Where(item => item.Text.ToLower().Contains(searchText.ToLower()));
            _items = new ObservableCollection<AlarmItemViewModel>(filtered);
            OnPropertyChanged(nameof(Items));
        }

        public override void Remove(SchedulerItemViewModel item)
        {
            Debug.Assert(item.Type == SchedulerItemType.Alarm);
            AlarmItemViewModel alarmItem = (AlarmItemViewModel)item;
            _allItems.Remove(alarmItem);
            _items.Remove(alarmItem);
        }

        public override SchedulerItemViewModel CreateNewItem()
        {
            return new AlarmItemViewModel(new AlarmItem { Text = string.Empty });
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
                case SchedulerItemType.Alarm:
                    break;
                case SchedulerItemType.Event:
                    EventItemViewModel postponedEvent = (EventItemViewModel)postponed;
                    if (postponedEvent.IsOneTimeReminder == false)
                        throw new InvalidOperationException();
                    break;
                default:
                    throw new NotSupportedException();
            }

            AlarmItem item = new AlarmItem();
            item.AlarmTime = TimeSpan.FromMinutes(postponeMinutes);
            item.Text = postponed.Text;
            _allItems.Add(new AlarmItemViewModel(item));
            UpdateRemainTime();
        }
    }
}
