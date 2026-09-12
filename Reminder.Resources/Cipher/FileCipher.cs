using System.IO;

namespace Reminder.Resources.Cipher
{
    public class FileCipher : ReminderCipher
    {
        public FileCipher() 
        { 
        }

        public void Encrypt(string sourcePath, string targetPath)
        {
            byte[] bytes = File.ReadAllBytes(sourcePath);
            byte[] encrypted = Encrypt(bytes);
            File.WriteAllBytes(targetPath, encrypted);
        }

        public void Decrypt(string sourcePath, string targetPath)
        {
            byte[] cipherBytes = File.ReadAllBytes(sourcePath);
            byte[] decrypted = Decrypt(cipherBytes);
            File.WriteAllBytes(targetPath, decrypted);
        }
    }
}
