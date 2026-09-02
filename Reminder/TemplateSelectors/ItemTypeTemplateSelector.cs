using Reminder.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Reminder.TemplateSelectors
{
    public abstract class ItemTypeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AlarmTemplate { get; set; }
        public DataTemplate EventTemplate { get; set; }
        public DataTemplate NoteTemplate { get; set; }

        ReminderItemType last = ReminderItemType.Event;

        public override DataTemplate SelectTemplate(object value, DependencyObject container)
        {
            ReminderItemType itemType = last;
            if (value!= null)
                itemType = ExtractItemType(value);
            last = itemType;

            switch (itemType)
            {
                case ReminderItemType.Alarm:
                    return AlarmTemplate;
                case ReminderItemType.Event:
                    return EventTemplate;
                case ReminderItemType.Note:
                    return NoteTemplate;
            }
            return null;
        }

        protected abstract ReminderItemType ExtractItemType(object value);
    }
}
