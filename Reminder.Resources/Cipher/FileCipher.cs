using System;
using System.IO;

namespace Reminder.Resources.Cipher
{
    public class FileCipher : ReminderCipher
    {
        public FileCipher() 
        { 
        }

        public Guid Encrypt(string sourcePath, Func<Guid, string> getTargetPath)
        {
            byte[] bytes = File.ReadAllBytes(sourcePath);
            byte[] encrypted = Encrypt(bytes);
            Guid result = encrypted.ToGuid();
            File.WriteAllBytes(getTargetPath(result), encrypted);
            return result;
        }

        public void Decrypt(string sourcePath, string targetPath)
        {
            byte[] cipherBytes = File.ReadAllBytes(sourcePath);
            byte[] decrypted = Decrypt(cipherBytes);
            File.WriteAllBytes(targetPath, decrypted);
        }
    }
}
