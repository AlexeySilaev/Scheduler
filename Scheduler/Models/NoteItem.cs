using System;

namespace Scheduler.Models
{
    public class NoteItem : SchedulerItem
    {
        public DateTime CreationDate { get; set; }

        public NoteItem()
        {
            CreationDate = DateTime.Now;
        }
    }
}
