using System;
using System.ComponentModel;
using System.Windows.Input;
using Scheduler.Models;

namespace Scheduler.ViewModels
{
    public enum SchedulerItemType
    {
        [Description("Будильник")] Alarm,
        [Description("Событие")] Event,
        [Description("Заметка")] Note
    }

    public abstract class SchedulerItemViewModel : BaseViewModel
    {
        protected readonly SchedulerItem _item;
        private string _text;

        public string Text
        {
            get { return _text; }
            set
            {
                if (Text == value) return;
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }

        public abstract SchedulerItemType Type { get; }

        public ICommand Edit { get; }
        public ICommand Delete { get; }

        protected SchedulerItemViewModel(SchedulerItem item)
        {
            _item = item;
            _text = item.Text;
            Edit = new Command(OnEdit);
            Delete = new Command(OnDelete);
        }

        private void OnEdit()
        {
            MainViewModel mainViewModel = MainViewModel.FindInstance();
            mainViewModel.OnEditItem(this);
        }

        private void OnDelete()
        {
            MainViewModel mainViewModel = MainViewModel.FindInstance();
            mainViewModel.OnDeleteItem(this);
        }

        public virtual void Save()
        {
            _item.Text = Text;
        }

        public override string ToString()
        {
            return Text;
        }
    }

    public abstract class ReminderItemViewModel : SchedulerItemViewModel
    {
        private bool _isExpired;
        private PostponeItem _postpone;

        public bool IsExpired
        {
            get { return _isExpired; }
            private set
            {
                if (IsExpired == value) return;
                _isExpired = value;
                OnPropertyChanged(nameof(IsExpired));
            }
        }

        public PostponeItem Postpone
        {
            get { return _postpone; }
            set
            {
                if (Postpone == value) return;
                _postpone = value;
                OnPostponeChanged();
            }
        }

        public bool AllowPostpone
        {
            get { return Postpone != CompleteItem.Instance; }
        }
        public string PostponeDesctiption
        {
            get { return Postpone.Description; }
        }

        public abstract bool IsOneTimeReminder { get; }
        public abstract string PeriodicityDescription { get; }

        protected ReminderItemViewModel(SchedulerItem item) : base(item)
        {
            _postpone = CompleteItem.Instance;
        }

        public abstract DateTime GetReminderDateTime();

        public virtual void OnCurrentReminderIsExpired()
        {
            IsExpired = IsOneTimeReminder;
        }

        private void OnPostponeChanged()
        {
            OnPropertyChanged(nameof(Postpone));
            OnPropertyChanged(nameof(AllowPostpone));
            OnPropertyChanged(nameof(PostponeDesctiption));
            if (AllowPostpone)
            {
                MainViewModel mainViewModel = MainViewModel.FindInstance();
                mainViewModel.Postpone(this, Postpone.Minutes);
            }
        }

    }
}
