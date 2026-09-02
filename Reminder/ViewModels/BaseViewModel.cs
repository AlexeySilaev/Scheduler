using System.ComponentModel;

namespace Reminder.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        private event PropertyChangedEventHandler _propertyChanged;

        protected BaseViewModel()
        {
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (_propertyChanged != null)
                _propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }
    }
}
