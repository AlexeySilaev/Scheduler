using Microsoft.Win32;
using Reminder.Installer.Wizard;
using Reminder.Cipher;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Xml;

namespace Reminder.Installer
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        private const string APP_NAME = "AV Reminder";

        private string _title;
        private string _installFolderPath;
        private string _unlockSecretsPassword;
        private bool _desktopLink;
        private bool _launchApplication;
        private bool _showPassword;
        private Visibility _passwordTipVisibility;
        private Guid? _prevUnlockKey;
        private int _installCounter;
        private string _installError;

        private PropertyChangedEventHandler _propertyChanged;

        private Guid? PrevUnlockKey
        {
            get
            {
                if (_prevUnlockKey == Guid.Empty)
                {
                    _prevUnlockKey = null;
                    XmlDocument prevDoc = GetXmlData(out _);
                    XmlNode prevPassword = prevDoc.SelectSingleNode("/Data/Settings/UnlockSecretsKey");
                    if (string.IsNullOrEmpty(prevPassword?.InnerText) == false)
                        if (Guid.TryParse(prevPassword.InnerText, out Guid result))
                            _prevUnlockKey = result;
                }
                return _prevUnlockKey;
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                if (Title == value) return;
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public string InstallFolderPath
        {
            get { return _installFolderPath; }
            set
            {
                if (InstallFolderPath == value) return;
                _installFolderPath = value;
                OnPropertyChanged(nameof(InstallFolderPath));
            }
        }

        public string UnlockSecretsPassword
        {
            get { return _unlockSecretsPassword; }
            set
            {
                if (UnlockSecretsPassword == value) return;
                _unlockSecretsPassword = value;
                OnPropertyChanged(nameof(UnlockSecretsPassword));
                UpdatePasswordTipVisibility();
            }
        }

        public bool DesktopLink
        {
            get { return _desktopLink; }
            set
            {
                if (DesktopLink == value) return;
                _desktopLink = value;
                OnPropertyChanged(nameof(DesktopLink));
            }
        }

        public bool LaunchApplication
        {
            get { return _launchApplication; }
            set
            {
                if (LaunchApplication == value) return;
                _launchApplication = value;
                OnPropertyChanged(nameof(LaunchApplication));
            }
        }

        public string EyeImage
        {
            get
            {
                if (_showPassword)
                    return "/Resources/eye_opened.png";
                else
                    return "/Resources/eye_closed.png";
            }
        }
        public string AppDataFolderPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), APP_NAME);
            }
        }

        public bool IsInstalling
        {
            get { return Interlocked.CompareExchange(ref _installCounter, 0, 0) != 0; }
        }

        public bool ShowPassword
        {
            get { return _showPassword; }
        }

        public Visibility PasswordTipVisibility
        {
            get { return _passwordTipVisibility; }
            private set
            {
                if (PasswordTipVisibility == value) return;
                _passwordTipVisibility = value;
                OnPropertyChanged(nameof(PasswordTipVisibility));
            }
        }

        public Visibility InstallingVisibility
        {
            get { return IsInstalling ? Visibility.Visible : Visibility.Hidden; }
        }

        public string InstallError
        {
            get { return _installError; }
            set
            {
                if (InstallError == value) return;
                _installError = value;
                OnPropertyChanged(nameof(InstallError));
                OnPropertyChanged(nameof(InstallErrorVisibility));
            }
        }

        public Visibility InstallErrorVisibility
        {
            get { return string.IsNullOrEmpty(InstallError) ? Visibility.Hidden : Visibility.Visible; }
        }


        public NavigationService NavigationService { get; set; }

        public ICommand BrowseFolderCommand { get; }

        public ICommand NextInstallCommand { get; }
        public ICommand NextSecretsCommand { get; }
        public ICommand NextFinishCommand { get; }

        public ICommand BackParametersCommand { get; }
        public ICommand BackSecretsCommand { get; }

        public ICommand CompleteCommand { get; }

        public MainWindowViewModel()
        {
            _installFolderPath = GetInstallFolderPath();
            _launchApplication = true;
            _passwordTipVisibility = Visibility.Hidden;
            _prevUnlockKey = Guid.Empty;
            BrowseFolderCommand = new CommandImpl(ExecuteBrowseFolder);
            NextSecretsCommand = new CommandImpl(ExecuteGoSecrets);
            NextInstallCommand = new CommandImpl(ExecuteNextInstall);
            BackParametersCommand = new CommandImpl(ExecuteBackParameters);
            BackSecretsCommand = new CommandCanExecuteImpl(ExecuteGoSecrets, CanNavigateFromInstall);
            NextFinishCommand = new CommandCanExecuteImpl(ExecuteNextFinish, CanNavigateFromInstall);
            CompleteCommand = new CommandImpl(ExecuteComplete);
        }

        public bool ToggleShowPassword()
        { 
            _showPassword = !ShowPassword;
            OnPropertyChanged(nameof(EyeImage));
            return _showPassword;
        }

        private string GetInstallFolderPath()
        {
            XmlDocument doc = GetXmlData(out _);
            XmlNode installedFolder = doc.SelectSingleNode("/Data/Settings/InstalledFolder");
            if (Directory.Exists(installedFolder?.InnerText))
                return installedFolder?.InnerText; // предлагаем обновить установленное приложение

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), APP_NAME);
        }

        private void ExecuteBrowseFolder()
        {
            using (FolderBrowserDialog d = new FolderBrowserDialog())
            {
                d.Description = "Укажите папку установки";
                d.SelectedPath = RemoveSubFolder(InstallFolderPath);
                d.ShowNewFolderButton = true;
                if (d.ShowDialog() == DialogResult.OK)
                {
                    InstallFolderPath = AppendSubFolder(d.SelectedPath);
                }
            }
        }

        private string AppendSubFolder(string path)
        {
            if (Path.GetFileName(path) != APP_NAME)
                path = Path.Combine(path, APP_NAME);
            return path;
        }

        private string RemoveSubFolder(string path)
        {
            if (Path.GetFileName(path) == APP_NAME)
                path = Path.GetDirectoryName(path);
            return path;
        }

        private void ExecuteGoSecrets()
        {
            NavigationService.Navigate(new Uri("/Wizard/SecretsPage.xaml", UriKind.Relative));
        }

        private void ExecuteNextInstall()
        {
            NavigationService.Navigate(new Uri("/Wizard/InstallPage.xaml", UriKind.Relative));
        }

        private bool CanNavigateFromInstall()
        {
            return IsInstalling == false;
        }

        private void ExecuteBackParameters()
        {
            NavigationService.Navigate(new Uri("/Wizard/ParametersPage.xaml", UriKind.Relative));
        }

        // если установка производится поверх прежней версии, нужна ее резерная копия
        // в случае ошибки установки будет произведен откат
        private async void ExecuteNextFinish()
        {
            InstallError = null;
            Interlocked.Increment(ref _installCounter);
            RaiseCanExecuteChanged(BackSecretsCommand);
            RaiseCanExecuteChanged(NextFinishCommand);
            OnPropertyChanged(nameof(InstallingVisibility));

            using (FolderBackupCopy programFilesBackup = new FolderBackupCopy(InstallFolderPath),
                appDataBackup = new FolderBackupCopy(AppDataFolderPath))
            {
                try
                {
                    await UnzipContentAsync();
                    string icoPath = await PrepareAppDataFolder();
                    await PrepareUninstaller(icoPath);
                    await SetAutoRun();
                    CheckCreateShortcut();
                    NavigationService.Navigate(new Uri("/Wizard/FinishPage.xaml", UriKind.Relative));
                }
                catch (Exception x)
                {
                    programFilesBackup.EnableRestoreSource();
                    appDataBackup.EnableRestoreSource();
                    InstallError = x.Message;
                }
                finally
                {
                    Interlocked.Decrement(ref _installCounter);
                    RaiseCanExecuteChanged(BackSecretsCommand);
                    RaiseCanExecuteChanged(NextFinishCommand);
                    OnPropertyChanged(nameof(InstallingVisibility));
                }
            }
        }

        private static void RaiseCanExecuteChanged(ICommand command)
        {
            CommandCanExecuteImpl canExecuteCommand = (CommandCanExecuteImpl)command;
            canExecuteCommand.RaiseCanExecuteChanged();
        }

        private Task UnzipContentAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                var reminders = Process.GetProcessesByName("AvReminder");
                if (reminders.Any())
                {
                    // достаточно первого, он один
                    Process avReminder = reminders.First();
                    if (avReminder.WaitForExit(3000) == false) // Ждем до 3 секунды
                        avReminder.Kill();
                }

                EnsureFolderExists(InstallFolderPath);
                using (Stream contentStream = Assembly.GetExecutingAssembly()
                           .GetManifestResourceStream("Reminder.Installer.Resources.Content.zip"))
                {
                    using (var zip = Package.Open(contentStream))
                    {
                        foreach (PackagePart part in zip.GetParts())
                        {
                            string target = Path.GetFullPath(Path.Combine(InstallFolderPath, part.Uri.OriginalString.TrimStart('/')));
                            if (File.Exists(target)) 
                            {
                                // отработать случай повторной установки в ту же папку
                                File.Delete(target);
                            }

                            using (Stream source = part.GetStream(FileMode.Open, FileAccess.Read))
                            {
                                FileStream targetFile = File.OpenWrite(target);
                                source.CopyTo(targetFile);
                                targetFile.Close();
                            }
                        }
                    }
                }
            });
        }

        private static void EnsureFolderExists(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                string parentFolderPath = Path.GetDirectoryName(folderPath);
                EnsureFolderExists(parentFolderPath); // для возможного понижения версии .NET
                Directory.CreateDirectory(folderPath);
            }
        }

        /// <summary>
        /// Минимальная подготовка. Папка есть - хорошо, нет - будет. Перенесем иконку для деинсталлятора.
        /// Проверим пароль разблокировки для секретов. Остальное выполнит само приложение.
        /// </summary>
        private Task<string> PrepareAppDataFolder()
        {
            return Task.Factory.StartNew(() =>
            {
                EnsureFolderExists(AppDataFolderPath);
                ConfigureUnlockSecretsPassword();

                const string icoName = "App.ico";
                string icoPath = Path.Combine(InstallFolderPath, icoName);
                string result = Path.Combine(AppDataFolderPath, icoName);
                File.Copy(icoPath, result, true);
                File.Delete(icoPath);
                return result;
            });
        }

        private XmlDocument GetXmlData(out string dataPath)
        {
            dataPath = Path.Combine(AppDataFolderPath, "data.xml");
            XmlDocument result = new XmlDocument();
            if (File.Exists(dataPath))
                result.Load(dataPath);
            return result;
        }

        private void ConfigureUnlockSecretsPassword()
        {
            Guid? unlockSecretsKey = string.IsNullOrEmpty(UnlockSecretsPassword) ?
                (Guid?)null : UnlockSecretsPassword.ToGuid();
            XmlDocument doc = GetXmlData(out string dataPath);
            if (doc.DocumentElement != null)
            {
                // XML загружен
                XmlNode secrets = doc.SelectSingleNode("/Data/Secrets");
                if (secrets == null)
                    doc.DocumentElement.AppendChild(doc.CreateElement("Secrets"));

                XmlNode settings = doc.SelectSingleNode("/Data/Settings");
                if (settings == null)
                {
                    // невероятный случай, когда файл поврежден настолько, что в нем нет настроек.
                    // выйдем. Пусть восстановится при следующем запуске клиента.
                    return;
                }
                XmlNode password = doc.SelectSingleNode("/Data/Settings/UnlockSecretsKey");
                if (unlockSecretsKey != null)
                {
                    if (password != null)
                        password.InnerText = unlockSecretsKey.ToString();
                    else
                    {
                        password = doc.CreateElement("UnlockSecretsKey");
                        password.InnerText = unlockSecretsKey.ToString();
                        if (settings != null)
                            settings.AppendChild(password);
                    }
                }
                else if (password != null)
                    settings.RemoveChild(password);
            }
            else
            {
                // пустой шаблон с ключом разблокировки секретов
                XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", null, null);
                doc.AppendChild(xmlDeclaration);
                XmlElement root = doc.CreateElement("Data");
                doc.AppendChild(root);
                XmlElement alarms = doc.CreateElement("Alarms");
                root.AppendChild(alarms);
                XmlElement events = doc.CreateElement("Events");
                root.AppendChild(events);
                XmlElement notes = doc.CreateElement("Notes");
                root.AppendChild(notes);
                XmlElement projects = doc.CreateElement("Projects");
                root.AppendChild(projects);
                XmlElement secrets = doc.CreateElement("Secrets");
                root.AppendChild(secrets);
                XmlElement settings = doc.CreateElement("Settings");
                root.AppendChild(settings);
                if (unlockSecretsKey != null)
                {
                    XmlElement password = doc.CreateElement("UnlockSecretsKey");
                    password.InnerText = unlockSecretsKey.ToString();
                    settings.AppendChild(password);
                }
            }
            doc.Save(dataPath);
        }

        private Task PrepareUninstaller(string icoPath)
        {
            return Task.Factory.StartNew(() =>
            {
                using (RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall", true))
                {
                    using (RegistryKey av = rk.CreateSubKey(APP_NAME))
                    {
                        av.SetValue("DisplayName", APP_NAME, RegistryValueKind.String);
                        av.SetValue("DisplayIcon", InQuotation(icoPath), RegistryValueKind.String);
                        av.SetValue("Publisher", "Alexey Silaev");
                        av.SetValue("UninstallString", InQuotation(Path.Combine(InstallFolderPath, "uninst000.exe")), RegistryValueKind.String);
                    }
                }
            });
        }

        private Task SetAutoRun()
        {
            return Task.Factory.StartNew(() =>
            {
                using (RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    rk.SetValue("AvReminder", InQuotation(Path.Combine(InstallFolderPath, "AvReminder.exe")), RegistryValueKind.String);
                }
            });
        }

        private static string InQuotation(string content)
        {
            return $"\"{content}\"";
        }

        private void CheckCreateShortcut()
        {
            if (DesktopLink == false) return;
            string desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            IWshRuntimeLibrary.IWshShell wsh = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)wsh.CreateShortcut(Path.Combine(desktopFolder, "AvReminder.lnk"));
            shortcut.WindowStyle = 1; //for default window, 3 for maximize, 7 for minimize
            shortcut.TargetPath = Path.Combine(InstallFolderPath, "AvReminder.exe");
            shortcut.Description = APP_NAME;
            shortcut.Save();
        }

        private void UpdatePasswordTipVisibility()
        {
            if (string.IsNullOrEmpty(UnlockSecretsPassword))
                PasswordTipVisibility = Visibility.Hidden;
            else if (PrevUnlockKey.HasValue)
            {
                Guid newUnlockKey = UnlockSecretsPassword.ToGuid();
                PasswordTipVisibility = PrevUnlockKey.Value == newUnlockKey
                    ? Visibility.Hidden : Visibility.Visible;
            }
            else
                PasswordTipVisibility = Visibility.Hidden;
        }

        private void ExecuteComplete()
        {
            string executablePath = Path.Combine(InstallFolderPath, "AvReminder.exe");
            if (LaunchApplication && File.Exists(executablePath))
                Process.Start(new ProcessStartInfo
                {
                    FileName = executablePath,
                    Verb = "runas" // админ админом, прилага прилагой
                });
            System.Windows.Application.Current.Shutdown();
        }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if(_propertyChanged != null)
                _propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region CommandImpl
        private class CommandImpl : ICommand
        {
            private readonly Action _execute;
            protected EventHandler _canExecuteChanged;

            public CommandImpl(Action execute)
            {
                _execute = execute;
            }

            protected virtual bool CanExecute()
            {
                return true;
            }

            event EventHandler ICommand.CanExecuteChanged
            {
                add { _canExecuteChanged += value; }
                remove { _canExecuteChanged -= value; }
            }

            bool ICommand.CanExecute(object parameter)
            {
                return CanExecute();
            }

            void ICommand.Execute(object parameter)
            {
                _execute();
            }
        }

        private class CommandCanExecuteImpl : CommandImpl
        {
            private readonly Func<bool> _canExecute;

            public CommandCanExecuteImpl(Action execute, Func<bool> canExecute)
                : base(execute)
            {
                _canExecute = canExecute;
            }

            public void RaiseCanExecuteChanged()
            {
                if(_canExecuteChanged != null)
                    _canExecuteChanged(this, EventArgs.Empty);
            }

            protected override bool CanExecute()
            {
                return _canExecute();
            }
        }

        #endregion CommandImpl
    }
}
