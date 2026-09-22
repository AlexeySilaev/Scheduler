using System;
using System.IO;

namespace Reminder.Resources.Storage
{
    public static class DiskHelper
    {
        public static void DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (UnauthorizedAccessException)
            {
                FileAttributes attributes = File.GetAttributes(path);
                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    // снимем и попробуем удалить заново
                    attributes &= ~FileAttributes.ReadOnly;
                    File.SetAttributes(path, attributes);
                    File.Delete(path);
                }
                else
                {
                    throw;
                }
            }
        }

        public static void DeleteFolder(string path, bool recursive)
        {
            try
            {
                Directory.Delete(path, recursive);
            }
            catch (UnauthorizedAccessException)
            {
                FileAttributes attributes = File.GetAttributes(path);
                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    attributes &= ~FileAttributes.ReadOnly; // Снимите атрибут
                    File.SetAttributes(path, attributes);
                    Directory.Delete(path, recursive); // Попробуйте удалить снова
                }
                else
                {
                    throw; // Перебросьте другое исключение, если причина не в ReadOnly
                }
            }
        }

    }
}
