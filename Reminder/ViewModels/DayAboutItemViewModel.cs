using Reminder.Extensions;
using Reminder.Resources.ViewModels;

namespace Reminder.ViewModels
{
    /// <summary>
    /// Уведомление за сутки: событие или проект.
    /// </summary>
    public class DayAboutItemViewModel : BaseViewModel
    {
        private bool _isExpanded;

        public ReminderItemViewModel Source { get; }
        public string Title { get; }
        public string ItemInfo { get; }
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (IsExpanded == value) return;
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }

        public DayAboutItemViewModel(EventItemViewModel source)
            : this(source, source.Text)
        {
        }

        public DayAboutItemViewModel(ProjectItemViewModel source)
            : this(source, source.Name)
        {
        }

        private DayAboutItemViewModel(ReminderItemViewModel source, string title)
        {
            Source = source;
            Title = title;
            ItemInfo = $"{source.Type.GetDescription()} - {source.PeriodicityDescription}";
        }
    }
}
