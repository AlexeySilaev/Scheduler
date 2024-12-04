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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (SchedulerModel.Instance.IsLoaded == false) return;
            LaunchChecker.RegisterClientProcess(this);
            DataContext.InitializeAsync(Dispatcher);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (SchedulerModel.Instance.IsLoaded == false) return;
            base.OnClosing(e);
            DataContext.OnClosing();
        }

        void ISchedulerClientProcess.ShowMainWindow()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (WindowState == WindowState.Minimized)
                    WindowState = WindowState.Normal;
                Activate();
                Focus();
            }));
        }
    }
}
