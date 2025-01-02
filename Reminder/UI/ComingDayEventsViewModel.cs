using System.Collections.ObjectModel;
using Reminder.ViewModels;

namespace Reminder.UI
{
    public class ComingDayEventsViewModel : BaseViewModel
    {
        public double PopupWidth { get; }
        public double PopupHeight { get; }

        public ObservableCollection<EventItemViewModel> Items { get; }

        public ComingDayEventsViewModel(EventItemViewModel[] items,
            double popupWidth, double popupHeight)
        {
            Items = new ObservableCollection<EventItemViewModel>(items);
            PopupWidth = popupWidth;
            PopupHeight = popupHeight;
        }
    }
}
