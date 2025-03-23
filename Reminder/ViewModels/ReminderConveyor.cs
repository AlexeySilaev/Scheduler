using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using Reminder.Models;
using Reminder.UI;
using Reminder.Extensions;

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
        private readonly ReminderGroup _events;
        private readonly Action<EventItemViewModel[]> _onConveyorRefresh;
        private readonly Timer _queueTimer;
        private readonly object _lockQueue;
        private readonly List<EventItemViewModel> _dayAboutEvents; // события, которые сработают через сутки

        private Queue<TimerReminderItem> _timerReminderQueue;
        private ReminderWindowViewModel _reminderWindowViewModel;

        // флаг для определения устаревших напоминаний при первом разборе очереди - на запуске
        private bool _isInitializingCheckExpired;

        public ReminderConveyor(AlarmGroup alarms, EventGroup events, Action<EventItemViewModel[]> onConveyorRefresh)
        {
            _alarms = alarms;
            _events = events;
            _onConveyorRefresh = onConveyorRefresh;
            _timerReminderQueue = new Queue<TimerReminderItem>();
            _queueTimer = new Timer();
            _queueTimer.AutoReset = false;
            _queueTimer.Elapsed += QueueTimer_Elapsed;
            _lockQueue = new object();
            _dayAboutEvents = new List<EventItemViewModel>();
            _isInitializingCheckExpired = true;
        }

        /// <summary>
        /// Пересобрать конвейер.
        /// </summary>
        public void Reconstruct()
        {
            lock (_lockQueue)
            {
                List<TimerReminderItem> queue = _alarms.GetTimerReminderItems();
                queue.AddRange(_events.GetTimerReminderItems());
                queue.Sort();

                _timerReminderQueue = new Queue<TimerReminderItem>(queue);
                CheckStartTimer();
            }
        }

        /// <summary>
        /// Остановить конвейер.
        /// </summary>
        public void Stop()
        {
            if (_reminderWindowViewModel?.View != null)
            {
                _reminderWindowViewModel.View.Close();
                _reminderWindowViewModel = null;
            }
        }

        public void OnReminderItemPostponed()
        {
            Reconstruct();
            // если все одноразовые элементы отложены, просматривать больше нечего
            if (_reminderWindowViewModel.IsAllItemsPostponed())
            {
                _reminderWindowViewModel.View.Close();
                _reminderWindowViewModel = null;
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
                if (nearestReminderItem.CheckExpired(e.SignalTime, _isInitializingCheckExpired))
                {
                    List<TimerReminderItem> expired = new List<TimerReminderItem>();
                    while (nearestReminderItem.CheckExpired(e.SignalTime, _isInitializingCheckExpired))
                    {
                        _timerReminderQueue.Dequeue();
                        expired.Add(nearestReminderItem);

                        if (_timerReminderQueue.Any())
                            nearestReminderItem = _timerReminderQueue.Peek();
                        else break;
                    }
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        OnReminderActivated(expired)));

                    Reconstruct();

                }
                else
                    CheckStartTimer();

                _isInitializingCheckExpired = false;

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
        // потому что таймер конвейера тикаеет чаще раза в минуту.
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
            // ежедневные события покажутся в окне уведомлений. Чтобы не накладывать окно предупреждений на окно уведомлений,
            // не показываем их. И так видно в уведомлениях. Но стоит проверить, что такие события показаны в уведомлениях
            result.RemoveAll(ev => ev.Periodicity == Periodicity.Daily && ev.EventDate < dayAboutTime.Date);
            return result;
        }

        private void OnReminderActivated(List<TimerReminderItem> expired)
        {
            if (_reminderWindowViewModel == null || _reminderWindowViewModel.View == null)
            {
                ReminderWindow window = new ReminderWindow();
                ReminderWindowViewModel context = (ReminderWindowViewModel)window.DataContext;
                context.View = window;
                expired.ForEach(reminder => context.Items.Add(reminder));
                window.SetPosition(ReminderModel.Instance.Settings.NotificationsWindowPosition);
                window.Show();
                _reminderWindowViewModel = context;
            }
            else
            {
                expired.ForEach(reminder => _reminderWindowViewModel.Items.Add(reminder));
            }

            if (ReminderModel.Instance.Settings.DeleteCompletedReminders)
                expired.ForEach(reminder =>
                {
                    if (reminder.Source.IsOneTimeReminder)
                    {
                        ReminderGroup group = reminder.Source.Type == ReminderItemType.Alarm ? _alarms : _events;
                        group.Remove(reminder.Source);
                        MainViewModel mainViewModel = MainViewModel.FindInstance();
                        mainViewModel.SaveToModel();
                    }
                });
            if (expired.Any(item => item.Source.Type == ReminderItemType.Event))
            {
                MainViewModel mainViewModel = MainViewModel.FindInstance();
                mainViewModel.OnEventActivated();
            }
        }
    }
}
