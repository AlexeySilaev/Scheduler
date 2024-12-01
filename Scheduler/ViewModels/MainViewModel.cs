using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Scheduler.Extensions;
using Scheduler.Models;
using Scheduler.UI;

namespace Scheduler.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ReminderConveyor _reminderConveyor;

        private SchedulerGroup _selectedGroup;
        private SchedulerItemViewModel _selectedItem;
        private string _searchText;
        private string _error;
        private string _validationWarning;
        private ConfirmDeleteViewModel _confirmDelete;
        private SchedulerItemViewModel _editingItem;

        private AlarmGroup Alarms
        {
            get { return (AlarmGroup)Groups[0]; }
        }
        private EventGroup Events
        {
            get { return (EventGroup)Groups[1]; }
        }

        public SchedulerGroup[] Groups { get; }

        public SchedulerGroup SelectedGroup
        {
            get { return _selectedGroup; }
            set
            {
                if (SelectedGroup == value) return;
                SearchText = string.Empty;
                OnCancelEditItem();
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
                OnCancelEditItem();
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

        public string ValidationWarning
        {
            get { return _validationWarning; }
            set
            {
                if (ValidationWarning == value) return;
                _validationWarning = value;
                OnPropertyChanged(nameof(ValidationWarning));
            }
        }

        public ConfirmDeleteViewModel ConfirmDelete
        {
            get { return _confirmDelete; }
            set
            {
                if (ConfirmDelete == value) return;
                _confirmDelete = value;
                OnPropertyChanged(nameof(ConfirmDelete));
            }
        }

        public SchedulerItemViewModel EditingItem
        {
            get { return _editingItem; }
            set
            {
                if (EditingItem == value) return;
                _editingItem = value;
                OnPropertyChanged(nameof(EditingItem));
                if (EditingItem == null)
                    ValidationWarning = null;
            }
        }


        public ICommand AddItemCommand { get; }
        public ICommand SaveItemCommand { get; }
        public ICommand CancelEditItemCommand { get; }
        public ICommand SettingsCommand { get; }

        public MainViewModel()
        {
            _searchText = string.Empty;
            Groups = new[] { new AlarmGroup(), new EventGroup(), (SchedulerGroup)new NoteGroup() };
            _reminderConveyor = new ReminderConveyor(Alarms, Events);
            SelectedGroup = Groups[0];
            AddItemCommand = new Command(OnAddItem);
            SaveItemCommand = new Command(OnEndEditItem);
            CancelEditItemCommand = new Command(OnCancelEditItem);
            SettingsCommand = new Command(OnSettingsCommand);
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
            EditingItem = schedulerItemViewModel;
        }

        public void OnDeleteItem(SchedulerItemViewModel schedulerItemViewModel)
        {
            try
            {
                if(SchedulerModel.Instance.Settings.DeleteSchedulerItemsMode == DeleteSchedulerItemsMode.Immediately)
                    DeleteItem(schedulerItemViewModel);
                else
                {
                    Debug.Assert(
                        SchedulerModel.Instance.Settings.DeleteSchedulerItemsMode == DeleteSchedulerItemsMode.Confirm);
                    ConfirmDelete = new ConfirmDeleteViewModel(schedulerItemViewModel,
                        confirmed =>
                        {
                            ConfirmDelete = null;
                            if (confirmed)
                                DeleteItem(schedulerItemViewModel);
                        });
                }
            }
            catch (Exception x)
            {
                Error = $"Ошибка удаления элемента: {x.Message}";
            }
        }

        private void DeleteItem(SchedulerItemViewModel schedulerItemViewModel)
        {
            int index = Convert.ToInt32(schedulerItemViewModel.Type);
            Groups[index].Remove(schedulerItemViewModel);
            _reminderConveyor.Reconstruct();
        }

        private void OnAddItem()
        {
            EditingItem = SelectedGroup.CreateNewItem();
        }

        private void OnEndEditItem()
        {
            ValidationWarning = EditingItem.Validate();
            if (string.IsNullOrEmpty(ValidationWarning))
            {
                EditingItem.EndEdit();
                SelectedGroup.CheckAddNew(EditingItem);
                EditingItem = null;
                FilterSchedulerGroupItems();
                _reminderConveyor.Reconstruct();
            }
        }

        private void OnCancelEditItem()
        {
            if (EditingItem == null) return;

            EditingItem.CancelEdit();
            EditingItem = null;
            ValidationWarning = null;
        }

        private void OnSettingsCommand()
        {
            ConfirmDelete = null;
            OnCancelEditItem();
        }

        private void FilterSchedulerGroupItems()
        {
            if (SelectedGroup != null)
                SelectedGroup.Filter(SearchText);
        }
    }
}
