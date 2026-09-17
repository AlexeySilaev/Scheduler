using Reminder.Resources.Cipher;
using System.IO;
using System.Xml.Serialization;

namespace Reminder.Resources.Storage
{
    public class DataImport
    {
        private readonly string _importFilePath;
        protected readonly bool _skipExisting;

        public DataImport(string importFilePath, bool skipExisting)
        { 
            _importFilePath = importFilePath;
            _skipExisting = skipExisting;
        }

        // Последовательность действий: 1) распаковка, 2) копирование зашифрованных файлов, 
        // 3) объединение зашифрованных строк, 4) объединение элементов в группах напоминалки,
        // 5) чистка битых ссылок на секреты.
        public void Execute()
        {
            string unpackFolderPath = Path.Combine(Path.GetTempPath(), "AvReminder.Import");
            if (Directory.Exists(unpackFolderPath))
                Directory.Delete(unpackFolderPath, true);
            Directory.CreateDirectory(unpackFolderPath);

            try
            {
                Unpack(unpackFolderPath);
                CopyFileSecrets(unpackFolderPath);
                MergeData(unpackFolderPath);
                ClearUnusedData();
            }
            finally
            {
                Directory.Delete(unpackFolderPath, true);
            }
        }

        private void Unpack(string unpackFolderPath)
        {
            string zip = Path.GetTempFileName();
            try
            {
                BitwiseScrambler scrambler = new BitwiseScrambler();
                scrambler.Scramble(_importFilePath, zip);

                ZipPackage package = new ZipPackage(zip, unpackFolderPath);
                package.Unzip(false);
            }
            finally
            {
                File.Delete(zip);
            }
        }

        private void CopyFileSecrets(string unpackFolderPath)
        {
            if (Directory.Exists(AssemblyContext.SecretsAppDataPath) == false)
                return; // данный пользователь не имеет группы секретов

            string importSecretsPath = Path.Combine(unpackFolderPath, AssemblyContext.SECRETS_SUB_FOLDER);
            if (Directory.Exists(importSecretsPath) == false)
                return;

            string[] secretFiles = Directory.GetFiles(importSecretsPath, $"*.{AssemblyContext.SECRET_FILE_EXTENSION}");
            foreach (string secretFile in secretFiles)
            {
                string destFilePath = Path.Combine(AssemblyContext.SecretsAppDataPath, Path.GetFileName(secretFile));
                if (File.Exists(destFilePath))
                {
                    if (_skipExisting == false)
                        File.Copy(secretFile, destFilePath, true);
                }
                else
                    File.Copy(secretFile, destFilePath);
            }
        }

        // этот метод знает только о строках-секретах.
        // об остальном позаботятся более компетентные
        protected virtual void MergeData(string unpackFolderPath)
        {
            if (Directory.Exists(AssemblyContext.SecretsAppDataPath) == false)
                return; // данный пользователь не имеет группы секретов

            string importSecretsFolder = Path.Combine(Path.Combine(unpackFolderPath, AssemblyContext.SECRETS_SUB_FOLDER));
            if (Directory.Exists(importSecretsFolder) == false)
                return;

            string importSecretsPath = Path.Combine(importSecretsFolder, AssemblyContext.SECRET_STRINS_FILE_NAME);
            XmlSerializer xmlSerializer = XmlSerializer.FromTypes(new[] { typeof(StringSecrets) })[0];
            using (Stream stream = File.Open(importSecretsPath, FileMode.Open, FileAccess.Read))
            {
                StringSecrets importSecrets = xmlSerializer.Deserialize(stream) as StringSecrets;
                Secrets.Instance.ImportStringSecrets(importSecrets, _skipExisting);
            }
        }

        protected virtual void ClearUnusedData()
        {
            // здесь синхронизировать нечего
        }
    }
}
