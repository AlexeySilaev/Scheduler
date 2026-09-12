using System;
using System.Security.Cryptography;
using System.Text;

namespace Reminder.Resources.Cipher
{
    public static class MD5Extensions
    {
        public static Guid ToGuid(this string text)
        {
            using (HashAlgorithm md5 = MD5.Create())
            {
                byte[] guid = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
                return new Guid(guid);
            }
        }
    }
}
