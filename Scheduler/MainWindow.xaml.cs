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
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (SchedulerModel.IsLoaded == false) return;
            LaunchChecker.RegisterClientProcess(this);
            DataContext.InitializeAsync(Dispatcher);
            if (SchedulerModel.Instance.Settings.IsFirstLaunch)
            {
                SchedulerModel.Instance.Settings.IsFirstLaunch = false;
                SchedulerModel.Instance.Save();
            }
            else if (SchedulerModel.Instance.Settings.MinimizeToTray)
            {
                WindowState = WindowState.Minimized;
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
           if(SchedulerModel.Settings.MinimizeToTray && WindowState == WindowState.Minimized)
               Hide();
           base.OnStateChanged(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (SchedulerModel.Settings.MinimizeToTray)
            {
                e.Cancel = true;
                Hide();
            }
            base.OnClosing(e);
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
