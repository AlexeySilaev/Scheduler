using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Input;

namespace Reminder.UI
{
    public class FrameworkNotifyIcon : FrameworkElement, IDisposable
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(FrameworkNotifyIcon), new PropertyMetadata(
                (d, e) =>
                {
                    var notifyIcon = ((FrameworkNotifyIcon)d)._notifyIcon;
                    if (notifyIcon == null)
                        return;
                    notifyIcon.Text = (string)e.NewValue;
                }));

        public static readonly DependencyProperty IconVisibleProperty =
            DependencyProperty.Register("IconVisible", typeof(bool), typeof(FrameworkNotifyIcon), new PropertyMetadata(
                true,
                (d, e) =>
                {
                    var notifyIcon = ((FrameworkNotifyIcon)d)._notifyIcon;
                    if (notifyIcon == null)
                        return;
                    notifyIcon.Visible = (bool)e.NewValue;
                }));

        private static readonly DependencyProperty NotifyRequestProperty =
            DependencyProperty.Register("NotifyRequest", typeof(NotifyRequestRecord), typeof(FrameworkNotifyIcon),
                new PropertyMetadata(
                    (d, e) =>
                    {
                        var r = (NotifyRequestRecord)e.NewValue;
                        ((FrameworkNotifyIcon)d)._notifyIcon?.ShowBalloonTip(r.Duration, r.Title, r.Text, r.Icon);
                    }));


        public static readonly DependencyProperty OpenCommandProperty = DependencyProperty
            .Register("OpenCommand", typeof(ICommand), typeof(FrameworkNotifyIcon), new FrameworkPropertyMetadata(null, OnOpenCommandChanged));
        public static readonly DependencyProperty AboutCommandProperty = DependencyProperty
            .Register("AboutCommand", typeof(ICommand), typeof(FrameworkNotifyIcon), new FrameworkPropertyMetadata(null, OnAboutCommandChanged));
        public static readonly DependencyProperty ExitCommandProperty = DependencyProperty
            .Register("ExitCommand", typeof(ICommand), typeof(FrameworkNotifyIcon), new FrameworkPropertyMetadata(null, OnExitCommandChanged));

        private static void OnOpenCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            FrameworkNotifyIcon owner = (FrameworkNotifyIcon)d;
            owner.SetCommand(OPEN_MENU_INDEX, (ICommand)args.NewValue);
        }
        private static void OnAboutCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            FrameworkNotifyIcon owner = (FrameworkNotifyIcon)d;
            owner.SetCommand(ABOUT_MENU_INDEX, (ICommand)args.NewValue);
        }
        private static void OnExitCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            FrameworkNotifyIcon owner = (FrameworkNotifyIcon)d;
            owner.SetCommand(EXIT_MENU_INDEX, (ICommand)args.NewValue);
        }

        private void SetCommand(int index, ICommand command)
        {
            if (DesignerProperties.GetIsInDesignMode(this) == false)
                _stripMenuCommands[index].Command = command;
        }

        private const int
            OPEN_MENU_INDEX = 0,
            ABOUT_MENU_INDEX = 1,
            EXIT_MENU_INDEX = 2;
        private readonly NotifyIcon _notifyIcon;
        private StripMenuCommand[] _stripMenuCommands;

        [Bindable(true)]
        [Category("Action")]
        public ICommand OpenCommand
        {
            get { return (ICommand)GetValue(OpenCommandProperty); }
            set { SetValue(OpenCommandProperty, value); }
        }

        [Bindable(true)]
        [Category("Action")]
        public ICommand AboutCommand
        {
            get { return (ICommand)GetValue(AboutCommandProperty); }
            set { SetValue(AboutCommandProperty, value); }
        }

        [Bindable(true)]
        [Category("Action")]
        public ICommand ExitCommand
        {
            get { return (ICommand)GetValue(ExitCommandProperty); }
            set { SetValue(ExitCommandProperty, value); }
        }

        public FrameworkNotifyIcon()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;
            _notifyIcon = new NotifyIcon
            {
                Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location),
                Visible = true,
                ContextMenuStrip = CreateContextMenu()
            };
            _stripMenuCommands[OPEN_MENU_INDEX].AddDoubleClickHandler(_notifyIcon);
            System.Windows.Application.Current.Exit += (obj, args) => { _notifyIcon.Dispose(); };
        }

        public string IconVisible
        {
            get => (string)GetValue(IconVisibleProperty);
            set => SetValue(IconVisibleProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public NotifyRequestRecord NotifyRequest
        {
            get => (NotifyRequestRecord)GetValue(NotifyRequestProperty);
            set => SetValue(NotifyRequestProperty, value);
        }

        public void Dispose()
        {
            _notifyIcon?.Dispose();
        }

        private ContextMenuStrip CreateContextMenu()
        {
            ToolStripMenuItem openItem = new ToolStripMenuItem("Открыть");
            ToolStripMenuItem about = new ToolStripMenuItem("О программе");
            ToolStripMenuItem exitItem = new ToolStripMenuItem("Выход");
            _stripMenuCommands = new[] { new StripMenuCommand(openItem),
                new StripMenuCommand(about), new StripMenuCommand(exitItem) };
            ContextMenuStrip result = new ContextMenuStrip
            {
                Items = { openItem, about, new ToolStripSeparator(), exitItem }
            };
            return result;
        }

        public class NotifyRequestRecord
        {
            public string Title { get; set; } = "";
            public string Text { get; set; } = "";
            public int Duration { get; set; } = 1000;
            public ToolTipIcon Icon { get; set; } = ToolTipIcon.Info;
        }

        private class StripMenuCommand
        {
            private readonly ToolStripMenuItem _menuItem;
            private ICommand _command;


            public ICommand Command
            {
                get { return _command; }
                set
                {
                    if (Command == value) return;
                    if (Command != null)
                        Command.CanExecuteChanged -= Command_CanExecuteChanged;
                    _command = value;
                    if (Command != null)
                    {
                        Command.CanExecuteChanged += Command_CanExecuteChanged;
                        _menuItem.Enabled = Command.CanExecute(null);
                    }
                    else
                        _menuItem.Enabled = false;
                }
            }

            public StripMenuCommand(ToolStripMenuItem menuItem)
            { 
                _menuItem = menuItem;
                _menuItem.Click += MenuItem_Click;
            }

            public void AddDoubleClickHandler(NotifyIcon notifyIcon)
            {
                notifyIcon.DoubleClick += MenuItem_Click;
            }

            private void MenuItem_Click(object sender, EventArgs e)
            {
                Command?.Execute(null);
            }

            private void Command_CanExecuteChanged(object sender, EventArgs e)
            {
                _menuItem.Enabled = _command.CanExecute(null);
            }
        }
    }
}
