using Microsoft.Win32;
using Reminder.Models;
using Reminder.Resources;
using Reminder.Resources.Cipher;
using Reminder.Resources.Storage;
using Reminder.Resources.ViewModels;
using Reminder.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Reminder.UI
{
    public class ManagementViewModel : HostContentViewModel, IDisposable
    {
        private readonly ISupportHostContent _secretsHostContent;
        private bool _canExecuteSecretsCommand;
        private string _importFilePath;
        private bool _skipMatchingData;
        private event EventHandler _secretsUnblocked;

        public override string Name { get { return "Управление"; } }

        public bool ShowSecretsGroup 
        {
            get { return ReminderModel.Instance.AreSecretsAllowed; }
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

        public string ExportProcessDescription
        {
            get 
            {
                if (ReminderModel.Instance.Settings.IntroductoryMode)
                    return "Файл экспорта отобразится в проводнике. Впоследствии его можно импортировать на другой машине.";
                return "Сделать резервную копию данных.";
            }
        }
        public ProcessViewModel ExportData { get; }
        public string ImportProcessDescription
        {
            get
            {
                if (ReminderModel.Instance.Settings.IntroductoryMode)
                    return "Сравнение элементов производится по описанию. По завершении операции данные визуально обновятся.";
                return "Восстановить данные из резервной копии.";
            }
        }
        public ProcessViewModel ImportData { get; }
        public Command LockOrUnlockSecrectsCommand { get; }
        public ICommand ExportCommand { get; }
        public Command ImportCommand { get; }
        public ICommand SelectImportFileCommand { get; }

        public event EventHandler SecretsUnblocked
        {
            add { _secretsUnblocked += value; }
            remove { _secretsUnblocked -= value; }
        }

        public ManagementViewModel(ISupportHostContent secretsHostContent)
        { 
            _secretsHostContent = secretsHostContent;
            _secretsHostContent.IsHostContentAllowedChanged += SecretsHostContent_IsHostContentAllowedChanged;
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
            _secretsHostContent.IsHostContentAllowed = !_secretsHostContent.IsHostContentAllowed;
            OnPropertyChanged(nameof(AllowUnblock));
            if (_secretsHostContent.IsHostContentAllowed == false)
                _secretsUnblocked?.Invoke(this, EventArgs.Empty);
        }

        private async void OnExport()
        {
            string exportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                AssemblyContext.EXPORT_DATA_FILE_NAME);
            await ExportData.PeformProcessAsync(exportPath, CreateExportFileAsync,
                () => Process.Start("explorer.exe", $"/select,\"{exportPath}\""));
        }

        private Task CreateExportFileAsync(string exportPath)
        {
            return Task.Factory.StartNew(() =>
            {
                DataExportManager.CreateExportFile(exportPath);
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
            //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.Title = "Файл импорта";
            openFileDialog.Filter = "Файлы для импорта (*.bin)|*.bin|Все файлы (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                ImportFilePath = openFileDialog.FileName;
            }
        }

        void IDisposable.Dispose()
        {
            _secretsHostContent.IsHostContentAllowedChanged -= SecretsHostContent_IsHostContentAllowedChanged;
        }

        private void SecretsHostContent_IsHostContentAllowedChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(AllowUnblock));
        }
    }
}
