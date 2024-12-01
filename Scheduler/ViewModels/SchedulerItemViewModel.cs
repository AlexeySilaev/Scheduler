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

        public Command Edit { get; }
        public ICommand Delete { get; }

        protected SchedulerItemViewModel(SchedulerItem item)
        {
            _item = item;
            _text = item.Text;
            Edit = new Command(OnEdit, CanExecuteEdit);
            Delete = new Command(OnDelete);
        }

        protected abstract bool CanExecuteEdit();

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

        public virtual string Validate()
        {
            if (string.IsNullOrWhiteSpace(Text))
                return "Необходимо заполнить описание.";
            return null;
        }

        public virtual void EndEdit()
        {
            _item.Text = Text;
        }

        public virtual void CancelEdit()
        {
            Text = _item.Text;
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
}
