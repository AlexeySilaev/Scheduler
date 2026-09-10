using System;
using System.IO;

namespace Reminder.Installer.Wizard
{
    internal class FolderBackupCopy : IDisposable
    {
        private readonly string _sourcePath;
        private readonly string _backupPathPath;
        private bool _enableRestoreSource;

        public FolderBackupCopy(string sourcePath) 
        {
            if (Directory.Exists(sourcePath))
            {
                _sourcePath = sourcePath;
                _backupPathPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                CopyAll(new DirectoryInfo(_sourcePath), new DirectoryInfo(_backupPathPath));
            }
        }

        public void Dispose()
        {
            if (Directory.Exists(_backupPathPath))
            { 
                if (_enableRestoreSource)
                {
                    DirectoryInfo source = new DirectoryInfo(_sourcePath);
                    ClearAll(source);
                    CopyAll(new DirectoryInfo(_backupPathPath), source);
                }
                Directory.Delete(_backupPathPath, true);
            }
        }

        public void EnableRestoreSource()
        {
            _enableRestoreSource = true;
        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        private static void ClearAll(DirectoryInfo target)
        {
            foreach (FileInfo file in target.EnumerateFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in target.EnumerateDirectories())
            {
                dir.Delete(true);
            }
        }

    }
}
