using Microsoft.Win32;
using Reminder.Models;
using Reminder.Resources;
using Reminder.Resources.Cipher;
using Reminder.Resources.Storage;
using Reminder.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Reminder.UI
{
    public class ManagementViewModel : BaseViewModel
    {
        private readonly ISupportHostContent _secretsHostContent;
        private bool _canExecuteSecretsCommand;
        private string _importFilePath;
        private bool _skipMatchingData;

        public bool ShowSecretsGroup 
        {
            get { return ReminderModel.Instance.Settings.UnlockSecretsKey != Guid.Empty; }
        }

        public bool AllowUnblock
        {
            get { return _secretsHostContent.IsHostContentAllowed; }
        }

        public string ImportFilePath
        {
            get { return _importFilePath; }
            private set
            {
                if (ImportFilePath == value) return;
                _importFilePath = value;
                OnPropertyChanged(nameof(ImportFilePath));
                ImportCommand.RaiseCanExecuteChanged();
            }
        }

        public bool SkipMatchingData
        {
            get { return _skipMatchingData; }
            set
            {
                if (SkipMatchingData == value) return;
                _skipMatchingData = value;
                OnPropertyChanged(nameof(SkipMatchingData));
            }
        }

        public ProcessViewModel ExportData { get; }
        public ProcessViewModel ImportData { get; }
        public Command LockOrUnlockSecrectsCommand { get; }
        public ICommand ExportCommand { get; }
        public Command ImportCommand { get; }
        public ICommand SelectImportFileCommand { get; }

        public ManagementViewModel(ISupportHostContent secretsHostContent)
        { 
            _secretsHostContent = secretsHostContent;
            _skipMatchingData = true;
            ExportData = new ProcessViewModel("Выполняется экспорт данных...");
            ImportData = new ProcessViewModel("Выполняется импорт данных...");
            LockOrUnlockSecrectsCommand = new Command(OnLockOrUnlockSecrects, CanExecuteSecretsCommand);
            ExportCommand = new Command(OnExport);
            ImportCommand = new Command(OnImport, CanExecuteImportCommand);
            SelectImportFileCommand = new Command(OnSelectImportFile);
        }

        public void RefreshBindings()
        { 
            OnPropertyChanged(nameof(SkipMatchingData));
        }

        internal void OnSecretsPasswordChanged(string secretsPassword)
        {
            _canExecuteSecretsCommand = ReminderModel.Instance.Settings.UnlockSecretsKey == secretsPassword.ToGuid();
            LockOrUnlockSecrectsCommand.RaiseCanExecuteChanged();
        }

        private bool CanExecuteSecretsCommand()
        {
            return _canExecuteSecretsCommand;
        }

        private void OnLockOrUnlockSecrects()
        {
            _secretsHostContent.SetHostContentAllowed(_secretsHostContent.IsHostContentAllowed);
            OnPropertyChanged(nameof(AllowUnblock));
        }

        private async void OnExport()
        {
            string exportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "avreminder.bin");
            await ExportData.PeformProcessAsync(exportPath, CreateExportFileAsync,
                () => Process.Start("explorer.exe", $"/select,\"{exportPath}\""));
        }

        private Task CreateExportFileAsync(string exportPath)
        {
            return Task.Factory.StartNew(() =>
            {
                string zip = Path.GetTempFileName();
                try
                {
                    ZipPackage package = new ZipPackage(zip, AssemblyContext.AppDataPath);
                    string[] filters = { "*.xml", "*.crpt" };
                    package.Zip(filters);

                    BitwiseScrambler scrambler = new BitwiseScrambler();
                    scrambler.Scramble(zip, exportPath);
                }
                finally
                { 
                    File.Delete(zip);
                }
            });
        }

        private bool CanExecuteImportCommand()
        {
            return string.IsNullOrEmpty(ImportFilePath) == false;
        } 

        private async void OnImport()
        {
            await ImportData.PeformProcessAsync(SkipMatchingData, PerformImportDataAsync, null);
            if (ImportData.Status == ProcessResult.Ok)
            {
                MainViewModel mainViewModel = MainViewModel.FindInstance();
                mainViewModel.OnImportCompleted();
            }
        }

        private Task PerformImportDataAsync(bool skipMatchingData)
        {
            return Task.Factory.StartNew(() =>
            {
                if (File.Exists(ImportFilePath) == false)
                    throw new FileNotFoundException();
                DataImport import = new ReminderDataImport(ImportFilePath, skipMatchingData);
                import.Execute();
            });
        }

        private void OnSelectImportFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.Title = "Файл импорта";
            openFileDialog.Filter = "Файлы для импорта (*.bin)|*.bin|Все файлы (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                ImportFilePath = openFileDialog.FileName;
            }
        }
    }
}
