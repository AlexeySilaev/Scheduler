using System.Windows;
using System.Windows.Controls;
using Scheduler.ViewModels;

namespace Scheduler.TemplateSelectors
{
    public class SchedulerItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AlarmTemplate { get; set; }
        public DataTemplate EventTemplate { get; set; }
        public DataTemplate NoteTemplate { get; set; }

        public override DataTemplate SelectTemplate(object value, DependencyObject container)
        {
            SchedulerItemViewModel item = (SchedulerItemViewModel)value;
            switch (item.Type)
            {
                case SchedulerItemType.Alarm:
                    return AlarmTemplate;
                case SchedulerItemType.Event:
                    return EventTemplate;
                case SchedulerItemType.Note:
                    return NoteTemplate;
            }
            return null;
        }
    }
}
