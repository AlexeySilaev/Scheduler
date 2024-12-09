using System;

namespace Scheduler.Models
{
    public class AlarmItem : SchedulerItem
    {
        /// <summary>
        /// Время срабатывания будильника в минутах.
        /// </summary>
        public int AlarmTimeMinutes { get; set; }

        /// <summary>
        /// Дата сегодняшнего дня. Нужна, если приложение перезапущено.
        /// </summary>
        public DateTime NowDate { get; set; }

        public ReminderRingtone Ringtone { get; set; }

        public AlarmItem()
        {
            AlarmTimeMinutes = 15;
            NowDate = DateTime.Now;
            Ringtone = ReminderRingtone.AsIs;
        }
    }
}
