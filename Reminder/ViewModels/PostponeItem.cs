using Reminder.Resources.ViewModels;
using System;
using System.Windows.Input;

namespace Reminder.ViewModels
{
    public class PostponeItem : BaseViewModel
    {
        public string Description { get; }
        public int Minutes { get; protected set; }
        public virtual bool AllowPostponeToAlarms { get { return true; } }

        public PostponeItem(string description, int minutes)
        {
            Description = description;
            Minutes = minutes;
        }

        public override string ToString()
        {
            return Description;
        }
    }

    public class CompleteItem : PostponeItem
    {
        public static CompleteItem Instance { get; }

        static CompleteItem()
        {
            Instance = new CompleteItem();
        }

        private CompleteItem() : base(" (нет) ", 0)
        {
        }
    }

    /// <summary>
    /// Время назначает пользователь.
    /// </summary>
    public class OtherItem : PostponeItem
    {
        public static int DefaultMinites { get; set; } = 10;

        private TimeSpan _time;

        public TimeSpan Time
        {
            get { return _time; }
            set
            {
                if (Time == value) return;
                _time = value;
                OnPropertyChanged(nameof(Time));
                Minutes = Convert.ToInt32(Time.TotalMinutes);
                DefaultMinites = Minutes;
            }
        }

        public ICommand Accept { get; }

        public OtherItem(Command accept) : base("(Другое)", DefaultMinites)
        {
            _time = TimeSpan.FromMinutes(Minutes);
            Accept = accept;
        }
    }

    /// <summary>
    /// Для событий с активным интервалом отложить на сутки.
    /// </summary>
    public class ContinueIntervalItem : PostponeItem
    {
        public static ContinueIntervalItem Instance { get; } 

        static ContinueIntervalItem()
        {
            Instance = new ContinueIntervalItem();
        }

        public override bool AllowPostponeToAlarms { get { return false; } } // элемент "Завтра" пересчитается в событиях

        private ContinueIntervalItem() : base("Завтра", 1440)
        {
        }
    }

    /// <summary>
    /// Цель достугнута, на сутки не переносим.
    /// </summary>
    public class BreakIntervalItem : PostponeItem
    {
        public static BreakIntervalItem Instance { get; }

        static BreakIntervalItem()
        {
            Instance = new BreakIntervalItem();
        }

        private BreakIntervalItem() : base("Отмена", 0)
        {
        }
    }

    /// <summary>
    /// Перенести дату окончания проекта.
    /// </summary>
    public class ProjectEndDateItem : PostponeItem
    {
        public override bool AllowPostponeToAlarms { get { return false; } }
        public DateTime EndDate { get; }

        public ProjectEndDateItem(DateTime endDate) : base(endDate.ToShortDateString(), 0)
        {
            EndDate = endDate;
        }
    }

}
