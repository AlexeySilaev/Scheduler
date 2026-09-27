using System.Collections.ObjectModel;
using Reminder.Extensions;
using Reminder.ViewModels;

namespace Reminder.UI
{
    public class ComingDayEventsViewModel
    {
        private bool _isReexpanding;

        public double PopupWidth { get; }
        public double PopupHeight { get; }
        public ObservableCollection<DayAboutItemViewModel> Items { get; }

        public ComingDayEventsViewModel(DayAboutItemViewModel[] items,
            double popupWidth, double popupHeight)
        {
            Items = new ObservableCollection<DayAboutItemViewModel>(items);
            PopupWidth = popupWidth;
            PopupHeight = popupHeight;
        }

        internal void SetExpandedItem(DayAboutItemViewModel expandedItem)
        {
            if (_isReexpanding) return;
            _isReexpanding = true;
            Items.ForEach(item => item.IsExpanded = item == expandedItem);
            _isReexpanding = false;
        }
    }
}
