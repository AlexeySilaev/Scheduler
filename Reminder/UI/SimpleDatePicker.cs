using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Reminder.UI
{
    // здесь убрана кнопка, чтобы дата
    // поместилась в окне напоминаний
    internal class SimpleDatePicker : DatePicker
    {
        Button _button;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _button = (Button)Template.FindName("PART_Button", this);
            _button.Visibility = Visibility.Collapsed;

            TextBox textBox = (TextBox)Template.FindName("PART_TextBox", this);
            textBox.PreviewMouseLeftButtonUp += TextBox_MouseLeftButtonUp;
            textBox.PreviewKeyDown += TextBox_KeyDown;
        }

        private void TextBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                SelectedDate = null;
        }
    }
}
