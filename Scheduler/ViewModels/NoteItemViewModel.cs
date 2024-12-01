using Scheduler.Models;
using System;

namespace Scheduler.ViewModels
{
    internal class NoteItemViewModel : SchedulerItemViewModel
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

        public override SchedulerItemType Type
        {
            get { return SchedulerItemType.Note; }
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
            CreationDate = DateTime.Now;
            NoteItem item = (NoteItem)_item;
            item.CreationDate = CreationDate;
        }

        public override void Save()
        {
            SchedulerModel.Instance.Notes.Add((NoteItem)_item);
        }
    }
}
