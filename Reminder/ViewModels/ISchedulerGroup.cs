using System.Collections;
using System.Collections.Generic;

namespace Reminder.ViewModels
{
    public interface ISchedulerGroup
    {
        string Name { get; }
        string ImagePath { get; }
        ICollection Items { get; }
        string Description { get; }
        bool IsVisible { get; }

        void Initialize();
        void InitializeComplete();
        void CheckAddNew(SchedulerItemViewModel item);
        SchedulerItemViewModel CreateNewItem();
        void Save();
        void Remove(SchedulerItemViewModel item);
        void Filter(string searchText);
    }

    public interface IReminderGroup : ISchedulerGroup
    {
        List<TimerReminderItem> CalculateTimerReminderItems();
    }
}
