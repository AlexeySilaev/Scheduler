using System.ComponentModel;

namespace Scheduler.ViewModels
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

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }
    }
}
