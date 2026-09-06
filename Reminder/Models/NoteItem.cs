using System;

namespace Reminder.Models
{
    public class NoteItem : ReminderItem
    {
        public DateTime CreationDate { get; set; }

        public NoteItem()
        {
            CreationDate = TimeProvider.Now;
        }
    }
}
