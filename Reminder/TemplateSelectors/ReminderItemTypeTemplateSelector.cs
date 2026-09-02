using Reminder.ViewModels;

namespace Reminder.TemplateSelectors
{
    public class ReminderItemTypeTemplateSelector : ItemTypeTemplateSelector
    {
        protected override ReminderItemType ExtractItemType(object value)
        {
            SchedulerItemViewModel item = (SchedulerItemViewModel)value;
            return item.Type;
        }
    }
}
