using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;
using Reminder.Models;
using Reminder.Resources.ViewModels;
using Reminder.ViewModels;

namespace Reminder.UI
{
    internal class ReminderWindowViewModel : BaseViewModel
    {
        private readonly SpeechSynthesizer _synthesizer;
        private ReminderWindow _window;
        private MediaElement _media;
        private Visibility _tipVisibility;
        private bool _isPlaying;

        private Settings Settings
        {
            get { return ReminderModel.Instance.Settings; }
        }

        public ObservableCollection<TimerReminderItem> Items { get; }
        public PostponeItem[] PostponeItems { get; }
        public PostponeItem[] IntervalItems { get; }

        public TimerReminderItem ReminderItemUnderMouse { get; set; }

        public ReminderWindow Window
        {
            get { return _window; }
            set
            {
                if (Window == value) return;
                _window = value;
                if (Window != null)
                {
                    _media = (MediaElement)Window.FindName("Media");
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

        public Visibility TipVisibility
        {
            get { return _tipVisibility; }
            private set
            {
                if (TipVisibility == value) return;
                _tipVisibility = value;
                OnPropertyChanged(nameof(TipVisibility));
            }
        }

        public ReminderWindowViewModel()
        {
            Items = new ObservableCollection<TimerReminderItem>();
            Items.CollectionChanged += ItemsOnCollectionChanged;
            if (App.Current.MainWindow != null)
                MainViewModel.FindInstance().PropertyChanged += MainViewModel_PropertyChanged;
            PostponeItems = new[]
            {
                CompleteItem.Instance,
                new PostponeItem("15 мин", 15),
                new PostponeItem("30 мин", 30),
                new PostponeItem("1 час", 60),
                new PostponeItem("2 часа", 120),
                new PostponeItem("4 часа", 240),
                new OtherItem(new Command(OnAcceptOtherItem))
            };
            IntervalItems = new PostponeItem[]
            {
                BreakIntervalItem.Instance,
                ContinueIntervalItem.Instance
            };
            _synthesizer = new SpeechSynthesizer();
            _synthesizer.SetOutputToDefaultAudioDevice();
            _tipVisibility = GetTipVisibility();
        }

        public void OnWindowClosed()
        {
            Window = null;
            Items.CollectionChanged -= ItemsOnCollectionChanged;
            if (App.Current.MainWindow != null)
                MainViewModel.FindInstance().PropertyChanged -= MainViewModel_PropertyChanged;
        }

        public bool IsAllItemsPostponed()
        {
            return Items.Count == Items.Count(item => item.AllowPostpone);
        }

        private void MainViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.IntroductoryMode))
                TipVisibility = GetTipVisibility();
        }

        private Visibility GetTipVisibility()
        { 
            return Settings.IntroductoryMode ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnAcceptOtherItem()
        {
            if (ReminderItemUnderMouse != null)
                ReminderItemUnderMouse.Postpone = PostponeItems.Last();
        }

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add) return;
            if (_isPlaying) return;
            RingtoneMode ringtoneMode = Settings.RingtoneMode;
            TimerReminderItem reminder = (TimerReminderItem)e.NewItems[0];
            if (reminder.Source.Ringtone != ReminderRingtone.AsIs)
                ringtoneMode = (RingtoneMode)Convert.ToInt32(reminder.Source.Ringtone);
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
                prompt.AppendTextWithHint(TimeProvider.Now.ToString("HH:mm"), SayAs.Time24);
                prompt.AppendBreak(TimeSpan.FromMilliseconds(1500));
                prompt.EndStyle();
                prompt.AppendText(reminder.Source.Text);
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

