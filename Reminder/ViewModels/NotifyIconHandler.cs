using System.Windows;
using System.Windows.Input;
using System;
using Reminder.Launch;
using Reminder.Models;
using Reminder.UI;

namespace Reminder.ViewModels
{
    public class NotifyIconHandler : BaseViewModel
    {
        private FrameworkNotifyIcon.NotifyRequestRecord _notifyRequest;
        private bool _isIconVisible;
        private bool _aboutShown;

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
        public ICommand NotifyIconAboutCommand { get; }
        public ICommand NotifyIconExitCommand { get; }

        public NotifyIconHandler()
        {
            IsIconVisible = ReminderModel.Instance.Settings.MinimizeToTray;
            NotifyCommand = new Command(() => Notify("Приложение продолжает работать"));
            NotifyIconOpenCommand = new Command(OnOpen);
            NotifyIconAboutCommand = new Command(OnAbout);
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
            IReminderClientProcess win = Application.Current.MainWindow as IReminderClientProcess;
            win?.ShowMainWindow();
        }

        private void OnAbout()
        {
            if (_aboutShown) return;
            AboutWindow about = new AboutWindow();
            about.Closed += (o, e) => _aboutShown = false;
            about.Show();
            _aboutShown = true;
        }

        private void OnExit()
        {
            Application.Current.Shutdown();
        }

    }
}
