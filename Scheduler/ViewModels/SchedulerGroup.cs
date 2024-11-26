using System;
using System.Collections;
using System.Collections.Generic;

namespace Scheduler.ViewModels
{
    public abstract class SchedulerGroup : BaseViewModel
    {
        public abstract string Name { get; }
        public abstract string ImagePath { get; }
        public abstract IEnumerable Items { get; }

        public abstract void Initialize();
        public abstract void Filter(string searchText);

        public void InitializeComplete()
        {
            OnPropertyChanged(nameof(Items));
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public abstract class ReminderGroup : SchedulerGroup
    {
        /// <summary>
        /// Получить все непросроченные напоминания.
        /// </summary>
        public abstract List<TimerReminderItem> GetTimerReminderItems();
    }
}
