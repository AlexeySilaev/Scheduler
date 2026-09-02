using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Reminder.Models;

namespace Reminder.ViewModels
{
    public class NoteGroup : SchedulerGroup<NoteItemViewModel>
    {
        public override string Name
        {
            get { return "Заметки"; }
        }

        public override string ImagePath
        {
            get { return "/Resources/notepad32x32.png"; }
        }

        public override string Description
        {
            get { return "Заметка - любой текст длиной до 1000 символов, который удобно оперативно сохранить: гиперссылка, телефон, адрес, рецепт и т.п. Сортируются по дате изменения."; }
        }

        public override void Initialize()
        {
            _allItems = ReminderModel.Instance.Notes.Select(n => new NoteItemViewModel(n))
                .OrderByDescending(n => n.CreationDate)
                .ToList();
            _items = new ObservableCollection<NoteItemViewModel>(_allItems);
        }

        public override void Save()
        {
            ReminderModel.Instance.Notes.Clear();
            base.Save();
        }

        public override void Remove(SchedulerItemViewModel item)
        {
            Debug.Assert(item.Type == ReminderItemType.Note);
            base.Remove(item);
        }

        public override SchedulerItemViewModel CreateNewItem()
        {
            return new NoteItemViewModel(new NoteItem { Text = string.Empty });
        }

        public override void CheckAddNew(SchedulerItemViewModel item)
        {
            Debug.Assert(item.Type == ReminderItemType.Note);
            base.CheckAddNew(item);
        }

        // сортировка по убыванию даты создания
        protected override int Compare(NoteItemViewModel x, NoteItemViewModel y)
        {
            return y.CreationDate.CompareTo(x.CreationDate);
        }
    }
}
