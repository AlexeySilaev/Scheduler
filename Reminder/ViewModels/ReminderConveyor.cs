using Reminder.Extensions;
using Reminder.Models;
using Reminder.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace Reminder.ViewModels
{
    /* Работа алгоритма.
       Для событий, напоминаний и проектов вычисляется ближайшее время срабатывания (для одноразовых ReminderItemViewModel
       оно единственно). Это вреия инкапсулируется элементами TimerReminderItem, они сортируются по возрастанию времени
       срабатывания ReminderDateTime и ставятся на конвейер _timerReminderQueue. После того, как наступает ближайшее
       срабатывание, очередь перестраивается, чтобы обновить ближайшее время срабатывания для периодических событий.
       Например, для сработавшего ежедевного события в очереди появится новый элемент, который будет обработан через сутки. */

    internal class ReminderConveyor : IEqualityComparer<DayAboutItemViewModel>
    {
        private const double SLOW_TIMER_INTERVAL = 1000 * 30;
        private const double FAST_TIMER_INTERVAL = 1000 * 0.5;

        private readonly IReminderGroup _alarms;
        private readonly IReminderGroup _events;
        private readonly IReminderGroup _projects;
        private readonly Action<DayAboutItemViewModel[]> _onConveyorRefresh;
        private readonly Timer _queueTimer;
        private readonly object _lockQueue;
        private readonly List<DayAboutItemViewModel> _dayAboutItems; // события и проекты, которые сработают через сутки
        private readonly List<TimerReminderItem> _timerReminderQueue;

        private ReminderWindowViewModel _reminderWindowViewModel;

        public ReminderConveyor(IReminderGroup alarms, IReminderGroup events, IReminderGroup projects,
            Action<DayAboutItemViewModel[]> onConveyorRefresh)
        {
            _alarms = alarms;
            _events = events;
            _projects = projects;
            _onConveyorRefresh = onConveyorRefresh;
            _timerReminderQueue = new List<TimerReminderItem>();
            _queueTimer = new Timer();
            _queueTimer.AutoReset = false;
            _queueTimer.Elapsed += QueueTimer_Elapsed;
            _lockQueue = new object();
            _dayAboutItems = new List<DayAboutItemViewModel>();
        }

        /// <summary>
        /// Пересобрать конвейер.
        /// </summary>
        public void Reconstruct()
        {
            lock (_lockQueue)
            {
                _timerReminderQueue.Clear();
                _timerReminderQueue.AddRange(_alarms.CalculateTimerReminderItems());
                _timerReminderQueue.AddRange(_events.CalculateTimerReminderItems());
                _timerReminderQueue.AddRange(_projects.CalculateTimerReminderItems());
                _timerReminderQueue.Sort();
                CheckStartTimer();
            }
        }

        /// <summary>
        /// Остановить конвейер.
        /// </summary>
        public void Stop()
        {
            if (_reminderWindowViewModel?.Window != null)
            {
                _reminderWindowViewModel.Window.Close();
                _reminderWindowViewModel = null;
            }
        }

        public async void OnReminderItemPostponed()
        {
            Reconstruct();
            // если все одноразовые элементы отложены, просматривать больше нечего
            if (_reminderWindowViewModel.IsAllItemsPostponed())
            {
                // пусть пользователь успеет увидеть ввод
                await Task.Delay(TimeSpan.FromSeconds(1));
                _reminderWindowViewModel.Window?.Close(); // "Window?" если ловкий пользователь закрыл окно
                _reminderWindowViewModel = null;
            }
        }

        private void CheckStartTimer()
        {
            if (_timerReminderQueue.Any() == false) return;

            TimerReminderItem nearestReminderItem = _timerReminderQueue.FirstOrDefault(item => item.IsExpired == false);
            if (nearestReminderItem == null)
            {
                // если нет активных элементов, есть ли просроченные напоминания? Будем обновлять в интерфейсе время,
                // на которое они просрочены
                nearestReminderItem = _timerReminderQueue.FirstOrDefault(item => item.Source.Type == ReminderItemType.Alarm);
            }

            if (nearestReminderItem != null)
            {
                TimeSpan timeToNearestReminder = nearestReminderItem.ReminderDateTime - TimeProvider.Now;
                if (timeToNearestReminder.TotalMilliseconds < SLOW_TIMER_INTERVAL)
                    _queueTimer.Interval = FAST_TIMER_INTERVAL; // будем тикать чаще, чтобы точнее попасть во время Ч
                else
                    _queueTimer.Interval = SLOW_TIMER_INTERVAL;
                _queueTimer.Enabled = true;
                _queueTimer.AutoReset = true;
            }
        }

        /// <summary>
        /// В процессе работы рядом стоящие по времени уведомления будут показаны в общем окне.
        /// </summary>
        private void QueueTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_lockQueue)
            {
                _queueTimer.Enabled = false;
                List<TimerReminderItem> activeItems = _timerReminderQueue
                    .Where(item => item.IsExpired == false)
                    .ToList();
                // чтобы не терять в окне уведомлений элементы с одинаковым временем,
                // пробежимся по голове очереди, где может быть серия просроченных
                List<TimerReminderItem> expired = new List<TimerReminderItem>();
                for (int i = 0; i < activeItems.Count; ++i)
                {
                    TimerReminderItem nearestReminderItem = activeItems[i];
                    if (nearestReminderItem.CheckExpired(e.SignalTime))
                    {
                        _timerReminderQueue.Remove(nearestReminderItem);
                        expired.Add(nearestReminderItem);
                    }
                    else break;
                }

                if (expired.Any())
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            OnReminderActivated(expired)));
                    Reconstruct();
                }
                else
                    CheckStartTimer();

                bool updated = UpdateDayAboutEvents(e.SignalTime);
                DayAboutItemViewModel[] newDayAboutItems =
                    updated ? _dayAboutItems.ToArray() : Array.Empty<DayAboutItemViewModel>();
                Application.Current.Dispatcher.BeginInvoke(new Action(() => _onConveyorRefresh(newDayAboutItems)));
            }
        }

        // Отслеживаем события, которые отработают ровно через сутки
        // Если событие отработает минутой меньше, чем через сутки, оно устарело, удаляем его,
        // оно уже отправлено во всплывающее окно подсказки.
        // Если событии уже имеется в коллекции, ничего не делаем, оно было добавлено чуть ранее,
        // потому что таймер конвейера тикает чаще раза в минуту.
        private bool UpdateDayAboutEvents(DateTime signalTime)
        {
            List<DayAboutItemViewModel> currentDayItems = GetDayAboutItems(signalTime.AddDays(1));
            bool result = Enumerable.SequenceEqual(_dayAboutItems, currentDayItems, this) == false;
            _dayAboutItems.Clear();
            _dayAboutItems.AddRange(currentDayItems);
            return result;
        }

        private List<DayAboutItemViewModel> GetDayAboutItems(DateTime dayAboutTime)
        {
            // выравниваем время срабатывания по границе минуты
            DateTime from = dayAboutTime.Date.AddHours(dayAboutTime.Hour).AddMinutes(dayAboutTime.Minute);
            DateTime to = from.AddSeconds(59);
            List<EventItemViewModel> eventItems = _timerReminderQueue
                .Where(reminder => reminder.Source.Type == ReminderItemType.Event 
                && from <= reminder.ReminderDateTime && reminder.ReminderDateTime <= to)
                .Select(reminder => (EventItemViewModel)reminder.Source)
                .ToList();
            // ежедневные события и последовательно идущие день за днем внутри недели или в интервале покажутся
            // в окне уведомлений. Чтобы не накладывать окно предупреждений на окно уведомлений, не показываем их.
            // И так видно в уведомлениях. Но стоит проверить, что такие события показаны в уведомлениях
            Func<EventItemViewModel, bool> allowRemovePredicate = (EventItemViewModel item) =>
            {
                if (item.EventDate == dayAboutTime.Date)
                    return false; // до даты ПЕРВОГО срабатывания сутки, покажем
                if (item.Periodicity == Periodicity.Daily)
                    return true; // напоминание за день о ежедневных не показываем никогда
                if (item.Periodicity == Periodicity.Weekly)
                {
                    // посмотрим, является ли предыдущий день (который только что отработал) днем активации уведомления
                    WeekDaysEnumerator weekDaysEnumerator = new WeekDaysEnumerator(item.WeekDays);
                    return weekDaysEnumerator.IsPreviousInWeekDays(dayAboutTime.DayOfWeek);
                }
                if (item.Range != null)
                    return item.Range.State != DaysRangeState.Dormant; // покажем до входа в интервал
                return false;
            };
            eventItems.RemoveAll(ev => allowRemovePredicate(ev));

            List<ProjectItemViewModel> projectItems = _timerReminderQueue
                .Where(reminder => reminder.Source.Type == ReminderItemType.Project
                && from <= reminder.ReminderDateTime && reminder.ReminderDateTime <= to)
                .Select(reminder => (ProjectItemViewModel)reminder.Source)
                .ToList();

            List<DayAboutItemViewModel> result = new List<DayAboutItemViewModel>();
            result.AddRange(eventItems.Select(source => new DayAboutItemViewModel(source)));
            result.AddRange(projectItems.Select(source => new DayAboutItemViewModel(source)));
            return result;
        }

        private void OnReminderActivated(List<TimerReminderItem> expired)
        {
            if (_reminderWindowViewModel == null || _reminderWindowViewModel.Window == null)
            {
                ReminderWindow window = new ReminderWindow();
                ReminderWindowViewModel context = window.DataContext;
                context.Window = window;
                expired.ForEach(reminder => context.Items.Add(reminder));
                window.SetPosition(ReminderModel.Instance.Settings.NotificationsWindowPosition);
                window.ShowActivated = false;
                window.Show();
                _reminderWindowViewModel = context;
            }
            else
            {
                expired.ForEach(reminder => _reminderWindowViewModel.Items.Add(reminder));
            }

            expired.ForEach(reminder =>
            {
                if (reminder.Source.AllowAutoDeleteOnExpired)
                {
                    ISchedulerGroup group = GroupByType(reminder.Source.Type);
                    group.Remove(reminder.Source);
                    MainViewModel mainViewModel = MainViewModel.FindInstance();
                    mainViewModel.SaveToModel();
                }
            });

            bool wasEvent = expired.Any(item => item.Source.Type == ReminderItemType.Event);
            MainViewModel.FindInstance().OnReminderActivated(wasEvent);
        }

        private IReminderGroup GroupByType(ReminderItemType type)
        {
            switch (type)
            {
                case ReminderItemType.Alarm: return _alarms;
                case ReminderItemType.Event: return _events;
                case ReminderItemType.Project: return _projects;
            }
            throw new ArgumentOutOfRangeException();
        }

        bool IEqualityComparer<DayAboutItemViewModel>.Equals(DayAboutItemViewModel x, DayAboutItemViewModel y)
        {
            return x.Source == y.Source;
        }

        int IEqualityComparer<DayAboutItemViewModel>.GetHashCode(DayAboutItemViewModel obj)
        {
            return obj.Source.GetHashCode();
        }
    }
}
