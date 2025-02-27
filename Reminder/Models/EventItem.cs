using System;

namespace Reminder.Models
{
    public class EventItem : ReminderItem
    {
        public DateTime EventDate { get; set; }
        public Periodicity Periodicity { get; set; }
        public ReminderRingtone Ringtone { get; set; }

        public EventItem()
        {
            EventDate = DateTime.Today.AddDays(1).AddHours(12);
            Periodicity = Periodicity.Once;
            Ringtone = ReminderRingtone.AsIs;
        }

        public DateTime GetNearestEventDate()
        {
            DateTime nearestDate = DateTime.Today > EventDate.Date ? DateTime.Today : EventDate.Date;
            switch (Periodicity)
            {
                case Periodicity.Daily:
                    DateTime currentDaily = nearestDate.AddHours(EventDate.Hour).AddMinutes(EventDate.Minute)
                        .AddSeconds(EventDate.Second);
                    if (currentDaily > DateTime.Now)
                        return currentDaily;
                    return currentDaily.AddDays(1);
                case Periodicity.Weekly:
                    DateTime currentWeekly = nearestDate.AddHours(EventDate.Hour).AddMinutes(EventDate.Minute)
                        .AddSeconds(EventDate.Second);
                    // выравниванием текущую дату по границе дня недели 
                    int dayShift = currentWeekly.DayOfWeek - EventDate.DayOfWeek;
                    DateTime shiftWeekly = currentWeekly.AddDays(-dayShift);
                    if (shiftWeekly < DateTime.Now)
                        shiftWeekly = shiftWeekly.AddDays(7);
                    return shiftWeekly;
                case Periodicity.Monthly:
                    return GetPeriodicalMothDate(nearestDate, 1);
                case Periodicity.Quarterly:
                    return GetPeriodicalMothDate(nearestDate, 3);
                case Periodicity.EverySixMonths:
                    return GetPeriodicalMothDate(nearestDate, 6);
                case Periodicity.Annually:
                    DateTime currentAnnually = new DateTime(nearestDate.Year, EventDate.Month,
                        Math.Min(EventDate.Day, DateTime.DaysInMonth(nearestDate.Year, nearestDate.Month)),
                        EventDate.Hour, EventDate.Minute, EventDate.Second);
                    if (currentAnnually > DateTime.Now)
                        return currentAnnually;
                    return currentAnnually.AddYears(1);
            }

            return EventDate;
        }

        private DateTime GetPeriodicalMothDate(DateTime nearestDate, int months)
        {
            DateTime currentNearest = new DateTime(nearestDate.Year, nearestDate.Month,
                Math.Min(EventDate.Day, DateTime.DaysInMonth(nearestDate.Year, nearestDate.Month)),
                EventDate.Hour, EventDate.Minute, EventDate.Second);
            if (currentNearest > DateTime.Now)
                return currentNearest;
            return currentNearest.AddMonths(months);
        }
    }
}
