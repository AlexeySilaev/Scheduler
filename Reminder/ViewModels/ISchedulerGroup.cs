using System.Collections;

namespace Reminder.ViewModels
{
    public interface ISchedulerGroup
    {
        string Name { get; }
        string ImagePath { get; }
        ICollection Items { get; }
        string Description { get; }

        void Initialize();
        void InitializeComplete();
        void CheckAddNew(SchedulerItemViewModel item);
        SchedulerItemViewModel CreateNewItem();
        void Save();
        void Remove(SchedulerItemViewModel item);
        void Filter(string searchText);
    }
}
