using System.IO;
using System.Reflection;

namespace Scheduler.Resources
{
    public static class EmbeddedResource
    {
        public const string DefaultRingtoneFileName = "bell.mp3";

        public static Stream GetDefaultRingtoneFile()
        {
            var names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(
                $"Scheduler.Resources.{DefaultRingtoneFileName}");
        }
    }
}
