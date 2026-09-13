using System;
using System.Text;

namespace Reminder.Resources.Cipher
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
            byte[] decrypted = Decrypt(cipherBytes);
            return Encoding.UTF8.GetString(decrypted);
        }
    }
}
