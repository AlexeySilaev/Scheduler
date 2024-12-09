using System;
using Scheduler.Launch;
using Scheduler.ViewModels;
using System.ComponentModel;
using System.Windows;
using Scheduler.Models;

namespace Scheduler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ISchedulerClientProcess
    {
        private bool _firstResize;

        private new MainViewModel DataContext
        {
            get { return (MainViewModel)base.DataContext; }
        }

        private SchedulerModel SchedulerModel
        {
            get { return SchedulerModel.Instance; }
        }

        public MainWindow()
        {
            _firstResize = true;
            InitializeComponent();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (SchedulerModel.IsLoaded == false || _firstResize == false) return;
            _firstResize = false;
            LaunchChecker.RegisterClientProcess(this);
            DataContext.InitializeAsync(Dispatcher);

            if (SchedulerModel.Instance.Settings.MinimizeToTray
                && SchedulerModel.Instance.Settings.IsFirstMinimize == false)
                WindowState = WindowState.Minimized;
        }

        protected override void OnStateChanged(EventArgs e)
       {
            if (SchedulerModel.Settings.MinimizeToTray && WindowState == WindowState.Minimized)
                HideAndNotifyMinimized();
            base.OnStateChanged(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (SchedulerModel.Settings.MinimizeToTray)
            {
                e.Cancel = true;
                HideAndNotifyMinimized();
            }
            base.OnClosing(e);
        }

        private void HideAndNotifyMinimized()
        {
            Hide();
            if (SchedulerModel.Instance.Settings.IsFirstMinimize)
            {
                SchedulerModel.Instance.Settings.IsFirstMinimize = false;
                SchedulerModel.Instance.Save();
                DataContext.NotifyIconHandler.NotifyCommand.Execute(null);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            DataContext.OnClosed();
            base.OnClosed(e);
        }

        void ISchedulerClientProcess.ShowMainWindow()
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
