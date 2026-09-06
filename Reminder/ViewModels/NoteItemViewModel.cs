using System;
using Reminder.Models;

namespace Reminder.ViewModels
{
    public class NoteItemViewModel : SchedulerItemViewModel
    {
        private DateTime _creationDate;

        public DateTime CreationDate
        {
            get { return _creationDate; }
            set
            {
                if (CreationDate == value) return;
                _creationDate = value;
                OnPropertyChanged(nameof(CreationDate));
            }
        }

        public override ReminderItemType Type
        {
            get { return ReminderItemType.Note; }
        }

        public NoteItemViewModel(NoteItem item) : base(item)
        {
            _creationDate = item.CreationDate;
        }

        protected override bool CanExecuteEdit()
        {
            return true;
        }

        public override void EndEdit()
        {
            base.EndEdit();
            CreationDate = TimeProvider.Now;
            NoteItem item = (NoteItem)_item;
            item.CreationDate = CreationDate;
        }

        public override void Save()
        {
            ReminderModel.Instance.Notes.Add((NoteItem)_item);
        }
    }
}
