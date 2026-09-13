using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace Reminder.Resources.Cipher
{
    public class ReminderCipher
    {
        private const int
            KEY_SIZE = 256, // бит
            SALT_SIZE = 32, // байт
            IV_SIZE = 16,   // байт
            ITERATIONS = 100;

        private readonly string _password;

        protected ReminderCipher()
        {
            _password = Convert.ToBase64String(Assembly.GetExecutingAssembly().GetName().GetPublicKey());
        }

        protected byte[] Encrypt(byte[] bytes)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                // Генерируем соль и IV
                byte[] salt = GenerateBytes(rng, SALT_SIZE);
                byte[] iv = GenerateBytes(rng, IV_SIZE);

                // Получаем ключ из пароля с помощью PBKDF2
                using (var pbkdf2 = new Rfc2898DeriveBytes(_password, salt, ITERATIONS, HashAlgorithmName.SHA256))
                {
                    byte[] key = pbkdf2.GetBytes(KEY_SIZE / 8);

                    // Шифрование AES
                    using (var aes = CreateAes(key, iv))
                    using (var encryptor = aes.CreateEncryptor())
                    {
                        using (var ms = new MemoryStream())
                        {
                            // Пишем соль и IV в начало результата (чтобы потом их прочитать)
                            ms.Write(salt, 0, salt.Length);
                            ms.Write(iv, 0, iv.Length);

                            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                            {
                                cs.Write(bytes, 0, bytes.Length);
                            }

                            return ms.ToArray();
                        }
                    }
                }
            }
        }

        protected byte[] Decrypt(byte[] cipherBytes)
        {
            int dataSize = SALT_SIZE + IV_SIZE;
            if (cipherBytes.Length < dataSize)
                throw new ArgumentException("Некорректные зашифрованные данные.");

            // Извлекаем соль и IV из начала массива
            byte[] salt = ExtractFromBuffer(cipherBytes, 0, SALT_SIZE);
            byte[] iv = ExtractFromBuffer(cipherBytes, SALT_SIZE, IV_SIZE);

            // Оставшаяся часть — собственно зашифрованные данные
            byte[] encryptedData = ExtractFromBuffer(cipherBytes, dataSize,
                cipherBytes.Length - dataSize);

            // Восстанавливаем ключ по паролю и соли
            using (var pbkdf2 = new Rfc2898DeriveBytes(_password, salt, ITERATIONS, HashAlgorithmName.SHA256))
            {
                byte[] key = pbkdf2.GetBytes(KEY_SIZE / 8);

                // Дешифрование AES
                using (var aes = CreateAes(key, iv))
                using (var decryptor = aes.CreateDecryptor())
                using (var ms = new MemoryStream(encryptedData))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var result = new MemoryStream())
                {
                    cs.CopyTo(result);
                    return result.ToArray();
                }
            }
        }

        private static Aes CreateAes(byte[] key, byte[] iv)
        {
            Aes result = Aes.Create();
            result.Key = key;
            result.IV = iv;
            result.Mode = CipherMode.CBC;
            result.Padding = PaddingMode.PKCS7;
            return result;
        }

        private static byte[] GenerateBytes(RandomNumberGenerator rng, int size)
        {
            byte[] result = new byte[size];
            rng.GetBytes(result);
            return result;
        }

        private static byte[] ExtractFromBuffer(byte[] buffer, int from, int size)
        {
            byte[] result = new byte[size];
            Array.Copy(buffer, from, result, 0, size);
            return result;
        }
    }
}
