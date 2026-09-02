using System;
using System.Collections.Generic;

namespace Reminder.Models
{
    /// <summary>
    /// Получение всех дней недели из комбинации битовых флагов.
    /// </summary>
    internal class WeekDaysEnumerator
    {
        private readonly int _weekDays;

        public WeekDaysEnumerator(int weekDays) 
        { 
            _weekDays = weekDays;
        }

        public IEnumerable<int> GetEnumerable(DayOfWeek firstWeekDay)
        {
            int from = Convert.ToInt32(firstWeekDay);
            if (InWeekDays(from))
            {
                Queue<int> queue = new Queue<int>(new int[] { 0, 1, 2, 3, 4, 5, 6 });
                // выстраиваем в порядке, начиная с from
                while (queue.Peek() != from)
                    queue.Enqueue(queue.Dequeue());
                // отбираем подходящие
                while (queue.Count > 0)
                {
                    int current = queue.Dequeue();
                    if (InWeekDays(current))
                        yield return current;
                }
            }
            else
                yield return from; // повреждено хранилище, вернем только начальный
        }

        public bool IsPreviousInWeekDays(DayOfWeek current)
        { 
            int previousDay = (Convert.ToInt32(current) + 6) % 7;
            return InWeekDays(previousDay);
        }

        private bool InWeekDays(int day)
        { 
            int raisedInPower = Convert.ToInt32(Math.Pow(2, day));
            return (_weekDays & raisedInPower) != 0;
        }
    }
}
