using System;
using System.IO;

namespace Reminder.Resources
{
    public static class AssemblyContext
    {
        public const string
            APP_NAME = "AV Reminder",
            EXPORT_DATA_FILE_NAME = "avreminder.bin";
        internal const string
            SECRETS_SUB_FOLDER = "Secrets",
            SECRET_FILE_EXTENSION = "crpt",
            SECRET_STRINS_FILE_NAME = "strings.xml";

        public static string AppDataPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), APP_NAME);
            }
        }
        public static string SecretsAppDataPath { get { return Path.Combine(AppDataPath, SECRETS_SUB_FOLDER); } }
    }
}
