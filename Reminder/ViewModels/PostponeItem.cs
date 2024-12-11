namespace Reminder.ViewModels
{
    public class PostponeItem
    {
        public string Description { get; }
        public int Minutes { get; }

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
}
