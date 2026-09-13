using System;
using System.IO;
using System.IO.IsolatedStorage;

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

        public Guid Encrypt(string sourcePath, Func<Guid, IsolatedStorageFileStream> targetStreamCreator)
        {
            byte[] bytes = File.ReadAllBytes(sourcePath);
            byte[] encrypted = Encrypt(bytes);
            Guid result = encrypted.ToGuid();
            using (IsolatedStorageFileStream targetStream = targetStreamCreator(result))
                targetStream.Write(encrypted, 0, encrypted.Length);
            return result;
        }

        public void Decrypt(string sourcePath, string targetPath)
        {
            byte[] cipherBytes = File.ReadAllBytes(sourcePath);
            byte[] decrypted = Decrypt(cipherBytes);
            File.WriteAllBytes(targetPath, decrypted);
        }

        public void Decrypt(IsolatedStorageFileStream sourceStream, string targetPath)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                sourceStream.CopyTo(ms);
                byte[] cipherBytes = ms.ToArray();
                byte[] decrypted = Decrypt(cipherBytes);
                File.WriteAllBytes(targetPath, decrypted);
            }
        }

    }
}
