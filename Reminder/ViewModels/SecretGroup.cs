using Reminder.Models;
using Reminder.UI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Reminder.ViewModels
{
    public class SecretGroup : SchedulerGroup<SecretItemViewModel>, ISupportHostContent
    {
        public override string Name
        {
            get { return "Секреты"; }
        }

        public override string ImagePath
        {
            get { return "/Resources/secret32x32.png"; }
        }

        public override string Description
        {
            get { return "Секрет - это зашифрованная строка или файл, представляющие конфиденциальные данные: пароли, ссылки, заклинания и т.п. Сортируются по частоте просмотров."; }
        }

        public override bool IsVisible
        {
            get { return ReminderModel.Instance.Settings.UnlockSecretsKey != Guid.Empty; }
        }

        public override void Initialize()
        {
            if (ReminderModel.Instance.Secrets.Any())
            {
                // зачем давать разрастаться числу просмотров? Нормализуем относительно минимального
                int minViewCount = ReminderModel.Instance.Secrets.Min(x => x.ViewCounter);
                ReminderModel.Instance.Secrets.ForEach(s => s.ViewCounter -= minViewCount - 1);
            }
            _allItems = ReminderModel.Instance.Secrets.Select(n => new SecretItemViewModel(n))
                .OrderByDescending(n => n.ViewCounter)
                .ToList();
            _items = new ObservableCollection<SecretItemViewModel>(_allItems);
        }

        public override void Save()
        {
            ReminderModel.Instance.Secrets.Clear();
            base.Save();
        }

        public override void Remove(SchedulerItemViewModel item)
        {
            Debug.Assert(item.Type == ReminderItemType.Secret);
            base.Remove(item);
        }

        public override SchedulerItemViewModel CreateNewItem()
        {
            return new SecretItemViewModel(new SecretItem { Text = string.Empty });
        }

        public override void CheckAddNew(SchedulerItemViewModel item)
        {
            Debug.Assert(item.Type == ReminderItemType.Secret);
            base.CheckAddNew(item);
        }

        // сортировка по убыванию количества просмотров
        protected override int Compare(SecretItemViewModel x, SecretItemViewModel y)
        {
            return y.ViewCounter.CompareTo(x.ViewCounter);
        }

        #region ISupportHostContent
        bool _isUnlocked;

        bool ISupportHostContent.IsHostContentAllowed 
        {
            get { return _isUnlocked == false; }
        }

        HostContent ISupportHostContent.GetHostContent()
        {
            return new HostContent(new UnlockSecretsControl(), new UnlockSecretsViewModel());
        }

        void ISupportHostContent.SetHostContentAllowed(bool value)
        { 
            _isUnlocked = value;
        }

        #endregion ISupportHostContent
    }
}
