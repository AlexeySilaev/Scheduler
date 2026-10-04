using Reminder.Resources.Storage;
using System;
using System.IO;

namespace Reminder.Installer.Wizard
{
    internal class FolderBackupCopy : IDisposable
    {
        private readonly DirectoryInfo _sourcePath;
        private readonly string _backupPathPath;
        private bool _enableRestoreSource;

        public FolderBackupCopy(string sourcePath) 
        {
            if (Directory.Exists(sourcePath))
            {
                _sourcePath = new DirectoryInfo(sourcePath);
                _backupPathPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                CopyAll(_sourcePath, new DirectoryInfo(_backupPathPath));
            }
        }

        public void Dispose()
        {
            if (Directory.Exists(_backupPathPath))
            { 
                if (_enableRestoreSource)
                {
                    ClearAll(_sourcePath);
                    CopyAll(new DirectoryInfo(_backupPathPath), _sourcePath);
                }
                DiskHelper.DeleteFolder(_backupPathPath, true);
            }
        }

        public void EnableRestoreSource()
        {
            _enableRestoreSource = true;
        }

        public static void ClearAll(DirectoryInfo target)
        {
            foreach (FileInfo file in target.EnumerateFiles())
            {
                DiskHelper.DeleteFile(file.FullName);
            }
            foreach (DirectoryInfo dir in target.EnumerateDirectories())
            {
                DiskHelper.DeleteFolder(dir.FullName, true);
            }
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
    }
}
