using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Reminder.Models;
using Reminder.UI;
using Reminder.Extensions;
using System.Windows;
using System.Collections.ObjectModel;

namespace Reminder.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private const double
            POPUP_WIDTH = 350,
            POPUP_HEIGHT = 215;
        private readonly ReminderConveyor _reminderConveyor;

        private SchedulerGroup _selectedGroup;
        private SchedulerItemViewModel _selectedItem;
        private string _searchText;
        private string _error;
        private string _validationWarning;
        private bool _isShowComingDayAboutEvents;
        private Rect _popupRect;
        private ConfirmDeleteViewModel _confirmDelete;
        private ComingDayEventsViewModel _comingDayEvents;
        private SchedulerItemViewModel _editingItem;
        private SettingsViewModel _settings;

        private EventGroup Events
        {
            get { return (EventGroup)Groups[0]; }
        }

        private AlarmGroup Alarms
        {
            get { return (AlarmGroup)Groups[1]; }
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
                Settings = null;
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
                _error = value;
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

        public ComingDayEventsViewModel ComingDayEvents
        {
            get { return _comingDayEvents; }
            set
            {
                if (ComingDayEvents == value) return;
                _comingDayEvents = value;
                OnPropertyChanged(nameof(ComingDayEvents));
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

        public SettingsViewModel Settings
        {
            get { return _settings; }
            set
            {
                if (Settings == value) return;
                _settings = value;
                OnPropertyChanged(nameof(Settings));
            }
        }

        /// <summary>
        /// Показывать всплывающее окно о грядущих за сутки событиях.
        /// Относится только к событиям, до наступления которых не меньше суток.
        /// </summary>
        public bool IsShowComingDayAboutEvents
        {
            get { return _isShowComingDayAboutEvents; }
            set
            {
                if (IsShowComingDayAboutEvents == value) return;
                _isShowComingDayAboutEvents = value;
                OnPropertyChanged(nameof(IsShowComingDayAboutEvents));
                if (IsShowComingDayAboutEvents == false)
                    ComingDayEvents = null;
            }
        }

        public Rect PopupRect
        {
            get { return _popupRect; }
            set
            {
                if (PopupRect == value) return;
                _popupRect = value;
                OnPropertyChanged(nameof(PopupRect));
            }
        }

        public ICommand AddItemCommand { get; }
        public ICommand SaveItemCommand { get; }
        public ICommand CancelEditItemCommand { get; }
        public ICommand SettingsCommand { get; }

        public NotifyIconHandler NotifyIconHandler { get; }


        public MainViewModel()
        {
            _searchText = string.Empty;
            Groups = new[] { new EventGroup(), new AlarmGroup(), (SchedulerGroup)new NoteGroup() };
            _reminderConveyor = new ReminderConveyor(Alarms, Events, OnConveyorRefresh);
            _popupRect = Rect.Empty;
            SelectedGroup = Groups[0];
            AddItemCommand = new Command(OnAddItem);
            SaveItemCommand = new Command(OnEndEditItem);
            CancelEditItemCommand = new Command(OnCancelEditItem);
            SettingsCommand = new Command(OnSettingsCommand);
            NotifyIconHandler = new NotifyIconHandler();
        }

        public void InitializeAsync(Dispatcher dispather)
        {
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

        public void OnClosed()
        {
            if (ReminderModel.Instance.IsLoaded == false) return;
            _reminderConveyor.Stop();
            SaveToModel();
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
                if(ReminderModel.Instance.Settings.DeleteReminderItemsMode == DeleteReminderItemsMode.Immediately)
                    DeleteItem(schedulerItemViewModel);
                else
                {
                    Debug.Assert(
                        ReminderModel.Instance.Settings.DeleteReminderItemsMode == DeleteReminderItemsMode.Confirm);
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
                SaveToModel();
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
            SelectedGroup = null;
            OnCancelEditItem();
            Settings = new SettingsViewModel(ReminderModel.Instance.Settings);
        }

        private void OnConveyorRefresh(EventItemViewModel[] newDayAboutEvents)
        {
            Alarms.UpdateRemainTime();
            if (ReminderModel.Instance.Settings.ShowComingDayAboutEvents == false
                || newDayAboutEvents.IsEmpty())
                return;

            PopupRect = WindowPositioning.GetRightBottomBounds(new Size(POPUP_WIDTH, POPUP_HEIGHT));
            if (ComingDayEvents == null)
                ComingDayEvents = new ComingDayEventsViewModel(newDayAboutEvents, POPUP_WIDTH, POPUP_HEIGHT);
            else
                newDayAboutEvents.ForEach(dayAbout => ComingDayEvents.Items.Add(dayAbout));
            IsShowComingDayAboutEvents = true;
        }

        private void FilterSchedulerGroupItems()
        {
            if (SelectedGroup != null)
                SelectedGroup.Filter(SearchText);
        }

        public void SaveToModel()
        {
            if (ReminderModel.Instance.IsLoaded)
            {
                Groups.ForEach(group => group.Save());
                ReminderModel.Instance.Save();
            }
        }
    }
}
