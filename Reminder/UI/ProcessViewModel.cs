using Reminder.Resources.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Reminder.UI
{
    public class ProcessViewModel : BaseViewModel
    {
        private ProcessResult _status;
        private bool _isProcessing;
        private TimeSpan _time;

        public string ProcessName { get; }

        public ProcessResult Status
        {
            get { return _status; }
            private set
            {
                if (Status == value) return;
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public bool IsProcessing
        {
            get { return _isProcessing; }
            private set
            {
                if (IsProcessing == value) return;
                _isProcessing = value;
                OnPropertyChanged(nameof(IsProcessing));
            }
        }

        public TimeSpan Time
        {
            get { return _time; }
            private set
            {
                if (Time == value) return;
                _time = value;
                OnPropertyChanged(nameof(Time));
            }
        }

        public ProcessViewModel(string processName)
        {
            ProcessName = processName;
            _status = ProcessResult.None;
        }

        public async Task PeformProcessAsync<T>(T argument, Func<T, Task> processBody, Action onOk)
        {
            IsProcessing = true;
            bool isOk = false;
            var sw = Stopwatch.StartNew();
            try
            {
                await processBody(argument);
                isOk = true;
                sw.Stop();
                Time = sw.Elapsed;
            }
            catch (Exception)
            {
                Status = ProcessResult.Error;
            }
            finally
            {
                IsProcessing = false;

                if (isOk)
                {
                    Status = ProcessResult.Ok;
                    onOk?.Invoke();
                }
            }
        }
    }

    public enum ProcessResult
    {
        [Description("Операции не проводилось.")]
        None,
        [Description("Операция завершилась успешно за ")]
        Ok,
        [Description("Операция завершилась с ошибкой.")]
        Error
    }
}
