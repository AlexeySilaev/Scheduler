using System;

namespace Scheduler.Models
{
    internal class NoteItem : SchedulerItem
    {
        public DateTime CreationDate { get; set; }

        public NoteItem()
        {
            CreationDate = DateTime.Now;
        }
    }
}
