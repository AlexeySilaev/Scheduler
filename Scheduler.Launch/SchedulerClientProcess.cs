using System.Diagnostics;

namespace Scheduler.Launch
{
    class SchedulerClientProcess : ISchedulerClientProcess
    {
        public void ShowMainWindow()
        {
            Debug.Assert(LaunchChecker.ThisClientProcess != null);
            LaunchChecker.ThisClientProcess.ShowMainWindow();
        }
    }
}
