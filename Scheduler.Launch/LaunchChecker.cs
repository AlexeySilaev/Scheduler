using System.Security.Policy;
using System.ServiceModel;

namespace Scheduler.Launch
{
    public static class LaunchChecker
    {
        private static ClientProcessServiceHost _processHost;
        public static ISchedulerClientProcess ThisClientProcess { get; private set; }

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

        public static void RegisterClientProcess(ISchedulerClientProcess thisClientProcess)
        {
            ThisClientProcess = thisClientProcess;
        }
    }
}
