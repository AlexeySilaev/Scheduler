using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Reminder.Extensions;
using Reminder.Models;

namespace Reminder.ViewModels
{
    public abstract class SchedulerGroup<T> : BaseViewModel,
        ISchedulerGroup,
        IComparer<T>
        where T : SchedulerItemViewModel
    {
        protected List<T> _allItems;
        protected ObservableCollection<T> _items;

        public ICollection Items
        {
            get { return _items; }
        }

        public abstract string Name { get; }
        public abstract string ImagePath { get; }
        public abstract string Description { get; }

        public abstract void Initialize();
        public abstract SchedulerItemViewModel CreateNewItem();


        public void InitializeComplete()
        {
            OnPropertyChanged(nameof(Items));
        }

        public virtual void CheckAddNew(SchedulerItemViewModel item)
        {
            if (_allItems.Contains(item) == false)
                _allItems.Add((T)item);
            _allItems.Sort(this);
        }

        public virtual void Remove(SchedulerItemViewModel item)
        {
            T schedulerItem = (T)item;
            _allItems.Remove(schedulerItem);
            _items.Remove(schedulerItem);
        }

        public virtual void Save()
        {
            _allItems.ForEach(item => item.Save());
        }

        public void Filter(string searchText)
        {
            var filtered = _allItems.Where(item => FilterPredicate(searchText, item));
            _items = new ObservableCollection<T>(filtered);
            OnPropertyChanged(nameof(Items));
        }

        protected virtual bool FilterPredicate(string searchText, T item)
        {
            return item.Text.ToLower().Contains(searchText.ToLower());
        }

        int IComparer<T>.Compare(T x, T y)
        {
            return Compare(x, y);
        }

        protected abstract int Compare(T x, T y);

        public override string ToString()
        {
            return Name;
        }
    }

    public abstract class ReminderGroup<T> : SchedulerGroup<T> where T : ReminderItemViewModel
    {
        protected abstract IEnumerable<T> FilterExpiredItems();
        protected abstract IOrderedEnumerable<T> OrderItems(IEnumerable<T> items);

        public override void Initialize()
        {
            IEnumerable<T> filtered = FilterExpiredItems();
            _allItems = OrderItems(filtered).ToList();
            _items = new ObservableCollection<T>(_allItems);
        }

        protected IEnumerable<T> FilterExpiredItems<TReminderItem>(List<TReminderItem> items, Func<TReminderItem, T> creator)
            where TReminderItem  : ReminderItem
        {
            IEnumerable<T> all = items.Select(e =>
            {
                T viewModel = creator(e);
                new TimerReminderItem(viewModel).CheckExpired(DateTime.Now);
                return viewModel;
            }).ToArray();
            if (ReminderModel.Instance.Settings.DeleteExpiredRemindersOnStart)
            {
                IEnumerable<T> expired = all.Where(item => item.State == ReminderState.Expired).ToArray();
                all = all.Except(expired);
                if (expired.Any())
                {
                    // в файле модели просроченные элементы, почистим
                    items.Clear();
                    all.ForEach(item => item.Save());
                    ReminderModel.Instance.Save();
                }
            }
            return all;
        }

        /// <summary>
        /// Получить все незавершенные напоминания.
        /// </summary>
        public List<TimerReminderItem> GetTimerReminderItems()
        {
            return _allItems
                .Where(a => a.State == ReminderState.Active || a.State == ReminderState.Expired)
                .Select(a => new TimerReminderItem(a))
                .ToList();
        }
    }
}
