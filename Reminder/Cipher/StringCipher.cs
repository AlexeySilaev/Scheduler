using System;
using System.IO;
using System.Text;

namespace Reminder.Cipher
{
    public class StringCipher : ReminderCipher
    {
        public StringCipher()
        {
        }

        public string Encrypt(string plainText)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] result = Encrypt(plainBytes);
            return Convert.ToBase64String(result);
        }

        public string Decrypt(string cipherTextBase64)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherTextBase64);

            using(Stream decrypted = Decrypt(cipherBytes))
            using (var sr = new StreamReader(decrypted))
                return sr.ReadToEnd(); // UTF8 по умолчанию
        }
    }
}
