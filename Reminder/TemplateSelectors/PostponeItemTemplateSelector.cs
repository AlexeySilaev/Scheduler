using Reminder.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Reminder.TemplateSelectors
{
    public class PostponeItemTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object value, DependencyObject container)
        {
            if (value is OtherItem)
                return (DataTemplate)Application.Current.FindResource("CustomizePostponeItemTemplate");
            return null;
        }
    }
}
