using Reminder.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Reminder.ViewModels
{
    public class ProjectGroup : SchedulerGroup<ProjectItemViewModel>
    {
        public override string Name
        {
            get { return "Проекты"; }
        }

        public override string ImagePath
        {
            get { return "/Resources/project32x32.png"; }
        }

        public override string Description
        {
            get { return "Проект - план из группы пунктов, в которых можно перечислить шаги реализации, компоненты структуры, достигаемые цели и т.п. Сортируются по дате их начала."; }
        }

        public override void Initialize()
        {
            _allItems = ReminderModel.Instance.Projects.Select(n => new ProjectItemViewModel(n))
                .OrderByDescending(n => n.StartDate)
                .ToList();
            _items = new ObservableCollection<ProjectItemViewModel>(_allItems);
        }

        public override void Save()
        {
            ReminderModel.Instance.Projects.Clear();
            base.Save();
        }

        public override void Remove(SchedulerItemViewModel item)
        {
            Debug.Assert(item.Type == ReminderItemType.Project);
            base.Remove(item);
        }

        public override SchedulerItemViewModel CreateNewItem()
        {
            return new ProjectItemViewModel(new ProjectItem { Text = string.Empty });
        }

        public override void CheckAddNew(SchedulerItemViewModel item)
        {
            Debug.Assert(item.Type == ReminderItemType.Project);
            base.CheckAddNew(item);
        }

        protected override bool FilterPredicate(string searchText, ProjectItemViewModel item)
        {
            return base.FilterPredicate(searchText, item) || item.Name.ToLower().Contains(searchText.ToLower());
        }

        // сортировка по убыванию даты создания
        protected override int Compare(ProjectItemViewModel x, ProjectItemViewModel y)
        {
            return y.StartDate.CompareTo(x.StartDate);
        }
    }
}
