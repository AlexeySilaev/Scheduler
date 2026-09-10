using System.IO;

namespace Reminder.Cipher
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

            using (Stream decrypted = Decrypt(cipherBytes))
            using (FileStream fs = new FileStream(targetPath, FileMode.Create))
            {
                decrypted.Position = 0;
                decrypted.CopyTo(fs);
            }
        }
    }
}
