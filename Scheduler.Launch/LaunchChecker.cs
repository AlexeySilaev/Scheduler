using System.Security.Policy;
using System.ServiceModel;

namespace Reminder.Launch
{
    public static class LaunchChecker
    {
        private static ClientProcessServiceHost _processHost;
        public static IReminderClientProcess ThisClientProcess { get; private set; }

        public static bool IsRunning()
        {
            try
            {
                ClientProcessProxy proxy = new ClientProcessProxy();
                //  proxy.Open();
                proxy.ShowMainWindow();
                proxy.Close();
                return true;
            }
            catch (CommunicationException)
            {
            }

            _processHost = new ClientProcessServiceHost();
            _processHost.Open();

            return false;
        }

        public static void RegisterClientProcess(IReminderClientProcess thisClientProcess)
        {
            ThisClientProcess = thisClientProcess;
        }
    }
}
