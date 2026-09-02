using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Reminder.Models
{
    public class EventItem : ReminderItem
    {
        public DateTime EventDate { get; set; }
        public Periodicity Periodicity { get; set; }
        /// <summary>
        /// Дни недели для еженедельных событий.
        /// </summary>
        [DefaultValue(0)]
        public int WeekDays { get; set; }
        /// <summary>
        /// Интервал напоминания для событий от ежемесячного и реже.
        /// </summary>
        [XmlElement, DefaultValue(null)]
        public DaysRange Range { get; set; }
        public ReminderRingtone Ringtone { get; set; }

        public EventItem()
        {
            EventDate = DateTime.Today.AddDays(1).AddHours(12);
            Periodicity = Periodicity.Once;
            Ringtone = ReminderRingtone.AsIs;
        }
    }

    public enum DaysRangeState
    {
        Dormant,
        Active,
        Broken
    }

    public class DaysRange
    { 
        /// <summary>
        /// Количество отслеживаемых дней.
        /// </summary>
        public int Days { get; set; }
        /// <summary>
        /// Пользователь прервал чреду ежедневных напоминаний.
        /// </summary>
        public DaysRangeState State { get; set; } = DaysRangeState.Dormant;
    }
}
