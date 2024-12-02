using System;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Scheduler.Models;
using Scheduler.ViewModels;

namespace Scheduler.UI
{
    public class SettingsViewModel :  BaseViewModel
    {
        private readonly Settings _settings;
        private readonly Timer _showErrorTimer;
        private bool _isMediaPlaying;
        private bool _isErrorTestPlay;

        public DeleteSchedulerItemsMode DeleteSchedulerItemsMode
        {
            get { return _settings.DeleteSchedulerItemsMode; }
            set
            {
                if (DeleteSchedulerItemsMode == value) return;
                _settings.DeleteSchedulerItemsMode = value;
                OnPropertyChanged(nameof(DeleteSchedulerItemsMode));
            }
        }

        public NotificationsWindowPosition NotificationsWindowPosition
        {
            get { return _settings.NotificationsWindowPosition; }
            set
            {
                if (NotificationsWindowPosition == value) return;
                _settings.NotificationsWindowPosition = value;
                OnPropertyChanged(nameof(NotificationsWindowPosition));
            }
        }

        public RingtoneMode RingtoneMode
        {
            get { return _settings.RingtoneMode; }
            set
            {
                if (RingtoneMode == value) return;
                _settings.RingtoneMode = value;
                OnPropertyChanged(nameof(RingtoneMode));
            }
        }

        public string CustomSoundFilePath
        {
            get { return _settings.CustomSoundFilePath; }
            set
            {
                if (CustomSoundFilePath == value) return;
                _settings.CustomSoundFilePath = value;
                OnPropertyChanged(nameof(CustomSoundFilePath));
                TestPlayFileCommand.RaiseCanExecuteChanged();
            }
        }

        public bool LaunchWithWindows
        {
            get { return _settings.LaunchWithWindows; }
            set
            {
                if (LaunchWithWindows == value) return;
                _settings.LaunchWithWindows = value;
                OnPropertyChanged(nameof(LaunchWithWindows));
            }
        }

        public bool MinimizeInTrayOnStart
        {
            get { return _settings.MinimizeInTrayOnStart; }
            set
            {
                if (MinimizeInTrayOnStart == value) return;
                _settings.MinimizeInTrayOnStart = value;
                OnPropertyChanged(nameof(MinimizeInTrayOnStart));
            }
        }

        public bool IsErrorTestPlay
        {
            get { return _isErrorTestPlay; }
            set
            {
                if (IsErrorTestPlay == value) return;
                _isErrorTestPlay = value;
                OnPropertyChanged(nameof(IsErrorTestPlay));
            }
        }

        public MediaElement Media { get; set; }

        public ICommand SelectCustomFileCommand { get; }
        public Command TestPlayFileCommand { get; }

        public SettingsViewModel(Settings settings)
        {
            _settings = settings;
            _showErrorTimer = new Timer();
            _showErrorTimer.AutoReset = false;
            _showErrorTimer.Interval = 1500;
            _showErrorTimer.Elapsed += ShowErrorTimer_Elapsed; 
            SelectCustomFileCommand = new Command(OnSelectCustomFile);
            TestPlayFileCommand = new Command(OnTestPlayFile, CanExecuteTestPlayFile);
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            SchedulerModel.Instance.Save();
        }

        #region Commands

        private void OnSelectCustomFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            string initialFolder = string.Empty;
            if (File.Exists(CustomSoundFilePath))
                initialFolder = Path.GetDirectoryName(CustomSoundFilePath);
            else
            {
                initialFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                if (Directory.Exists(initialFolder) == false)
                    initialFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            openFileDialog.InitialDirectory = initialFolder;
            openFileDialog.Filter = "Звуковые файлы (mp3, wav)|*.mp3;*.wav";
            if (openFileDialog.ShowDialog() == true)
                CustomSoundFilePath = openFileDialog.FileName;
        }

        private bool CanExecuteTestPlayFile()
        {
            return File.Exists(CustomSoundFilePath) && _isMediaPlaying == false;
        }

        private void OnTestPlayFile()
        {
            if (Media == null) return;
            Media.Source = new Uri(CustomSoundFilePath);
            Media.MediaEnded += Media_MediaEnded;
            Media.MediaFailed += MediaOnMediaFailed;
            _isMediaPlaying = true;
            TestPlayFileCommand.RaiseCanExecuteChanged();
            Media.Play();
        }

        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            OnFinishTestPlaying();
        }

        private void MediaOnMediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            OnFinishTestPlaying();
            e.Handled = true;
            IsErrorTestPlay = true;
            _showErrorTimer.AutoReset = false;
            _showErrorTimer.Start();
        }

        private void ShowErrorTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsErrorTestPlay = false;
                CustomSoundFilePath = null;
            });
        }

        private void OnFinishTestPlaying()
        {
            Media.Source = null;
            Media.MediaEnded -= Media_MediaEnded;
            Media.MediaFailed -= MediaOnMediaFailed;
            _isMediaPlaying = false;
            TestPlayFileCommand.RaiseCanExecuteChanged();
        }


        #endregion Commands
    }
}
