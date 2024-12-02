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

    public enum RingtoneMode
    {
        NoSound,
        Default,
        Custom
    }

    public class Settings
    {
        public DeleteSchedulerItemsMode DeleteSchedulerItemsMode { get; set; }
        public NotificationsWindowPosition NotificationsWindowPosition { get; set; }
        public RingtoneMode RingtoneMode { get; set; }
        public string CustomSoundFilePath { get; set; }
        public bool LaunchWithWindows { get; set; }
        public bool MinimizeInTrayOnStart { get; set; }

        public Settings()
        {
            DeleteSchedulerItemsMode = DeleteSchedulerItemsMode.Confirm;
            NotificationsWindowPosition = NotificationsWindowPosition.RightBottom;
            RingtoneMode = RingtoneMode.Default;
            LaunchWithWindows = true;
        }
    }
}
