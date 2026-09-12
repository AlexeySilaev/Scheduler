using Reminder.Models;
using Reminder.Resources.Storage;
using System;
using System.Windows;
using System.Windows.Input;

namespace Reminder.ViewModels
{
    public class SecretItemViewModel : SchedulerItemViewModel
    {
        private int _viewCounter;
        private SecretKind _kind;
        private Guid _value;
        private string _secretString;

        private SecretItem SecretItem
        {
            get { return (SecretItem)_item; }
        }

        public override ReminderItemType Type
        {
            get { return ReminderItemType.Secret; }
        }

        public SecretKind Kind
        {
            get { return _kind; }
            set 
            {
                if (Kind == value) return;
                _kind = value;
                OnPropertyChanged(nameof(Kind));
            }
        }

        public int ViewCounter
        {
            get { return _viewCounter; }
            set
            {
                if (ViewCounter == value) return;
                _viewCounter = value;
                OnPropertyChanged(nameof(ViewCounter));
            }
        }

        public Guid Value
        {
            get { return _value; }
            set
            {
                if (Value == value) return;
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        public string SecretString
        {
            get { return _secretString; }
            set
            {
                if (SecretString == value) return;
                _secretString = value;
                OnPropertyChanged(nameof(SecretString));
            }
        }

        public ICommand Copy { get; }

        public SecretItemViewModel(SecretItem item) : base(item)
        {
            _kind = item.Kind;
            _viewCounter = item.ViewCounter;
            _value = item.Value;
            _secretString = Secrets.Instance.GetStringSecret(Value);
            Copy = new Command(OnCopy);
        }

        private void OnCopy()
        {
            Clipboard.SetText(SecretString);
            SecretItem.ViewCounter = ++ViewCounter;
        }

        protected override bool CanExecuteEdit()
        {
            return true;
        }

        public override void EndEdit()
        {
            base.EndEdit();
            if (Value == Guid.Empty)
                Value = Secrets.Instance.AddStringSecret(SecretString);
            else
                Value = Secrets.Instance.UpdateStringSecret(Value, SecretString);
            SecretItem.Kind = Kind;
            SecretItem.Value = Value;
            SecretItem.ViewCounter = ++ViewCounter;
        }

        public override void CancelEdit()
        {
            base.CancelEdit();
            Kind = SecretItem.Kind;
            Value = SecretItem.Value;
            SecretString = Secrets.Instance.GetStringSecret(Value);
            SecretItem.ViewCounter = ++ViewCounter;
        }

        public override void Save()
        {
            ReminderModel.Instance.Secrets.Add((SecretItem)_item);
        }

        public override string Validate(bool isNew)
        {
            string result = base.Validate(isNew);
            if (string.IsNullOrEmpty(result))
            {
                if (string.IsNullOrEmpty(SecretString))
                    return "Необходимо заполнить значение.";
            }
            return result;
        }

    }
}
