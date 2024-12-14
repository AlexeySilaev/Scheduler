using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Reminder.Installer
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        private const string INSTALL_SUBFOLDER = "AV Reminder";

        private string _title;
        private string _installFolderPath;
        private bool _desktopLink;
        private bool _launchApplication;
        private int _installCounter;
        private string _installError;

        private PropertyChangedEventHandler _propertyChanged;

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

        public bool IsInstalling
        {
            get { return Interlocked.CompareExchange(ref _installCounter, 0, 0) != 0; }
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

        public ICommand BackParametersCommand { get; }
        public ICommand NextFinishCommand { get; }

        public ICommand CompleteCommand { get; }

        public MainWindowViewModel()
        {
            _installFolderPath = GetInstallFolderPath();
            _launchApplication = true;
            BrowseFolderCommand = new CommandImpl(ExecuteBrowseFolder);
            NextInstallCommand = new CommandImpl(ExecuteNextInstall);
            BackParametersCommand = new CommandCanExecuteImpl(ExecuteBackParameters, CanNavigateFromInstall);
            NextFinishCommand = new CommandCanExecuteImpl(ExecuteNextFinish, CanNavigateFromInstall);
            CompleteCommand = new CommandImpl(ExecuteComplete);
        }

        private string GetInstallFolderPath()
        {
            string appDataFolderPath =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), INSTALL_SUBFOLDER);
            string dataPath = Path.Combine(appDataFolderPath, "data.xml");
            if (File.Exists(dataPath))
            {
                string content = File.ReadAllText(dataPath);
                string pattern = @"<InstalledFolder>(.*?)</InstalledFolder>";
                var regex = new Regex(pattern);
                var matches = regex.Matches(content);
                if (matches.Count > 0)
                {
                    string result = matches[0].Groups[1].Value;
                    if (Directory.Exists(result))
                        return result; // предлагаем обновить установленное приложение
                }
            }

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), INSTALL_SUBFOLDER);
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
            if (Path.GetFileName(path) != INSTALL_SUBFOLDER)
                path = Path.Combine(path, INSTALL_SUBFOLDER);
            return path;
        }

        private string RemoveSubFolder(string path)
        {
            if (Path.GetFileName(path) == INSTALL_SUBFOLDER)
                path = Path.GetDirectoryName(path);
            return path;
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

        private async void ExecuteNextFinish()
        {
            InstallError = null;
            Interlocked.Increment(ref _installCounter);
            RaiseCanExecuteChanged(BackParametersCommand);
            RaiseCanExecuteChanged(NextFinishCommand);
            OnPropertyChanged(nameof(InstallingVisibility));
            try
            {
                await UnzipContentAsync();
                CheckCreateShortcut();
                NavigationService.Navigate(new Uri("/Wizard/FinishPage.xaml", UriKind.Relative));
            }
            catch (Exception x)
            {
                InstallError = x.Message;
            }
            finally
            {
                Interlocked.Decrement(ref _installCounter);
                RaiseCanExecuteChanged(BackParametersCommand);
                RaiseCanExecuteChanged(NextFinishCommand);
                OnPropertyChanged(nameof(InstallingVisibility));
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
                EnsureFolderExists(InstallFolderPath);
                using (Stream contentStream = Assembly.GetExecutingAssembly()
                           .GetManifestResourceStream("Reminder.Installer.Resources.Content.zip"))
                {
                    using (var zip = Package.Open(contentStream))
                    {
                        foreach (PackagePart part in zip.GetParts())
                        {
                            string target = Path.GetFullPath(Path.Combine(InstallFolderPath, part.Uri.OriginalString.TrimStart('/')));
                            if(File.Exists(target)) // отработать случай повторной установки в ту же папку
                                File.Delete(target);

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
                EnsureFolderExists(parentFolderPath);
                Directory.CreateDirectory(folderPath);
            }
        }

        private void CheckCreateShortcut()
        {
            if (DesktopLink == false) return;
            string desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            IWshRuntimeLibrary.IWshShell wsh = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)wsh.CreateShortcut(Path.Combine(desktopFolder, "AvReminder.lnk"));
            shortcut.WindowStyle = 1; //for default window, 3 for maximize, 7 for minimize
            shortcut.TargetPath = Path.Combine(InstallFolderPath, "AvReminder.exe");
            shortcut.Description = "AV Reminder";
            shortcut.Save();
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
