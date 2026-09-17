using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Reminder.Models;
using Reminder.UI;
using Reminder.Extensions;

namespace Reminder.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private const double
            POPUP_WIDTH = 350,
            POPUP_HEIGHT = 215;
        private readonly ReminderConveyor _reminderConveyor;
        private readonly SettingsViewModel _settings;

        private ISchedulerGroup _selectedGroup;
        private SchedulerItemViewModel _selectedItem;
        private string _searchText;
        private string _error;
        private string _validationWarning;
        private bool _isShowComingDayAboutEvents;
        private bool _isShifted;
        private Rect _popupRect;
        private ConfirmDeleteViewModel _confirmDelete;
        private ComingDayEventsViewModel _comingDayEvents;
        private SchedulerItemViewModel _editingItem;
        private HostContent _hostContent;

        private EventGroup Events
        {
            get { return (EventGroup)Groups[0]; }
        }

        private AlarmGroup Alarms
        {
            get { return (AlarmGroup)Groups[1]; }
        }

        public ISchedulerGroup [] Groups { get; }

        public ISchedulerGroup SelectedGroup
        {
            get { return _selectedGroup; }
            set
            {
                if (SelectedGroup == value) return;
                SearchText = string.Empty;
                OnCancelEditItem();
                _selectedGroup = value;
                OnPropertyChanged(nameof(SelectedGroup));
                if (SelectedGroup is ISupportHostContent hostContentGroup && hostContentGroup.IsHostContentAllowed)
                    SetHostContent(hostContentGroup.GetHostContent(), SelectedGroup);
                else
                    HostContent = null;
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

        public bool IsShifted
        {
            get { return _isShifted; }
            set
            {
                if (IsShifted == value) return;
                _isShifted = value;
                OnPropertyChanged(nameof(IsShifted));
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
                IsShifted = EditingItem != null;
            }
        }

        public HostContent HostContent
        {
            get { return _hostContent; }
            private set
            {
                if (HostContent?.ViewModel == value?.ViewModel) return;
                _hostContent = value;
                OnPropertyChanged(nameof(HostContent));
            }
        }

        public double MainWindowLeft
        {
            get { return AppSettings.MainWindowLeft; }
            set
            {
                if (MainWindowLeft == value) return;
                AppSettings.MainWindowLeft = value;
                OnPropertyChanged(nameof(MainWindowLeft));
            }
        }
        public double MainWindowTop
        {
            get { return AppSettings.MainWindowTop; }
            set
            {
                if (MainWindowTop == value) return;
                AppSettings.MainWindowTop = value;
                OnPropertyChanged(nameof(MainWindowTop));
            }
        }

        public double MainWindowHeight
        {
            get { return AppSettings.MainWindowHeight; }
            set
            {
                if (MainWindowHeight == value) return;
                AppSettings.MainWindowHeight = value;
                OnPropertyChanged(nameof(MainWindowHeight));
            }
        }

        public double MainWindowWidth
        {
            get { return AppSettings.MainWindowWidth; }
        }

        public double MainWindowMinHeight
        {
            get { return AppSettings.MainWindowMinHeight; }
        }

        Settings AppSettings { get { return ReminderModel.Instance.Settings; } }


        private void Settings_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.IntroductoryMode))
            {
                // прокидываем изменившуюся настройку, мы находимся в настройках
                // пользователь щелкат галку ознакомительного режима
                // вынос в отдельное свойство обусловлен тем что свойство Settings всегда null,
                // если не отображаются настройки, и биндинг Settings.IntroductoryMode не отработает
                OnPropertyChanged(nameof(IntroductoryMode));
            }
        }

        /// <summary>
        /// Признак ознакомительного режима.
        /// </summary>
        public bool IntroductoryMode { get { return AppSettings.IntroductoryMode; } }

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
        public ICommand ManagementCommand { get; }
        public ICommand SettingsCommand { get; }

        public NotifyIconHandler NotifyIconHandler { get; }


        public MainViewModel()
        {
            Groups = new[] { new EventGroup(), new AlarmGroup(), (ISchedulerGroup)new NoteGroup(),
                (ISchedulerGroup)new ProjectGroup(), (ISchedulerGroup)new SecretGroup() };
            _settings = new SettingsViewModel(AppSettings);
            _settings.PropertyChanged += Settings_OnPropertyChanged;

            _searchText = string.Empty;
            _reminderConveyor = new ReminderConveyor(Alarms, Events, OnConveyorRefresh);
            _popupRect = Rect.Empty;
            SelectedGroup = Groups[0];
            AddItemCommand = new Command(OnAddItem);
            SaveItemCommand = new Command(OnEndEditItem);
            CancelEditItemCommand = new Command(OnCancelEditItem);
            ManagementCommand = new Command(OnManagement);
            SettingsCommand = new Command(OnSettings);
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

        // замена использования DI
        public static MainViewModel FindInstance()
        {
            return App.Current.MainWindow.DataContext as MainViewModel;
        }

        public void Postpone(ReminderItemViewModel reminderItemViewModel, PostponeItem postpone)
        {
            if(postpone != ContinueIntervalItem.Instance) // элемент "Завтра" пересчитается в событиях
                Alarms.AddPostponed(reminderItemViewModel, postpone.Minutes);
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
                if(AppSettings.DeleteReminderItemsMode == DeleteReminderItemsMode.Immediately)
                    DeleteItem(schedulerItemViewModel);
                else
                {
                    Debug.Assert(
                        AppSettings.DeleteReminderItemsMode == DeleteReminderItemsMode.Confirm);
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
            schedulerItemViewModel.EndDelete();
            if (Groups[index] == SelectedGroup)
                OnPropertyChanged(nameof(SelectedGroup)); // обновим количество найденных элементов
            SaveToModel();
            _reminderConveyor.Reconstruct();
        }

        private void OnAddItem()
        {
            EditingItem = SelectedGroup.CreateNewItem();
        }

        private void OnEndEditItem()
        { 
            ValidationWarning = EditingItem.Validate(EditingItem.In(SelectedGroup.Items) == false);
            if (string.IsNullOrEmpty(ValidationWarning))
            {
                EditingItem.EndEdit();
                SelectedGroup.CheckAddNew(EditingItem);
                IsShifted = false;
                SaveToModel();
                FilterSchedulerGroupItems();
                _reminderConveyor.Reconstruct();
            }
        }

        private void OnCancelEditItem()
        {
            if (EditingItem == null) return;

            EditingItem.CancelEdit();
            IsShifted = false;
            ValidationWarning = null;
        }

        public void OnIsShiftedChangedCompleted(bool isShifted)
        {
            if (isShifted == false)
            {
                // закончился скроллинг панели редактирования в исходное состояние
                // теперь можно обнулить редактируемый элемент
                // если это сделать на начале скроллинга (сразу на окончании редактирования), панель поедет задвигаться пустой
                EditingItem = null;
            }
        }

        public void OnImportCompleted()
        {
            OnCancelEditItem();
            InitializeAsync(App.Current.MainWindow.Dispatcher);
        }

        private void OnManagement()
        {
            ManagementViewModel management = new ManagementViewModel((ISupportHostContent)Groups[4]);
            SetHostContent(new HostContent(new ManagementControl(), management), null);
        }

        private void OnSettings()
        {
            SetHostContent(new HostContent(new SettingsControl(), _settings), null);
        }

        private void SetHostContent(HostContent hostContent, ISchedulerGroup selectedGroup)
        {
            ConfirmDelete = null;
            SelectedGroup = selectedGroup;
            OnCancelEditItem();
            HostContent = hostContent;
        }

        private void OnConveyorRefresh(EventItemViewModel[] newDayAboutEvents)
        {
            Alarms.UpdateRemainTime();
            if (AppSettings.ShowComingDayAboutEvents == false
                || newDayAboutEvents.IsEmpty())
                return;

            PopupRect = WindowPositioning.GetRightBottomBounds(new Size(POPUP_WIDTH, POPUP_HEIGHT));
            if (ComingDayEvents == null)
                ComingDayEvents = new ComingDayEventsViewModel(newDayAboutEvents, POPUP_WIDTH, POPUP_HEIGHT);
            else
                newDayAboutEvents.ForEach(dayAbout => ComingDayEvents.Items.Add(dayAbout));
            IsShowComingDayAboutEvents = true;
        }

        public void SaveToModel()
        {
            if (ReminderModel.Instance.IsLoaded)
            {
                Groups.ForEach(group => group.Save());
                ReminderModel.Instance.Save();
            }
        }

        public void OnReminderActivated(bool withEvent)
        {
            if (withEvent)
                Events.OnEventActivated(SearchText);
            OnPropertyChanged(nameof(SelectedGroup)); // обновим количество найденных элементов
        }

        private void FilterSchedulerGroupItems()
        {
            if (SelectedGroup != null)
                SelectedGroup.Filter(SearchText);
        }
    }
}
