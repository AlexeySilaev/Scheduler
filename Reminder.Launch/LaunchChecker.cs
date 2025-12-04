using System.Security.Policy;
using System.ServiceModel;

namespace Reminder.Launch
{
    public static class LaunchChecker
    {
        private static ClientProcessServiceHost _processHost;
        public static IReminderClientProcess ThisClientProcess { get; private set; }
        public static bool IsRunning { get; private set; }

        static LaunchChecker()
        {
            try
            {
                ClientProcessProxy proxy = new ClientProcessProxy();
                //  proxy.Open();
                proxy.ShowMainWindow();
                proxy.Close();
                IsRunning = true;
            }
            catch (CommunicationException)
            {
            }

            if (IsRunning == false)
            {
                // запустились, даем себя послушать
                _processHost = new ClientProcessServiceHost();
                _processHost.Open();
            }
        }

        public static void RegisterClientProcess(IReminderClientProcess thisClientProcess)
        {
            ThisClientProcess = thisClientProcess;
        }
    }
}
