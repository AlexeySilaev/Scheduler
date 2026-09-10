using Reminder.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reminder.ViewModels
{
    public class SecretGroup : SchedulerGroup<SecretItemViewModel>
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

        public override void Initialize()
        {
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

    }
}
