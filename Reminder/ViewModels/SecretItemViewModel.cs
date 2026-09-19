using Microsoft.Win32;
using Reminder.Models;
using Reminder.Resources.Storage;
using Reminder.Resources.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Reminder.ViewModels
{
    public class SecretItemViewModel : SchedulerItemViewModel
    {
        private int _viewCounter;
        private SecretKind _kind;
        private Guid _value;
        private string _secretString;
        private SecretFileViewModel _secretFile;

        private SecretItem SecretItem
        {
            get { return (SecretItem)_item; }
        }

        public override ReminderItemType Type
        {
            get { return ReminderItemType.Secret; }
        }

        public SecretKind Kind
        {
            get { return _kind; }
            set 
            {
                if (Kind == value) return;
                _kind = value;
                OnPropertyChanged(nameof(Kind));
                if (Kind == SecretKind.String)
                    SecretFile = null;
                else
                    SecretFile = new SecretFileViewModel();
            }
        }

        public int ViewCounter
        {
            get { return _viewCounter; }
            set
            {
                if (ViewCounter == value) return;
                _viewCounter = value;
                OnPropertyChanged(nameof(ViewCounter));
            }
        }

        public Guid Value
        {
            get { return _value; }
            set
            {
                if (Value == value) return;
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        public string SecretString
        {
            get { return _secretString; }
            set
            {
                if (SecretString == value) return;
                _secretString = value;
                OnPropertyChanged(nameof(SecretString));
            }
        }

        public SecretFileViewModel SecretFile
        {
            get { return _secretFile; }
            set
            {
                if (SecretFile == value) return;
                _secretFile = value;
                OnPropertyChanged(nameof(SecretFile));
            }
        }

        public string SelectFileTip
        {
            get
            {
                if (ReminderModel.Instance.Settings.IntroductoryMode)
                    return "Будет отображено только имя исходного файла, который не используется. Реальный файл хранится в зашифрованном виде отдельно. Кнопка «Выгрузить» в списке секретов - получить реальный файл.";
                return null;
            }
        }


        public ICommand Copy { get; }
        public ICommand Download { get; }
        public ICommand SelectFile { get; }

        public SecretItemViewModel(SecretItem item) : base(item)
        {
            _kind = item.Kind;
            _viewCounter = item.ViewCounter;
            _value = item.Value;
            _secretString = Secrets.Instance.GetStringSecret(Value);
            _secretFile = GetSecretFile();
            Copy = new Command(OnCopy);
            Download = new Command(OnDownload);
            SelectFile = new Command(OnSelectFile);
        }

        private void OnCopy()
        {
            Clipboard.SetText(SecretString);
            SecretItem.ViewCounter = ++ViewCounter;
        }

        private void OnDownload()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Выгрузить файл";
            saveFileDialog.FileName = SecretFile.Name;
           // saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (saveFileDialog.ShowDialog() == true)
            {
                Secrets.Instance.GetFileSecret(Value, saveFileDialog.FileName);
                SecretItem.ViewCounter = ++ViewCounter;
            }
        }

        private void OnSelectFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Выбор файла";
            //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (openFileDialog.ShowDialog() == true)
            {
                // проверка на CancelEdit() при автоблокировке
                MainViewModel mainViewModel = MainViewModel.FindInstance();
                if (mainViewModel?.EditingItem != this)
                    return;

                SecretFile.Name = Path.GetFileName(openFileDialog.FileName);
                SecretFile.Length = new FileInfo(openFileDialog.FileName).Length;
                SecretFile.FullPath = openFileDialog.FileName;
            }
        }

        protected override bool CanExecuteEdit()
        {
            return true;
        }

        public override void EndEdit()
        {
            base.EndEdit();
            if (Value == Guid.Empty)
            {
                if (Kind == SecretKind.String)
                    Value = Secrets.Instance.AddStringSecret(SecretString);
                else
                    Value = Secrets.Instance.AddFileSecret(SecretFile.FullPath);
            }
            else
            {
                if (Kind == SecretKind.String)
                    Value = Secrets.Instance.UpdateStringSecret(Value, SecretString);
                else
                {
                    Guid newValue = Secrets.Instance.UpdateFileSecret(SecretFile.FullPath);
                    if(newValue != Value) // это скорее для проформы. Не совпадут из-за добавления "соли"
                        CheckDeleteFileSecret();
                    Value = newValue;
                    SecretFile.FullPath = null;
                }
            }
            SecretItem.Kind = Kind;
            SecretItem.Value = Value;
            SecretItem.ViewCounter = ++ViewCounter;
            if (SecretFile != null)
                SecretItem.File = new SecretFile { Name = SecretFile.Name, Length = SecretFile.Length };
            else
                SecretItem.File = null;
        }

        public override void CancelEdit()
        {
            base.CancelEdit();
            Kind = SecretItem.Kind;
            Value = SecretItem.Value;
            SecretString = Secrets.Instance.GetStringSecret(Value);
            SecretItem.ViewCounter = ++ViewCounter;
            SecretFile = GetSecretFile();
        }

        public override void EndDelete()
        {
            base.EndDelete();
            if (Kind == SecretKind.String)
                Secrets.Instance.RemoveStringSecret(Value);
            else
                CheckDeleteFileSecret();
        }

        private void CheckDeleteFileSecret()
        {
            int fileSecretCount = ReminderModel.Instance.Secrets
                .Where(s => s.Kind == SecretKind.File && s.Value == Value)
                .Count();
            if (fileSecretCount == 1)
                Secrets.Instance.RemoveFileSecret(Value);
        }

        public override void Save()
        {
            ReminderModel.Instance.Secrets.Add((SecretItem)_item);
        }

        public override string Validate(bool isNew)
        {
            string result = base.Validate(isNew);
            if (string.IsNullOrEmpty(result))
            {
                if (Kind == SecretKind.String)
                {
                    if (string.IsNullOrEmpty(SecretString))
                        return "Необходимо заполнить значение.";
                }
                else
                {
                    if (File.Exists(SecretFile?.FullPath) == false)
                        return "Необходимо выбрать существующий файл.";
                    if(SecretFile.Length > 1_073_741_824L)
                        return "Файл слишком велик. Максимальный размер - 1 Гб.";
                }
            }
            return result;
        }

        private SecretFileViewModel GetSecretFile()
        {
            if (SecretItem.File != null)
                return new SecretFileViewModel { Name = SecretItem.File.Name, Length = SecretItem.File.Length };
            else
                return null;
        }
    }

    public class SecretFileViewModel : BaseViewModel
    {
        private string _name;
        private long _length;

        public string Name 
        {
            get { return _name; }
            set
            {
                if (Name == value) return;
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public long Length
        {
            get { return _length; }
            set
            {
                if (Length == value) return;
                _length = value;
                OnPropertyChanged(nameof(Length));
            }
        }

        internal string FullPath { get; set; }
    }
}
