namespace Reminder.Models
{
    public abstract class ReminderItem 
    {
        public string Text { get; set; }

        protected ReminderItem()
        {
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
