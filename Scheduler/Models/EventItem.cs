using System;
using System.ComponentModel;

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
            switch (Periodicity)
            {
                case Periodicity.Daily:
                    DateTime currentDaily = DateTime.Today.AddHours(EventDate.Hour).AddMinutes(EventDate.Minute)
                        .AddSeconds(EventDate.Second);
                    if(currentDaily > DateTime.Now)
                        return currentDaily;
                    return currentDaily.AddDays(1);
                case Periodicity.Weekly:
                    DateTime currentWeekly = DateTime.Today.AddHours(EventDate.Hour).AddMinutes(EventDate.Minute)
                        .AddSeconds(EventDate.Second);
                    // выравниванием текущую дату по границе дня недели
                    int dayShift = currentWeekly.DayOfWeek - EventDate.DayOfWeek;
                    DateTime shiftWeekly = currentWeekly.AddDays(-dayShift);
                    if (shiftWeekly < DateTime.Now)
                        shiftWeekly = shiftWeekly.AddDays(7);
                    return shiftWeekly;
                case Periodicity.Monthly:
                    DateTime currentMonthly = new DateTime(DateTime.Today.Year, DateTime.Today.Month,
                        Math.Min(EventDate.Day, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month)),
                        EventDate.Hour, EventDate.Minute, EventDate.Second);
                    if (currentMonthly > DateTime.Now)
                        return currentMonthly;
                    return currentMonthly.AddMonths(1);
                case Periodicity.Quarterly:
                    DateTime currentQuarterly = new DateTime(DateTime.Today.Year, EventDate.Month,
                        Math.Min(EventDate.Day, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month)),
                        EventDate.Hour, EventDate.Minute, EventDate.Second);
                    if (currentQuarterly > DateTime.Now)
                        return currentQuarterly;
                    return currentQuarterly.AddMonths(3);
                case Periodicity.Annually:
                    DateTime currentAnnually = new DateTime(DateTime.Today.Year, EventDate.Month,
                        Math.Min(EventDate.Day, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month)),
                        EventDate.Hour, EventDate.Minute, EventDate.Second);
                    if (currentAnnually > DateTime.Now)
                        return currentAnnually;
                    return currentAnnually.AddYears(1);
            }
            return EventDate;
        }
    }
}
