using System.Collections.ObjectModel;
using Reminder.Resources.ViewModels;
using Reminder.ViewModels;

namespace Reminder.UI
{
    public class ComingDayEventsViewModel : BaseViewModel
    {
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
    }
}
