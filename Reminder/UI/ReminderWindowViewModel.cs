using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;
using Reminder.Models;
using Reminder.ViewModels;

namespace Reminder.UI
{
    internal class ReminderWindowViewModel : BaseViewModel
    {
        private readonly SpeechSynthesizer _synthesizer;
        private ReminderWindow _view;
        private MediaElement _media;
        private bool _isPlaying;

        private Settings Settings
        {
            get { return ReminderModel.Instance.Settings; }
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
                    _synthesizer.SpeakCompleted += SynthesizerOnSpeakCompleted;
                }
                else if (_media != null)
                {
                    _media.MediaEnded -= OnPlayingFinished;
                    _media.MediaFailed -= OnPlayingFinished;
                    _media = null;
                    _synthesizer.SpeakCompleted -= SynthesizerOnSpeakCompleted;
                }
            }
        }

        public bool TopMost
        {
            get { return ReminderModel.Instance.Settings.TopMostNotificationsWindow; }
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
            _synthesizer = new SpeechSynthesizer();
            _synthesizer.SetOutputToDefaultAudioDevice();
        }

        public bool IsAllOneTimeItemsPostponed()
        {
            return Items.Count(item => item.IsOneTimeReminder)
                   == Items.Count(item => item.IsOneTimeReminder && item.AllowPostpone);
        }

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add) return;
            if (_isPlaying) return;
            RingtoneMode ringtoneMode = Settings.RingtoneMode;
            ReminderItemViewModel reminder = (ReminderItemViewModel)e.NewItems[0];
            if (reminder.Ringtone != ReminderRingtone.AsIs)
                ringtoneMode = (RingtoneMode)Convert.ToInt32(reminder.Ringtone);
            if (ringtoneMode == RingtoneMode.NoSound) return;

            if (ringtoneMode == RingtoneMode.SpeechSynthesizer)
            {
                PromptBuilder prompt = new PromptBuilder();
                PromptStyle timeStyle = new PromptStyle();
                timeStyle.Volume = PromptVolume.Soft;
                timeStyle.Rate = PromptRate.NotSet;
                prompt.StartStyle(timeStyle);
                prompt.AppendText("Уведомление напониналки");
                prompt.AppendBreak(TimeSpan.FromMilliseconds(1000));
                prompt.AppendTextWithHint(DateTime.Now.ToString("HH:mm"), SayAs.Time24);
                prompt.AppendBreak(TimeSpan.FromMilliseconds(1500));
                prompt.EndStyle();
                prompt.AppendText(reminder.Text);
                _synthesizer.Rate = Convert.ToInt32(PromptRate.Medium);
                _isPlaying = true;
                _synthesizer.SpeakAsync(prompt);
            }
            else
            {
                string filePath = Settings.GetDefaultRingtoneFile();
                if (ringtoneMode == RingtoneMode.Custom && File.Exists(Settings.CustomSoundFilePath))
                    filePath = Settings.CustomSoundFilePath;
                if (File.Exists(filePath) == false)
                    return;

                _isPlaying = true;
                _media.Source = new Uri(filePath);
                _media.Play();
            }
        }

        private void OnPlayingFinished(object sender, RoutedEventArgs e)
        {
            _isPlaying = false;
        }
        private void SynthesizerOnSpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            _isPlaying = false;
        }

    }
}

