using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Scheduler.Models;

namespace Scheduler.ViewModels
{
    public class Events : ReminderGroup
    {
        private EventItemViewModel[] _allItems;
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
            _allItems = SchedulerModel.Instance.Events.Select(e => new EventItemViewModel(e))
                .OrderBy(item => item.NearestEventDate)
                .ToArray();
            _items = new ObservableCollection<EventItemViewModel>(_allItems);
        }

        public override void Filter(string searchText)
        {
            var filtered = _allItems.Where(item => item.Text.ToLower().Contains(searchText.ToLower()));
            _items = new ObservableCollection<EventItemViewModel>(filtered);
            OnPropertyChanged(nameof(Items));
        }

        public override List<TimerReminderItem> GetTimerReminderItems()
        {
            return _allItems
                .Where(a => a.ExpiredType == ExpiredType.Active)
                .Select(a => new TimerReminderItem(a))
                .ToList();
        }
    }
}
