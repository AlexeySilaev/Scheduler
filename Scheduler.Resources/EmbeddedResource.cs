using System.IO;
using System.Reflection;

namespace Reminder.Resources
{
    public static class EmbeddedResource
    {
        public const string DefaultRingtoneFileName = "bell.mp3";

        public static Stream GetDefaultRingtoneFile()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(
                $"Reminder.Resources.{DefaultRingtoneFileName}");
        }
    }
}
