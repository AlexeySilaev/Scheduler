namespace Scheduler.Models
{
    public abstract class SchedulerItem 
    {
        public string Text { get; set; }

        protected SchedulerItem()
        {
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
