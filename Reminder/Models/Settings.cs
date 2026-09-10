using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using Reminder.Extensions;
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
        /// <summary>
        /// При запуске приложения удалять просроченные напоминания и одноразовые события.
        /// </summary>
        public bool DeleteExpiredRemindersOnStart { get; set; }
        /// <summary>
        /// Для событий, наступление которых от текущего момента больше суток, предупредить за день
        /// об их наступлении.
        /// </summary>
        public bool ShowComingDayAboutEvents { get; set; }
        public bool IntroductoryMode { get; set; }
        // ↓↓↓ служебные настройки, наружу не видны ↓↓↓
        public bool IsFirstMinimize { get; set; }
        public string InstalledFolder { get; set; } // если приложение уже установлено, инсталлятор покажет эту папку
        // хэш пароля для разблокировки секретов
        [DefaultValue(null)]
        public Guid? UnlockSecretsKey { get; set; }
        public double MainWindowLeft { get; set; }
        public double MainWindowTop { get; set; }
        public double MainWindowHeight { get; set; }
        public double MainWindowWidth { get { return 730; } }
        public double MainWindowMinHeight { get { return 485; } }
        // ↑↑↑ окончание блока служебных настроек ↑↑↑

        public Settings()
        {
            DeleteReminderItemsMode = DeleteReminderItemsMode.Confirm;
            NotificationsWindowPosition = NotificationsWindowPosition.RightBottom;
            RingtoneMode = RingtoneMode.Default;
            LaunchWithWindows = true;
            MinimizeToTray = true;
            IntroductoryMode = true;
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

        /// <summary>
        /// Проверить, что окно полностью умещается на экран. При необходимости отцентровать.
        /// </summary>
        public void ValidateMainWindowBounds()
        {
            bool setCentered = false;
            if (MainWindowHeight == 0)
            {
                // предполагаем первый запуск, настройки еще не сохранены на диск
                MainWindowHeight = MainWindowMinHeight;
                setCentered = true;
            }
            Rect windowBounds = new Rect(MainWindowLeft, MainWindowTop, MainWindowWidth, MainWindowHeight);
            Point location = WindowPositioning.GetValidLocation(windowBounds, setCentered);
            MainWindowLeft = location.X;
            MainWindowTop = location.Y;
        }
    }
}
