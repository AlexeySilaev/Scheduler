using System;
using System.Windows;
using System.Windows.Threading;
using Scheduler.Launch;
using Scheduler.Models;
using Scheduler.ViewModels;

namespace Scheduler
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (LaunchChecker.IsRunning())
            {
                Current.Shutdown();
                return;
            }

            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            SchedulerModel.Instance.Load();
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
                OnUnexpectedError("Необработанная ошибка", exception.Message);
            else
                OnUnexpectedError("Необработанная ошибка", e.ExceptionObject.ToString());
        }

        private static void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            OnUnexpectedError("Ошибка на фоновом потоке", e.Exception.Message);
            e.Handled = true;
        }

        static void OnUnexpectedError(string title, string message)
        {
            Current.Dispatcher.Invoke(() =>
            {
                MainViewModel viewModel = MainViewModel.FindInstance();
                viewModel.Error = $"{title}: {message}";
            });
        }
    }
}
