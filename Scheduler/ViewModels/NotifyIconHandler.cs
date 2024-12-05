using Scheduler.Launch;
using System.Windows;
using System.Windows.Input;
using Scheduler.UI;
using Scheduler.Models;

namespace Scheduler.ViewModels
{
    public class NotifyIconHandler : BaseViewModel
    {
        private FrameworkNotifyIcon.NotifyRequestRecord _notifyRequest;
        private bool _isIconVisible;

        public FrameworkNotifyIcon.NotifyRequestRecord NotifyRequest
        {
            get { return _notifyRequest; }
            set
            {
                if (NotifyRequest == value) return;
                _notifyRequest= value;
                OnPropertyChanged(nameof(NotifyRequest));
            }
        }

        public bool IsIconVisible
        {
            get { return _isIconVisible; }
            set
            {
                if (IsIconVisible == value) return;
                _isIconVisible = value;
                OnPropertyChanged(nameof(IsIconVisible));
            }
        }


        public ICommand NotifyCommand { get; }
        public ICommand NotifyIconOpenCommand { get; }
        public ICommand NotifyIconExitCommand { get; }

        public NotifyIconHandler()
        {
            IsIconVisible = SchedulerModel.Instance.Settings.MinimizeToTray;
            NotifyCommand = new Command(() => Notify("Приложение продолжает работать"));
            NotifyIconOpenCommand = new Command(OnOpen);
            NotifyIconExitCommand = new Command(OnExit);
        }

        private void Notify(string message)
        {
            NotifyRequest = new FrameworkNotifyIcon.NotifyRequestRecord
            {
                Title = "Минимизация в системный лоток",
                Text = message,
                Duration = 1000
            };
        }

        private void OnOpen()
        {
            ISchedulerClientProcess win = Application.Current.MainWindow as ISchedulerClientProcess;
            win?.ShowMainWindow();
        }

        private void OnExit()
        {
            Application.Current.Shutdown();
        }

    }
}
