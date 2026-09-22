using Microsoft.Win32;
using Reminder.Launch;
using Reminder.Resources;
using Reminder.Resources.Storage;
using Reminder.Resources.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Reminder.UnInstaller
{
    internal class MainWindowViewModel : BaseViewModel
    {
        private bool _exportData;
        private string _exportFilePath;

        public bool IsUninslalling { get; private set; }
        public bool IsExportDataEnabled { get { return !IsUninslalling; } }
        public bool ExportData
        {
            get { return _exportData; }
            set
            {
                if (ExportData == value) return;
                _exportData = value;
                OnPropertyChanged(nameof(ExportData));
                if (ExportData == false)
                    ExportFilePath = null;
                SelectExportDataFileCommand.RaiseCanExecuteChanged();
            }
        }
        public string ExportFilePath
        {
            get { return _exportFilePath; }
            set
            {
                if (ExportFilePath == value) return;
                _exportFilePath = value;
                OnPropertyChanged(nameof(ExportFilePath));
            }
        }

        public Command SelectExportDataFileCommand { get; }
        public Command UninstallCommand { get; }
        public Command CancelCommand { get; }

        public MainWindowViewModel()
        {
            SelectExportDataFileCommand = new Command(OnSelectExportDataFile, () => !IsUninslalling && ExportData);
            UninstallCommand = new Command(OnUninstall, () => !IsUninslalling);
            CancelCommand = new Command(OnCancel, () => !IsUninslalling);
        }

        private void OnSelectExportDataFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Файл резервной копии";
            saveFileDialog.FileName = AssemblyContext.EXPORT_DATA_FILE_NAME;

            if (saveFileDialog.ShowDialog() == true)
                ExportFilePath = saveFileDialog.FileName;
        }

        private async void OnUninstall()
        {
            IsUninslalling = true;
            OnPropertyChanged(nameof(IsExportDataEnabled));
            SelectExportDataFileCommand.RaiseCanExecuteChanged();
            UninstallCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();

            await Task.Run(() =>
            {
                try
                {
                    StopAvReminder();
                    ClearRegistry();
                    ExportAppData();
                    DeleteAppData();
                    DeleteShortcut();
                    if (File.Exists(ExportFilePath))
                        Process.Start("explorer.exe", $"/select,\"{ExportFilePath}\"");
                }
                finally
                {
                    SelfDelete();
                }
            });
        }

        private static void StopAvReminder()
        {
            try
            {
                ClientProcessProxy proxy = new ClientProcessProxy();
                proxy.Shutdown();
                proxy.Close();
            }
            catch (CommunicationException)
            {
                try
                {
                    var reminders = Process.GetProcessesByName("AvReminder");
                    if (reminders.Any())
                        reminders[0].Kill(); // достаточно первого, он один
                }
                catch (Win32Exception)
                { 
                    // отказано в доступе, процесс завершен
                }
            }
        }

        private static void ClearRegistry()
        {
            try
            {
                using (RegistryKey uninst =
                       Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\",
                           true))
                {
                    uninst.DeleteSubKeyTree(AssemblyContext.APP_NAME);
                }

                RegistryKey rk =
                    Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (rk != null)
                {
                    rk.DeleteValue("AvReminder", false);
                    rk.Close();
                }
            }
            catch (ArgumentException)
            {
                // страховка на редкий случай, если кто-то баловался и руками удалил
            }
        }

        private void ExportAppData()
        {
            if (string.IsNullOrEmpty(ExportFilePath)) return;
            DataExportManager.CreateExportFile(ExportFilePath);
        }

        private static void DeleteAppData()
        {
            string appDataFolder = AssemblyContext.AppDataPath;
            if (Directory.Exists(appDataFolder))
                DiskHelper.DeleteFolder(appDataFolder, true);
        }

        private static void DeleteShortcut()
        {
            string shortcutPath =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "AvReminder.lnk");
            if (File.Exists(shortcutPath))
                DiskHelper.DeleteFile(shortcutPath);
        }

        private static void SelfDelete()
        {
            using (Stream stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Reminder.UnInstaller.batchCommand.bin"))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // некоторым антивирусам не нравится, когда файл удаляет сам себя
                    // поднимем текст команды из ресурсов
                    stream.CopyTo(memoryStream);
                    byte[] bytes = memoryStream.ToArray();
                    string commandFormat = Encoding.UTF8.GetString(bytes);

                    ProcessStartInfo psi = new ProcessStartInfo("cmd.exe",
                        String.Format(commandFormat, AppDomain.CurrentDomain.BaseDirectory));
                    psi.WindowStyle = ProcessWindowStyle.Hidden;
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;
                    Process.Start(psi);
                    Environment.Exit(0);
                }
            }
        }

        private void OnCancel()
        {
            Application.Current.Shutdown();
        }
    }
}
