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
                    return GetNearestPeriodicalMothDate(nearestDate, 1);
                case Periodicity.Quarterly:
                    return GetNearestPeriodicalMothDate(nearestDate, 3);
                case Periodicity.EverySixMonths:
                    return GetNearestPeriodicalMothDate(nearestDate, 6);
                case Periodicity.Annually:
                    return GetNearestPeriodicalMothDate(nearestDate, 12);
            }

            return EventDate;
        }

        private DateTime GetNearestPeriodicalMothDate(DateTime nearestDate, int months)
        {
            if (EventDate > DateTime.Now)
                return EventDate;
            // сколько месяцев между месяцем даты события и месяцем текущей даты?
            int monthInterval = DateTime.Today.Month - EventDate.Month;
            if (monthInterval < 0)
                monthInterval = 12 + monthInterval;
            // попадаем ли мы в monthInterval с шагом months без остатка?
            int additionalMonths = 0;
            if (monthInterval > months)
                additionalMonths = monthInterval % months;
            else
                additionalMonths = months - monthInterval;

            DateTime template = DateTime.Today.AddMonths(additionalMonths).AddHours(EventDate.Hour).AddMinutes(EventDate.Minute).AddMilliseconds(EventDate.Second);
            DateTime result = new DateTime(template.Year, template.Month, Math.Min(EventDate.Day, DateTime.DaysInMonth(template.Year, template.Month)),
                template.Hour, template.Minute, template.Second);
            if (result < DateTime.Now)
                result = result.AddMonths(months);
            return result;
        }
    }
}
