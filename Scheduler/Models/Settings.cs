using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Models
{
    public enum DeleteSchedulerItemsMode
    {
        Confirm,
        Immediately
    }

    public enum NotificationsWindowPosition
    {
        Center,
        RightBottom
    }

    public class Settings
    {
        public DeleteSchedulerItemsMode DeleteSchedulerItemsMode { get; set; }
        public NotificationsWindowPosition NotificationsWindowPosition { get; set; }

        public Settings()
        {
            DeleteSchedulerItemsMode = DeleteSchedulerItemsMode.Immediately;
            NotificationsWindowPosition = NotificationsWindowPosition.RightBottom;
        }
    }
}
