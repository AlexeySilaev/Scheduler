using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Scheduler.UI
{
    /// <summary>
    /// Редактор времени.
    /// </summary>
    public class TimeTextBox : TextBox
    {
        private const int
            MiNUTES_BEGIN = 3,
            SEPARATOR_POSITION = 2,
            WORKING_SELECTION_LENGTH = 1;

        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(TimeSpan), typeof(TimeTextBox),
                new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTimeChangedCallBack));

        public TimeSpan Time
        {
            get { return (TimeSpan)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        public TimeTextBox()
        {
            ContextMenu = null;
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, OnPasteCommand));
        }

        private void OnPasteCommand(object sender, ExecutedRoutedEventArgs e)
        {
            // запретить вставку из буфера
        }

        protected override void OnSelectionChanged(RoutedEventArgs e)
        {
            if (SelectionStart == Text.Length)
            {
                SelectionStart = Text.Length - WORKING_SELECTION_LENGTH;
            }
            else if (SelectionStart == SEPARATOR_POSITION)
            {
                SelectionStart = MiNUTES_BEGIN;
            }

            if (SelectionLength != WORKING_SELECTION_LENGTH)
                SelectionLength = WORKING_SELECTION_LENGTH;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    if (SelectionStart == MiNUTES_BEGIN)
                    {
                        SelectionStart = WORKING_SELECTION_LENGTH;
                        e.Handled = true;
                    }
                    else if (SelectionStart > 0)
                        SelectionStart--;
                    else
                        e.Handled = true;

                    SelectionLength = WORKING_SELECTION_LENGTH;
                    break;
                case Key.Right:
                    if (SelectionStart == 4)
                        e.Handled = true;
                    if (SelectionStart == 1)
                    {
                        SelectionStart = MiNUTES_BEGIN;
                        e.Handled = true;
                    }
                    SelectionLength = WORKING_SELECTION_LENGTH;
                    break;
                case Key.Delete:
                case Key.Back:
                    e.Handled = true;
                    break;
            }
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            e.Handled = true;
            if (e.Text?.Length != WORKING_SELECTION_LENGTH)
                return;
            char digit = e.Text[0];
            if (char.IsDigit(digit) == false)
                return;
            if (SelectionStart < SEPARATOR_POSITION)
            {
                // редактируем часы
                StringBuilder hours = new StringBuilder(Text.Substring(0, SEPARATOR_POSITION));
                hours[SelectionStart] = digit;
                if (int.Parse(hours.ToString()) < 24)
                    e.Handled = false; // ввод корректен, разрешаем замену символа
            }
            else if (SelectionStart > SEPARATOR_POSITION)
            {
                // редактируем минуты
                StringBuilder minutes = new StringBuilder(Text.Substring(MiNUTES_BEGIN));
                minutes[SelectionStart - MiNUTES_BEGIN] = digit;
                if (int.Parse(minutes.ToString()) < 59)
                    e.Handled = false; // ввод корректен, разрешаем замену символа
            }
        }

        private static void OnTimeChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TimeTextBox timeBox = sender as TimeTextBox;
            if (timeBox != null)
                timeBox.Text = $"{timeBox.Time.Hours:D2}:{timeBox.Time.Minutes:D2}";
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            Time = TimeSpan.Parse(Text);
        }
    }
}
