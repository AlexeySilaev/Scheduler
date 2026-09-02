using Reminder.Models;
using Reminder.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Reminder.Extensions
{
	internal static class EventItemViewModelExtensions
	{
        public static DateTime GetNearestEventDate(this EventItemViewModel viewModel,
            DateTime eventDate, int weekDays, DaysRangeViewModel range)
        {
            switch (viewModel.Periodicity)
            {
                case Periodicity.Daily:
                    return GetNearestPeriodicalDailyDate(eventDate);
                case Periodicity.Weekly:
                    return GetNearestPeriodicalWeekDate(eventDate, weekDays);
                case Periodicity.Monthly:
                    return GetNearestPeriodicalMonthDate(eventDate, range, 1);
                case Periodicity.Quarterly:
                    return GetNearestPeriodicalMonthDate(eventDate, range, 3);
                case Periodicity.EverySixMonths:
                    return GetNearestPeriodicalMonthDate(eventDate, range, 6);
                case Periodicity.Annually:
                    return GetNearestPeriodicalMonthDate(eventDate, range, 12);
            }

            return viewModel.FullEventDate;
        }

        private static DateTime GetNearestPeriodicalDailyDate(DateTime eventDate)
        {
            if (DateTime.Now < eventDate)
                return eventDate;
            DateTime result = DateTime.Today.AddHours(eventDate.Hour)
                .AddMinutes(eventDate.Minute).AddSeconds(eventDate.Second);
            if (result < DateTime.Now)
                result = result.AddDays(1);
            return result;
        }

        private static DateTime GetNearestPeriodicalWeekDate(DateTime eventDate, int weekDays)
        {
            // метод определения ближайшей даты: вычислить все в интервале недели и взять наименьшую
            var days = new WeekDaysEnumerator(weekDays).GetEnumerable(eventDate.DayOfWeek);
            DateTime baseWeekDate = eventDate.AddDays(-days.First());
            List<DateTime> nearestWeekDates = new List<DateTime>();
            days.ForEach(day => nearestWeekDates.Add(GetNearestPeriodicalWeekDate(baseWeekDate, day)));
            nearestWeekDates.Sort();
            return nearestWeekDates.First();

            // получение ближайшей даты по дню недели
            DateTime GetNearestPeriodicalWeekDate(DateTime initialWeekDate, int dayOfWeek)
            {
                DateTime weekEventDate = initialWeekDate.AddDays(dayOfWeek);
                DateTime nearestDate = DateTime.Today > weekEventDate.Date ? DateTime.Today : weekEventDate.Date;
                DateTime currentWeekly = nearestDate.AddHours(weekEventDate.Hour).AddMinutes(weekEventDate.Minute)
                    .AddSeconds(weekEventDate.Second);
                // выравниванием текущую дату по границе дня недели 
                int dayShift = currentWeekly.DayOfWeek - weekEventDate.DayOfWeek;
                DateTime shiftWeekly = currentWeekly.AddDays(-dayShift);
                if (shiftWeekly < DateTime.Now)
                    shiftWeekly = shiftWeekly.AddDays(7);
                return shiftWeekly;
            }
        }

        private static DateTime GetNearestPeriodicalMonthDate(DateTime eventDate, DaysRangeViewModel range, int months)
        {
            DateTime now = DateTime.Now;
            if (eventDate > now)
            {
                range?.Clear();
                return eventDate;
            }
            // сколько месяцев между месяцем даты события и месяцем текущей даты?
            int monthInterval = eventDate.Month - DateTime.Today.Month;
            if (monthInterval < 0)
                monthInterval = 12 + monthInterval;
            if (months == 1)
            {
                // ежемесячное событие пока считаем в текущем месяце
                // если дата уже прошла, позже она будет скорректирована
                monthInterval = 0;
            }

            DateTime template = DateTime.Today
                .AddMonths(monthInterval)
                .AddHours(eventDate.Hour)
                .AddMinutes(eventDate.Minute)
                .AddMilliseconds(eventDate.Second);
            DateTime result = new DateTime(template.Year, template.Month,
                Math.Min(eventDate.Day, DateTime.DaysInMonth(template.Year, template.Month)),
                template.Hour, template.Minute, template.Second);

            // поправка на интервал напоминания
            if (range != null)
            {
                // если у нас был отрицательный monthInterval
                bool checkPrevMonth = eventDate.Month - DateTime.Today.Month == -1;
                result = range.CheckApplyRange(result, now, checkPrevMonth);
            }

            // что, если событие приходится на текущий месяц, но дата дня уже прошла?
            if (result < now)
                result = result.AddMonths(months);
            return result;
        }

    }
}
