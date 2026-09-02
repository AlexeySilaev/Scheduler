using System.Windows;
using System.Windows.Controls;
using Reminder.ViewModels;

namespace Reminder.TemplateSelectors
{
    public class ReminderItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AlarmTemplate { get; set; }
        public DataTemplate EventTemplate { get; set; }
        public DataTemplate NoteTemplate { get; set; }
        public DataTemplate ProjectTemplate { get; set; }

        public override DataTemplate SelectTemplate(object value, DependencyObject container)
        {
            if (value == null)
                return null;
            SchedulerItemViewModel item = (SchedulerItemViewModel)value;
            
            switch (item.Type)
            {
                case ReminderItemType.Alarm:
                    return AlarmTemplate;
                case ReminderItemType.Event:
                    return EventTemplate;
                case ReminderItemType.Note:
                    return NoteTemplate;
                case ReminderItemType.Project:
                    return ProjectTemplate;
            }
            return null;
        }
    }
}
