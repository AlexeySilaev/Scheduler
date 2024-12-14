using System.IO;
using System.Windows;
using Reminder.Resources;

namespace Reminder.Models
{
    public class Settings
    {
        public DeleteReminderItemsMode DeleteReminderItemsMode { get; set; }
        public NotificationsWindowPosition NotificationsWindowPosition { get; set; }
        public RingtoneMode RingtoneMode { get; set; }
        public string CustomSoundFilePath { get; set; }
        public bool LaunchWithWindows { get; set; }
        public bool MinimizeToTray { get; set; }
        public bool TopMostNotificationsWindow { get; set; }
        // служебные настройки, наружу не видны
        public bool IsFirstMinimize { get; set; }
        public string InstalledFolder { get; set; } // если приложение уже установлено, инсталлятор покажет эту папку

        public Settings()
        {
            DeleteReminderItemsMode = DeleteReminderItemsMode.Confirm;
            NotificationsWindowPosition = NotificationsWindowPosition.RightBottom;
            RingtoneMode = RingtoneMode.Default;
            LaunchWithWindows = true;
            MinimizeToTray = true;
            TopMostNotificationsWindow = true;
            IsFirstMinimize = true;
        }

        public string GetDefaultRingtoneFile()
        {
            string result = Path.Combine(ReminderModel.AppDataPath, EmbeddedResource.DefaultRingtoneFileName);
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
