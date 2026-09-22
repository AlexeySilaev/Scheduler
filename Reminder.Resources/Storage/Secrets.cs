using Reminder.Resources.Cipher;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Reminder.Resources.Storage
{
    public class Secrets
    {
        public static Secrets Instance { get; } = new Secrets();

        private readonly StringCipher _stringCipher;
        private readonly FileCipher _fileCipher;

        private StringSecrets _stringSecrets;

        private string SecretsFilePath => Path.Combine(AssemblyContext.SecretsAppDataPath, "strings.xml");
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
            return key;
        }

        public Guid UpdateStringSecret(Guid key, string text)
        {
            string value = _stringCipher.Encrypt(text);
            Guid newKey = value.ToGuid();
            StringSecrets.Update(key, new StringSecret { Key = newKey, Value = value });
            return newKey;
        }

        public bool RemoveStringSecret(Guid key)
        {
            return StringSecrets.Remove(key);
        }

        public bool GetFileSecret(Guid key, string targetPath)
        {
            string fileName = GetFileSecretName(key);
            if (File.Exists(fileName) == false)
                return false;
            _fileCipher.Decrypt(fileName, targetPath);
            return true;
        }

        public Guid AddFileSecret(string path)
        {
            return _fileCipher.Encrypt(path, GetFileSecretName);
        }

        public Guid UpdateFileSecret(string path)
        {
            return _fileCipher.Encrypt(path, GetFileSecretName);
        }

        public bool RemoveFileSecret(Guid key)
        {
            string fileName = GetFileSecretName(key);
            if (File.Exists(fileName))
            {
                DiskHelper.DeleteFile(fileName);
                return true;
            }
            return false;
        }

        public void DeleteExcept(Guid[] stringSecrets, Guid[] fileSecrets)
        {
            StringSecrets.RemoveExcept(stringSecrets);
            if (Directory.Exists(AssemblyContext.SecretsAppDataPath))
            {
                List<string> files = Directory.GetFiles(AssemblyContext.SecretsAppDataPath,
                    $"*.{AssemblyContext.SECRET_FILE_EXTENSION}")
                    .Where(file => fileSecrets.Contains(Guid.Parse(Path.GetFileNameWithoutExtension(file))) == false)
                    .ToList();
                files.ForEach(file => DiskHelper.DeleteFile(file));
            }
        }


        static string GetFileSecretName(Guid key)
        {
            return Path.Combine(Path.Combine(AssemblyContext.AppDataPath, AssemblyContext.SECRETS_SUB_FOLDER),
                Path.ChangeExtension(key.ToString(), AssemblyContext.SECRET_FILE_EXTENSION));
        }

        private Secrets()
        {
            _stringCipher = new StringCipher();
            _fileCipher = new FileCipher();
        }

        private void LoadStringSecrets()
        {
            if (File.Exists(SecretsFilePath) == false)
            {
                _stringSecrets = new StringSecrets(SecretsFilePath);
            }
            else
            {
                XmlSerializer xmlSerializer = XmlSerializer.FromTypes(new[] { typeof(StringSecrets) })[0];
                using (Stream stream = File.Open(SecretsFilePath, FileMode.Open, FileAccess.Read))
                {
                    _stringSecrets = xmlSerializer.Deserialize(stream) as StringSecrets;
                    _stringSecrets.OnDeserialized(SecretsFilePath);
                }
            }
        }

        internal void ImportStringSecrets(StringSecrets importSecrets, bool skipExisting)
        {
            StringSecrets.Import(importSecrets, skipExisting);
        }
    }
}
