using System;
using System.Security.Cryptography;
using System.Text;

namespace Reminder.Resources.Cipher
{
    public static class MD5Extensions
    {
        public static Guid ToGuid(this string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            return bytes.ToGuid();
        }

        public static Guid ToGuid(this byte[] bytes)
        {
            using (HashAlgorithm md5 = MD5.Create())
            {
                byte[] guid = md5.ComputeHash(bytes);
                return new Guid(guid);
            }
        }
    }
}
