using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Scheduler.Extensions;
using Scheduler.Models;

namespace Scheduler.ViewModels
{
    public class Notes : SchedulerGroup
    {
        private List<NoteItemViewModel> _allItems;
        private ObservableCollection<NoteItemViewModel> _items;

        public override string Name
        {
            get { return "Заметки"; }
        }

        public override string ImagePath
        {
            get { return "/Resources/notepad32x32.png"; }
        }

        public override IEnumerable Items
        {
            get { return _items; }
        }

        public override void Initialize()
        {
            _allItems = SchedulerModel.Instance.Notes.Select(n => new NoteItemViewModel(n))
                .OrderByDescending(n => n.CreationDate)
                .ToList();
            _items = new ObservableCollection<NoteItemViewModel>(_allItems);
        }

        public override void Filter(string searchText)
        {
            var filtered = _allItems.Where(item => item.Text.ToLower().Contains(searchText.ToLower()));
            _items = new ObservableCollection<NoteItemViewModel>(filtered);
            OnPropertyChanged(nameof(Items));
        }

        public override void Remove(SchedulerItemViewModel item)
        {
            Debug.Assert(item.Type == SchedulerItemType.Note);
            NoteItemViewModel noteItem = (NoteItemViewModel)item;
            _allItems.Remove(noteItem);
            _items.Remove(noteItem);
        }

        public override SchedulerItemViewModel CreateNewItem()
        {
            return new NoteItemViewModel(new NoteItem { Text = SchedulerItemType.Note.GetDescription() });
        }
    }
}
