using System;
using System.ComponentModel;
using System.Windows;
using Reminder.Launch;
using Reminder.Models;
using Reminder.ViewModels;

namespace Reminder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IReminderClientProcess
    {
        private bool _firstResize;

        private new MainViewModel DataContext
        {
            get { return (MainViewModel)base.DataContext; }
        }

        private ReminderModel ReminderModel
        {
            get { return ReminderModel.Instance; }
        }

        public MainWindow()
        {
            _firstResize = true;
            InitializeComponent();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (ReminderModel.IsLoaded == false || _firstResize == false) return;
            _firstResize = false;
            LaunchChecker.RegisterClientProcess(this);
            DataContext.InitializeAsync(Dispatcher);

            if (ReminderModel.Instance.Settings.MinimizeToTray
                && ReminderModel.Instance.Settings.IsFirstMinimize == false)
                WindowState = WindowState.Minimized;
        }

        protected override void OnStateChanged(EventArgs e)
       {
            if (ReminderModel.Settings.MinimizeToTray && WindowState == WindowState.Minimized)
                HideAndNotifyMinimized();
            base.OnStateChanged(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (ReminderModel.Settings.MinimizeToTray)
            {
                e.Cancel = true;
                HideAndNotifyMinimized();
            }
            base.OnClosing(e);
        }

        private void HideAndNotifyMinimized()
        {
            Hide();
            if (ReminderModel.Instance.Settings.IsFirstMinimize)
            {
                ReminderModel.Instance.Settings.IsFirstMinimize = false;
                ReminderModel.Instance.Save();
                DataContext.NotifyIconHandler.NotifyCommand.Execute(null);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            DataContext.OnClosed();
            base.OnClosed(e);
        }

        void IReminderClientProcess.ShowMainWindow()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Show();
                if (WindowState == WindowState.Minimized)
                    WindowState = WindowState.Normal;
                Activate();
                Focus();
            }));
        }
    }
}
