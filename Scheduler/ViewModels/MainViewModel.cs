using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Scheduler.Extensions;

namespace Scheduler.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ReminderConveyor _reminderConveyor;

        private SchedulerGroup _selectedGroup;
        private SchedulerItemViewModel _selectedItem;
        private string _searchText;
        private string _error;

        private Alarms Alarms
        {
            get { return (Alarms)Groups[0]; }
        }
        private Events Events
        {
            get { return (Events)Groups[1]; }
        }

        public SchedulerGroup[] Groups { get; }

        public SchedulerGroup SelectedGroup
        {
            get { return _selectedGroup; }
            set
            {
                if (SelectedGroup == value) return;
                SearchText = string.Empty;
                _selectedGroup = value;
                OnPropertyChanged(nameof(SelectedGroup));
            }
        }
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (SearchText == value) return;
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                FilterSchedulerGroupItems();
            }
        }

        public SchedulerItemViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (SelectedItem == value) return;
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        public string Error
        {
            get { return _error; }
            set
            {
                if (Error == value) return;
                _error= value;
                OnPropertyChanged(nameof(Error));
            }
        }

        public ICommand AddItemCommand { get; }

        public MainViewModel()
        {
            _searchText = string.Empty;
            Groups = new[] { new Alarms(), new Events(), (SchedulerGroup)new Notes() };
            _reminderConveyor = new ReminderConveyor(Alarms, Events);
            SelectedGroup = Groups[0];
            AddItemCommand = new Command(OnAddItem);
        }

        public void InitializeAsync(Dispatcher dispather)
        {
            WindowPositioning.DefinePrimaryMonitor();
            Task.Factory.StartNew(() =>
            {
                Groups.ForEach(group => group.Initialize());
                _reminderConveyor.Reconstruct();
            })
            .ContinueWith((t) =>
            {
                dispather.Invoke(() =>
                {
                    if (t.IsFaulted)
                        Error = $"Ошибка инициализации: {t.Exception.InnerException.Message}";
                    else
                        Groups.ForEach(group => group.InitializeComplete());
                });
            });

        }

        public void OnClosing()
        {
            _reminderConveyor.Stop();
        }

        public static MainViewModel FindInstance()
        {
            return App.Current.MainWindow.DataContext as MainViewModel;
        }

        public void Postpone(ReminderItemViewModel reminderItemViewModel, int postponeMinutes)
        {
            Alarms.AddPostponed(reminderItemViewModel, postponeMinutes);
            Alarms.Filter(SearchText);
            _reminderConveyor.OnReminderItemPostponed();
        }

        public void OnEditItem(SchedulerItemViewModel schedulerItemViewModel)
        {
        }

        public void OnDeleteItem(SchedulerItemViewModel schedulerItemViewModel)
        {
        }

        private void FilterSchedulerGroupItems()
        {
            if (SelectedGroup != null)
                SelectedGroup.Filter(SearchText);
        }

        private void OnAddItem()
        {
        }

    }
}
