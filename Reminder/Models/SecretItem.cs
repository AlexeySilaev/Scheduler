using System;
using System.ComponentModel;

namespace Reminder.Models
{
    public class SecretItem : ReminderItem
    {
        public int ViewCounter { get; set; }
        public SecretKind Kind { get; set; }
        public Guid Value { get; set; }

        public SecretItem()
        {
            Kind = SecretKind.String;
        }
    }

    public enum SecretKind
    {
        [Description("Строка")] String,
        [Description("Файл")] File
    }
}
