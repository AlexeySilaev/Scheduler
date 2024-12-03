using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Scheduler.Models;
using Scheduler.ViewModels;

namespace Scheduler.UI
{
    internal class ReminderWindowViewModel : BaseViewModel
    {
        private ReminderWindow _view;
        private MediaElement _media;
        private bool _isPlaying;

        private Settings Settings
        {
            get { return SchedulerModel.Instance.Settings; }
        }

        public ObservableCollection<ReminderItemViewModel> Items { get; }
        public PostponeItem[] PostponeItems { get; }

        public ReminderWindow View
        {
            get { return _view; }
            set
            {
                if (View == value) return;
                _view = value;
                if (View != null)
                {
                    _media = (MediaElement)View.FindName("Media");
                    _media.MediaEnded += OnPlayingFinished;
                    _media.MediaFailed += OnPlayingFinished;
                }
                else if (_media != null)
                {
                    _media.MediaEnded -= OnPlayingFinished;
                    _media.MediaFailed -= OnPlayingFinished;
                    _media = null;
                }
            }
        }

        public bool TopMost
        {
            get { return SchedulerModel.Instance.Settings.TopMostNotificationsWindow; }
        }

        public ReminderWindowViewModel()
        {
            Items = new ObservableCollection<ReminderItemViewModel>();
            Items.CollectionChanged += ItemsOnCollectionChanged;
            PostponeItems = new[]
            {
                CompleteItem.Instance,
                new PostponeItem("5 мин", 5),
                new PostponeItem("10 мин", 10),
                new PostponeItem("30 мин", 30),
                new PostponeItem("1 час", 60),
                new PostponeItem("2 часа", 120),
                new PostponeItem("4 часа", 240),
            };
        }

        public bool IsAllOneTimeItemsPostponed()
        {
            return Items.Count(item => item.IsOneTimeReminder)
                   == Items.Count(item => item.IsOneTimeReminder && item.AllowPostpone);
        }

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add) return;
            if (Settings.RingtoneMode == RingtoneMode.NoSound) return;
            if (_isPlaying) return;

            string filePath = Settings.GetDefaultRingtoneFile();
            if (Settings.RingtoneMode == RingtoneMode.Custom && File.Exists(Settings.CustomSoundFilePath))
                filePath = Settings.CustomSoundFilePath;
            if (File.Exists(filePath) == false)
                return;

            _isPlaying = true;
            _media.Source = new Uri(filePath);
            _media.Play();
        }

        private void OnPlayingFinished(object sender, RoutedEventArgs e)
        {
            _isPlaying = false;
        }

    }
}

