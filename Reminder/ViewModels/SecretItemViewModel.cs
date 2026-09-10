using Reminder.Models;

namespace Reminder.ViewModels
{
    public class SecretItemViewModel : SchedulerItemViewModel
    {
        private int _viewCounter;
        private SecretKind _kind;

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

        public SecretItemViewModel(SecretItem item) : base(item)
        {
            _kind = item.Kind;
            _viewCounter = item.ViewCounter;
        }

        protected override bool CanExecuteEdit()
        {
            return true;
        }

        public override void EndEdit()
        {
            base.EndEdit();
            SecretItem.Kind = Kind;
        }

        public override void CancelEdit()
        {
            base.CancelEdit();
            Kind = SecretItem.Kind;
        }

        public override void Save()
        {
            ReminderModel.Instance.Secrets.Add((SecretItem)_item);
        }
    }
}
