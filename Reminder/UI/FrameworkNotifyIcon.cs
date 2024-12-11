using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Windows;

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

        private static readonly RoutedEvent OpenSelectedEvent = EventManager.RegisterRoutedEvent("OpenSelected",
            RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(FrameworkNotifyIcon));

        private static readonly RoutedEvent AboutSelectedEvent = EventManager.RegisterRoutedEvent("AboutSelected",
            RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(FrameworkNotifyIcon));

        private static readonly RoutedEvent ExitSelectedEvent = EventManager.RegisterRoutedEvent("ExitSelected",
            RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(FrameworkNotifyIcon));

        private readonly NotifyIcon _notifyIcon;

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
            _notifyIcon.DoubleClick += OpenItemOnClick;
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

        public event RoutedEventHandler OpenSelected
        {
            add => AddHandler(OpenSelectedEvent, value);
            remove => RemoveHandler(OpenSelectedEvent, value);
        }

        public event RoutedEventHandler AboutSelected
        {
            add => AddHandler(AboutSelectedEvent, value);
            remove => RemoveHandler(AboutSelectedEvent, value);
        }

        public event RoutedEventHandler ExitSelected
        {
            add => AddHandler(ExitSelectedEvent, value);
            remove => RemoveHandler(ExitSelectedEvent, value);
        }

        private ContextMenuStrip CreateContextMenu()
        {
            ToolStripMenuItem openItem = new ToolStripMenuItem("Открыть");
            openItem.Click += OpenItemOnClick;
            ToolStripMenuItem about = new ToolStripMenuItem("О программе");
            about.Click += AboutItemOnClick;
            ToolStripMenuItem exitItem = new ToolStripMenuItem("Выход");
            exitItem.Click += ExitItemOnClick;
            ContextMenuStrip result = new ContextMenuStrip
            {
                Items = { openItem, about, new ToolStripSeparator(), exitItem }
            };
            return result;
        }

        private void OpenItemOnClick(object sender, EventArgs eventArgs)
        {
            var args = new RoutedEventArgs(OpenSelectedEvent);
            RaiseEvent(args);
        }

        private void AboutItemOnClick(object sender, EventArgs eventArgs)
        {
            var args = new RoutedEventArgs(AboutSelectedEvent);
            RaiseEvent(args);
        }

        private void ExitItemOnClick(object sender, EventArgs eventArgs)
        {
            var args = new RoutedEventArgs(ExitSelectedEvent);
            RaiseEvent(args);
        }

        public class NotifyRequestRecord
        {
            public string Title { get; set; } = "";
            public string Text { get; set; } = "";
            public int Duration { get; set; } = 1000;
            public ToolTipIcon Icon { get; set; } = ToolTipIcon.Info;
        }
    }
}
