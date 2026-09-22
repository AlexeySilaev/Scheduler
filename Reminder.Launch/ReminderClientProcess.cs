using System.Diagnostics;

namespace Reminder.Launch
{
    class ReminderClientProcess : IReminderClientProcess
    {
        public void ShowMainWindow()
        {
            Debug.Assert(LaunchChecker.ThisClientProcess != null);
            LaunchChecker.ThisClientProcess.ShowMainWindow();
        }

        public void Shutdown()
        {
            Debug.Assert(LaunchChecker.ThisClientProcess != null);
            LaunchChecker.ThisClientProcess.Shutdown();
        }
    }
}
