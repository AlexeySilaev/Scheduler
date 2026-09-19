using Reminder.Resources.Cipher;
using System.IO;

namespace Reminder.Resources.Storage
{
    public static class DataExportManager
    {
        public static void CreateExportFile(string exportPath)
        {
            string zip = Path.GetTempFileName();
            try
            {
                ZipPackage package = new ZipPackage(zip, AssemblyContext.AppDataPath);
                string[] filters = { "*.xml", $"*.{AssemblyContext.SECRET_FILE_EXTENSION}" };
                package.Zip(filters);

                BitwiseScrambler scrambler = new BitwiseScrambler();
                scrambler.Scramble(zip, exportPath);
            }
            finally
            {
                File.Delete(zip);
            }
        }
    }
}
