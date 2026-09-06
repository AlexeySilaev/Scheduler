using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using Reminder.Models;
using Reminder.UI;
using Reminder.Extensions;
using System.Threading.Tasks;

namespace Reminder.ViewModels
{
 /* Работа алгоритма.
    Для напоминаний и событий вычисляется ближайшее время срабатывания (в случае напоминаний и одноразовых событий
    оно единственно). Это вреия инкапсулируется элементами TimerReminderItem, они сортируются по возрастанию времени
    срабатывания ReminderDateTime и ставятся на конвейер _timerReminderQueue. После того, как наступает ближайшее
    срабатывание, очередь перестраивается, чтобы обновить ближайшее время срабатывания для периодических событий.
    Например, для сработавшего ежедевного события в очереди появится новый элемент, который будет обработан через сутки. */

    internal class ReminderConveyor
    {
        private const double SLOW_TIMER_INTERVAL = 1000 * 30;
        private const double FAST_TIMER_INTERVAL = 1000 * 0.5;

        private readonly AlarmGroup _alarms;
        private readonly EventGroup _events;
        private readonly Action<EventItemViewModel[]> _onConveyorRefresh;
        private readonly Timer _queueTimer;
        private readonly object _lockQueue;
        private readonly List<EventItemViewModel> _dayAboutEvents; // события, которые сработают через сутки
        private readonly List<TimerReminderItem> _timerReminderQueue;

        private ReminderWindowViewModel _reminderWindowViewModel;

        public ReminderConveyor(AlarmGroup alarms, EventGroup events, Action<EventItemViewModel[]> onConveyorRefresh)
        {
            _alarms = alarms;
            _events = events;
            _onConveyorRefresh = onConveyorRefresh;
            _timerReminderQueue = new List<TimerReminderItem>();
            _queueTimer = new Timer();
            _queueTimer.AutoReset = false;
            _queueTimer.Elapsed += QueueTimer_Elapsed;
            _lockQueue = new object();
            _dayAboutEvents = new List<EventItemViewModel>();
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
                _reminderWindowViewModel.Window?.Close(); // Window? если ловкий пользователь закрыл окно
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
                EventItemViewModel[] newDayAboutEvents =
                    updated ? _dayAboutEvents.ToArray() : Array.Empty<EventItemViewModel>();
                Application.Current.Dispatcher.BeginInvoke(new Action(() => _onConveyorRefresh(newDayAboutEvents)));
            }
        }

        // Отслеживаем события, которые отработают ровно через сутки
        // Если событие отработает минутой меньше, чем через сутки, оно устарело, удаляем его,
        // оно уже отправлено во всплывающее окно подсказки.
        // Если событии уже имеется в коллекции, ничего не делаем, оно было добавлено чуть ранее,
        // потому что таймер конвейера тикает чаще раза в минуту.
        private bool UpdateDayAboutEvents(DateTime signalTime)
        {
            List<EventItemViewModel> currentDayEvents = GetDayAboutEvents(signalTime.AddDays(1));
            bool result = Enumerable.SequenceEqual(_dayAboutEvents, currentDayEvents) == false;
            _dayAboutEvents.Clear();
            _dayAboutEvents.AddRange(currentDayEvents);
            return result;
        }

        private List<EventItemViewModel> GetDayAboutEvents(DateTime dayAboutTime)
        {
            // выравниваем время срабатывания по границе минуты
            DateTime from = dayAboutTime.Date.AddHours(dayAboutTime.Hour).AddMinutes(dayAboutTime.Minute);
            DateTime to = from.AddSeconds(59);
            List<EventItemViewModel> result = _timerReminderQueue
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
            result.RemoveAll(ev => allowRemovePredicate(ev));
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
                if (reminder.Source.IsOneTimeReminder)
                {
                    ISchedulerGroup group = reminder.Source.Type == ReminderItemType.Alarm ?
                       (ISchedulerGroup)_alarms : (ISchedulerGroup)_events;
                    group.Remove(reminder.Source);
                    MainViewModel mainViewModel = MainViewModel.FindInstance();
                    mainViewModel.SaveToModel();
                }
            });

            bool wasEvent = expired.Any(item => item.Source.Type == ReminderItemType.Event);
            MainViewModel.FindInstance().OnReminderActivated(wasEvent);
        }
    }
}
