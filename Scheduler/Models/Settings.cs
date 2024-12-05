using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Scheduler.Resources;

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
        public bool MinimizeToTray { get; set; }
        public bool TopMostNotificationsWindow { get; set; }

        public Settings()
        {
            DeleteSchedulerItemsMode = DeleteSchedulerItemsMode.Confirm;
            NotificationsWindowPosition = NotificationsWindowPosition.RightBottom;
            RingtoneMode = RingtoneMode.Default;
            LaunchWithWindows = true;
            MinimizeToTray = true;
            TopMostNotificationsWindow = true;
        }

        public string GetDefaultRingtoneFile()
        {
            const string resourceName = "bell.mp3";
            string result = Path.Combine(SchedulerModel.AppDataPath, resourceName);
            if (File.Exists(result))
                return result;
            using (FileStream fileStream = File.Create(result))
            {
                string appName = Path.GetFileNameWithoutExtension(Application.ResourceAssembly.Location);
                EmbeddedResource.GetDefaultRingtoneFile().CopyTo(fileStream);
            }
            return result;
        }

    }
}
