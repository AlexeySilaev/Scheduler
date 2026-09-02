using System;

namespace Reminder.Models
{
    [Serializable]
    public class Data
    {
        public Data()
        {
        }

        public AlarmItem[] Alarms { get; set; }
        public EventItem[] Events { get; set; }
        public NoteItem[] Notes { get; set; }
        public ProjectItem[] Projects { get; set; }
        public Settings Settings { get; set; }
    }
}
