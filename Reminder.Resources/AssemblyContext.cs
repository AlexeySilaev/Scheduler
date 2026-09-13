using System;
using System.IO;

namespace Reminder.Resources
{
    public static class AssemblyContext
    {
        public const string APP_NAME = "AV Reminder";
        public static string AppDataPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), APP_NAME);
            }
        }
    }
}
