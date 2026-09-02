using Reminder.Launch;
using Reminder.Models;
using Reminder.UI;
using Reminder.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;

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

        // Это событие вынуждено используется вместо OnLoad.
        // На OnLoad в WPF окно уже видно на экране и автоматическое сворачивание приведет к мельканию.
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
            if (string.IsNullOrEmpty(ReminderModel.Instance.Settings.InstalledFolder))
            {
                ReminderModel.Instance.Settings.InstalledFolder = System.Windows.Forms.Application.StartupPath;
                ReminderModel.Instance.Save();
                // фича для удобства: на первом запуске все группы пусты
                // одарим пользователя кастомизацией настроек
                DataContext.SettingsCommand.Execute(null);
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (ReminderModel.Settings.MinimizeToTray && WindowState == WindowState.Minimized)
                HideAndNotifyMinimized();
            base.OnStateChanged(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (ReminderModel.Settings.MinimizeToTray && LaunchChecker.IsRunning == false)
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

        private void ShiftControl_OnIsShiftedChangedCompleted(object sender, EventArgs e)
        {
            ShiftControl shiftControl = (ShiftControl)sender;
            DataContext.OnIsShiftedChangedCompleted(shiftControl.IsShifted);
        }
    }
}
