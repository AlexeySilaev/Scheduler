using Reminder.Resources.Cipher;
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml.Serialization;

namespace Reminder.Resources.Storage
{
    public class Secrets
    {
        public static Secrets Instance { get; } = new Secrets();

        private readonly StringCipher _stringCipher;
        private readonly FileCipher _fileCipher;

        private IsolatedStorageFile _storage;
        private StringSecrets _stringSecrets;

        private IsolatedStorageFile Storage
        {
            get
            {
                if (_storage == null)
                    Initialize();
                return _storage;
            }
        }

        private StringSecrets StringSecrets
        {
            get
            {
                if (_stringSecrets == null)
                    LoadStringSecrets();
                return _stringSecrets;
            }
        }

        public string GetStringSecret(Guid key)
        {
            string result = StringSecrets.Get(key);
            if (result != null)
                result = _stringCipher.Decrypt(result);
            return result;
        }

        public Guid AddStringSecret(string text)
        { 
            string value = _stringCipher.Encrypt(text);
            Guid key = value.ToGuid();
            StringSecrets.Add(new StringSecret { Key = key, Value = value });
            SaveStringSecrets();
            return key;
        }

        public Guid UpdateStringSecret(Guid key, string text)
        {
            string value = _stringCipher.Encrypt(text);
            Guid newKey = value.ToGuid();
            StringSecrets.Update(key, new StringSecret { Key = newKey, Value = value });
            SaveStringSecrets();
            return newKey;
        }

        public bool RemoveStringSecret(Guid key)
        {
            bool result = StringSecrets.Remove(key);
            SaveStringSecrets();
            return result;
        }

        public bool GetFileSecret(Guid key, string targetPath)
        {
            string fileName = GetFileSecretName(key);
            if (Storage.FileExists(fileName) == false)
                return false;
            using (IsolatedStorageFileStream stream = Storage.OpenFile(fileName, FileMode.Open, FileAccess.Read))
            {
                _fileCipher.Decrypt(stream, targetPath);
                return true;
            }
        }

        public Guid AddFileSecret(string path)
        {
            return _fileCipher.Encrypt(path, key =>
            {
                string fileName = GetFileSecretName(key);
                return Storage.OpenFile(fileName, FileMode.CreateNew, FileAccess.Write);
            });
        }

        public Guid UpdateFileSecret(string path)
        {
            return _fileCipher.Encrypt(path, key =>
            {
                string fileName = GetFileSecretName(key);
                return Storage.OpenFile(fileName, FileMode.Create, FileAccess.Write);
            });
        }

        public bool RemoveFileSecret(Guid key)
        {
            string fileName = GetFileSecretName(key);
            if (Storage.FileExists(fileName))
            { 
                Storage.DeleteFile(fileName);
                return true;
            }
            return false;
        }

        static string GetFileSecretName(Guid key)
        {
            return Path.ChangeExtension(key.ToString(), "crpt");
        }

        private Secrets()
        {
            _stringCipher = new StringCipher();
            _fileCipher = new FileCipher();
        }

        ~Secrets()
        {
            if (_storage != null)
            {
                _storage.Dispose();
                _storage = null;
            }
        }

        private void Initialize()
        {
            _storage = IsolatedStorageFile.GetUserStoreForAssembly();
        }

        private void LoadStringSecrets()
        {
            if (Storage.FileExists(StringSecrets.FILE_NAME) == false)
            {
                using (Stream stream = Storage.OpenFile(StringSecrets.FILE_NAME, FileMode.CreateNew, FileAccess.Write))
                {
                    _stringSecrets = new StringSecrets();
                    _stringSecrets.Save(stream);
                }
            }
            else
            {
                XmlSerializer xmlSerializer = XmlSerializer.FromTypes(new[] { typeof(StringSecrets) })[0];
                using (Stream stream = Storage.OpenFile(StringSecrets.FILE_NAME, FileMode.Open, FileAccess.Read))
                {
                    _stringSecrets = xmlSerializer.Deserialize(stream) as StringSecrets;
                }
            }
        }

        private void SaveStringSecrets()
        {
            using (Stream stream = Storage.OpenFile(StringSecrets.FILE_NAME, FileMode.Create, FileAccess.Write))
            {
                StringSecrets.Save(stream);
            }
        }
    }
}
