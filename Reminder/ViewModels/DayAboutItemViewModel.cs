namespace Reminder.ViewModels
{
    /// <summary>
    /// Уведомление за сутки: событие или проект.
    /// </summary>
    public class DayAboutItemViewModel
    {
        public ReminderItemViewModel Source { get; }
        public string Title { get; }
        public string PeriodicityDescription { get; }

        public DayAboutItemViewModel(EventItemViewModel source)
        {
            Source = source;
            Title = source.Text;
            PeriodicityDescription = source.PeriodicityDescription;
        }

        public DayAboutItemViewModel(ProjectItemViewModel source)
        {
            Source = source;
            Title = source.Name;
            PeriodicityDescription = source.PeriodicityDescription;
        }
    }
}
