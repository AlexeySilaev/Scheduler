using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using Scheduler.Models;

namespace Scheduler.ViewModels
{
    public class Notes : SchedulerGroup
    {
        private NoteItemViewModel[] _allItems;
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
                .ToArray();
            _items = new ObservableCollection<NoteItemViewModel>(_allItems);
        }

        public override void Filter(string searchText)
        {
            var filtered = _allItems.Where(item => item.Text.ToLower().Contains(searchText.ToLower()));
            _items = new ObservableCollection<NoteItemViewModel>(filtered);
            OnPropertyChanged(nameof(Items));
        }

    }
}
