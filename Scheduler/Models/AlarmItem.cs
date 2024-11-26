using System;

namespace Scheduler.Models
{
    internal class AlarmItem : SchedulerItem
    {
        /// <summary>
        /// Время срабатывания будильника.
        /// </summary>
        public TimeSpan AlarmTime { get; set; }

        /// <summary>
        /// Дата сегодняшнего дня. Нужна, если приложение перезапущено.
        /// </summary>
        public DateTime NowDate { get; set; }

        public AlarmItem()
        {
            AlarmTime = TimeSpan.FromMinutes(15);
            NowDate = DateTime.Now;
        }
    }
}
