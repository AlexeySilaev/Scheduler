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
            DateTime nearestDate = DateTime.Today > EventDate.Date ? DateTime.Today : EventDate.Date;
            switch (Periodicity)
            {
                case Periodicity.Daily:
                    DateTime currentDaily = nearestDate.AddHours(EventDate.Hour).AddMinutes(EventDate.Minute)
                        .AddSeconds(EventDate.Second);
                    if(currentDaily > DateTime.Now)
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
                    DateTime currentMonthly = new DateTime(nearestDate.Year, nearestDate.Month,
                        Math.Min(EventDate.Day, DateTime.DaysInMonth(nearestDate.Year, nearestDate.Month)),
                        EventDate.Hour, EventDate.Minute, EventDate.Second);
                    if (currentMonthly > DateTime.Now)
                        return currentMonthly;
                    return currentMonthly.AddMonths(1);
                case Periodicity.Quarterly:
                    DateTime currentQuarterly = new DateTime(nearestDate.Year, EventDate.Month,
                        Math.Min(EventDate.Day, DateTime.DaysInMonth(nearestDate.Year, nearestDate.Month)),
                        EventDate.Hour, EventDate.Minute, EventDate.Second);
                    if (currentQuarterly > DateTime.Now)
                        return currentQuarterly;
                    return currentQuarterly.AddMonths(3);
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
    }
}
