using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Scheduler.Extensions;
using Scheduler.UI;

namespace Scheduler.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private const double SLOW_TIMER_INTERVAL = 1000 * 1;//30;
        private const double FAST_TIMER_INTERVAL = 1000 * 0.5;

        private readonly Timer _queueTimer;
        private readonly object _lockQueue;
        private SchedulerGroup _selectedGroup;
        private SchedulerItemViewModel _selectedItem;
        private string _searchText;
        private string _error;
        private Queue<TimerReminderItem> _timerReminderQueue;
        private ReminderWindowViewModel _reminderWindowViewModel;

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
            _timerReminderQueue = new Queue<TimerReminderItem>();
            _queueTimer = new Timer();
            _queueTimer.AutoReset = false;
            _queueTimer.Elapsed += QueueTimer_Elapsed;
            _lockQueue = new object();
            Groups = new[] { new Alarms(), new Events(), (SchedulerGroup)new Notes() };
            SelectedGroup = Groups[0];
            AddItemCommand = new Command(OnAddItem);
        }

        public void InitializeAsync(Dispatcher dispather)
        {
            WindowPositioning.DefinePrimaryMonitor();
            Task.Factory.StartNew(() =>
            {
                Groups.ForEach(group => group.Initialize());
                RebuildAlarmQueue();
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
            _queueTimer.Enabled = false;
            if (_reminderWindowViewModel?.View != null)
            {
                _reminderWindowViewModel.View.Close();
                _reminderWindowViewModel = null;
            }
        }

        public static MainViewModel FindInstance()
        {
            return App.Current.MainWindow.DataContext as MainViewModel;
        }

        public void Postpone(ReminderItemViewModel reminderItemViewModel, int postponeMinutes)
        {
            Alarms.AddPostponed(reminderItemViewModel, postponeMinutes);
            Alarms.Filter(SearchText);
            RebuildAlarmQueue();
            // если все одноразовые элементы отложены, просматривать больше нечего
            if (_reminderWindowViewModel.IsAllOneTineItemsPostponed())
            {
                _reminderWindowViewModel.View.Close();
                _reminderWindowViewModel = null;
            }
        }

        public void OnEditItem(SchedulerItemViewModel schedulerItemViewModel)
        {
        }

        public void OnDeleteItem(SchedulerItemViewModel schedulerItemViewModel)
        {
        }

        private void RebuildAlarmQueue()
        {
            lock (_lockQueue)
            {
                List<TimerReminderItem> queue = Alarms.GetTimerReminderItems();
                queue.AddRange(Events.GetTimerReminderItems());
                queue.Sort();

                _timerReminderQueue = new Queue<TimerReminderItem>(queue);
                CheckStartTimer();
            }
        }

        private void CheckStartTimer()
        {
            if (_timerReminderQueue.Any() == false) return;

            TimerReminderItem nearestReminderItem = _timerReminderQueue.Peek();
            TimeSpan timeToNearestReminder = nearestReminderItem.ReminderDateTime - DateTime.Now;
            if (timeToNearestReminder.TotalMilliseconds < SLOW_TIMER_INTERVAL)
                _queueTimer.Interval = FAST_TIMER_INTERVAL; // будем тикать чаще, чтобы точнее попасть во время Ч
            else
                _queueTimer.Interval = SLOW_TIMER_INTERVAL;
            _queueTimer.Enabled = true;
            _queueTimer.AutoReset = true;
        }

        /* Работа алгоритма.
           Для будильников и событий вычисляется ближайшее время срабатывания (в случае будильников и одноразовых событий 
           оно единственно). Это вреия инкапсулируется элементами TimerReminderItem, они сортируются по возрастанию времени 
           срабатывания ReminderDateTime и ставятся на конвейер _timerReminderQueue. После того, как наступает ближайшее 
           срабатывание, очередь перестраивается, чтобы обновить ближайшее время срабатывания для периодических событий. 
           Например, для сработавшего ежедевного события в очереди появится новый элемент, который будет обработан через сутки. */

        /// <summary>
        /// В процессе работы рядом стоящие по времени уведомления будут показаны в общем окне.
        /// </summary>
        private void QueueTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_lockQueue)
            {
                _queueTimer.Enabled = false;
                TimerReminderItem nearestReminderItem = _timerReminderQueue.Peek();
                // чтобы не терять в окне уведомлений элементы с одинаковым временем,
                // пробежимся по голове очереди, где может быть серия просроченных
                if (nearestReminderItem.CheckExpired(e.SignalTime))
                {
                    List<ReminderItemViewModel> expired = new List<ReminderItemViewModel>();
                    while(nearestReminderItem.CheckExpired(e.SignalTime))
                    {
                        _timerReminderQueue.Dequeue();
                        expired.Add(nearestReminderItem.Source);

                        if (_timerReminderQueue.Any())
                            nearestReminderItem = _timerReminderQueue.Peek();
                        else break;
                    }
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        OnReminderActivated(expired)));

                    RebuildAlarmQueue();

                }
                else
                    CheckStartTimer();

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    Alarms.UpdateRemainTime()));

            }
        }

        private void OnReminderActivated(List<ReminderItemViewModel> expired)
        {
            if (_reminderWindowViewModel == null || _reminderWindowViewModel.View == null)
            {
                ReminderWindow window = new ReminderWindow();
                ReminderWindowViewModel context = (ReminderWindowViewModel)window.DataContext;
                context.View = window;
                expired.ForEach(reminder => context.Items.Add(reminder));
                window.SetRightBottom();
                window.Show();
                _reminderWindowViewModel = context;
            }
            else
            {
                expired.ForEach(reminder => _reminderWindowViewModel.Items.Add(reminder));
            }
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
