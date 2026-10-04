using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Reminder.Models;

namespace Reminder.ViewModels
{
    public class EventGroup : ReminderGroup<EventItemViewModel>
    {
        public override string Name
        {
            get { return "События"; }
        }

        public override string ImagePath
        {
            get { return "/Resources/event32x32.png"; }
        }

        public override string Description
        {
            get { return "Событие - основной вид уведомления, срабатывающий одноразово на определенную дату или по периоду. Сортируются по дате срабатывания."; }
        }

        public override void Save()
        {
            ReminderModel.Instance.Events.Clear();
            base.Save();
        }

        public override void Remove(SchedulerItemViewModel item)
        {
            Debug.Assert(item.Type == ReminderItemType.Event);
            base.Remove(item);
        }

        public override SchedulerItemViewModel CreateNewItem()
        {
            return new EventItemViewModel(new EventItem { Text = string.Empty });
        }

        public override void CheckAddNew(SchedulerItemViewModel item)
        {
            Debug.Assert(item.Type == ReminderItemType.Event);
            base.CheckAddNew(item);
        }

        public void OnEventActivated(string searchText)
        {
            _allItems = _allItems.OrderBy(item => item.ReminderDateTime)
                .ToList();
            Filter(searchText);
        }

        protected override IEnumerable<EventItemViewModel> FilterExpiredItems()
        {
            return FilterExpiredItems(ReminderModel.Instance.Events, item => new EventItemViewModel(item));
        }

        protected override IOrderedEnumerable<EventItemViewModel> OrderItems(IEnumerable<EventItemViewModel> events)
        {
            return events.OrderBy(item => item.ReminderDateTime);
        }


        // сортировка по возрастанию времени срабатывания
        protected override int Compare(EventItemViewModel x, EventItemViewModel y)
        {
            return x.ReminderDateTime.CompareTo(y.ReminderDateTime);
        }

    }
}
