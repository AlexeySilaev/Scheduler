using System;

namespace Reminder.Models
{
    public class ProjectItem : ReminderItem
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string[] Clauses{ get; set; }

        public ProjectItem()
        {
            StartDate = TimeProvider.Today.AddHours(12);
            Clauses = Array.Empty<string>();
        }

        public bool ShouldSerializeEndDate()
        {
            return EndDate.HasValue;
        }
    }
}
