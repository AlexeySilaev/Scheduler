using System.Collections.ObjectModel;
using System.Linq;
using Scheduler.ViewModels;

namespace Scheduler.UI
{
    internal class ReminderWindowViewModel : BaseViewModel
    {
        public ObservableCollection<ReminderItemViewModel> Items { get; }
        public PostponeItem[] PostponeItems { get; }
        public ReminderWindow View { get; set; }

        public ReminderWindowViewModel()
        {
            Items = new ObservableCollection<ReminderItemViewModel>();
            PostponeItems = new[]
            {
                CompleteItem.Instance,
                new PostponeItem("5 мин", 5),
                new PostponeItem("15 мин", 15),
                new PostponeItem("30 мин", 30),
                new PostponeItem("1 час", 60),
                new PostponeItem("2 часа", 120),
                new PostponeItem("4 часа", 240),
            };
        }

        public bool IsAllOneTineItemsPostponed()
        {
            return Items.Count(item => item.IsOneTimeReminder)
                   == Items.Count(item => item.IsOneTimeReminder && item.AllowPostpone);
        }
    }
}

