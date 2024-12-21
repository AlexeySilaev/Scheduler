using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Reminder.Models;

namespace Reminder.ViewModels
{
    public class EventGroup : ReminderGroup
    {
        private List<EventItemViewModel> _allItems;
        private ObservableCollection<EventItemViewModel> _items;

        public override string Name
        {
            get { return "События"; }
        }

        public override string ImagePath
        {
            get { return "/Resources/event32x32.png"; }
        }

        public override IEnumerable Items
        {
            get { return _items; }
        }

        public override void Initialize()
        {
            _allItems = ReminderModel.Instance.Events.Select(e => new EventItemViewModel(e))
                .OrderBy(item => item.NearestEventDate)
                .ToList();
            _items = new ObservableCollection<EventItemViewModel>(_allItems);
        }

        public override void Save()
        {
            ReminderModel.Instance.Events.Clear();
            _allItems.ForEach(item => item.Save());
        }

        public override void Filter(string searchText)
        {
            var filtered = _allItems.Where(item => item.Text.ToLower().Contains(searchText.ToLower()));
            _items = new ObservableCollection<EventItemViewModel>(filtered);
            OnPropertyChanged(nameof(Items));
        }

        public override void Remove(SchedulerItemViewModel item)
        {
            Debug.Assert(item.Type == ReminderItemType.Event);
            EventItemViewModel eventItem = (EventItemViewModel)item;
            _allItems.Remove(eventItem);
            _items.Remove(eventItem);
        }

        public override SchedulerItemViewModel CreateNewItem()
        {
            return new EventItemViewModel(new EventItem { Text = string.Empty });
        }

        public override void CheckAddNew(SchedulerItemViewModel item)
        {
            Debug.Assert(item.Type == ReminderItemType.Event);
            if (_allItems.Contains(item) == false)
                _allItems.Add((EventItemViewModel)item);
            _allItems.Sort(this);
        }

        public override List<TimerReminderItem> GetTimerReminderItems()
        {
            return _allItems
                .Where(a => a.State == ReminderState.Active)
                .Select(a => new TimerReminderItem(a))
                .ToList();
        }

        // сортировка по возрастанию времени срабатывания
        protected override int Compare(SchedulerItemViewModel x, SchedulerItemViewModel y)
        {
            EventItemViewModel x1 = (EventItemViewModel)x;
            EventItemViewModel y1 = (EventItemViewModel)y;
            return x1.NearestEventDate.CompareTo(y1.NearestEventDate);
        }

    }
}
