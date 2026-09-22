using System.ServiceModel;

namespace Reminder.Launch
{
    public class ClientProcessProxy : ClientBase<IReminderClientProcess>, IReminderClientProcess
    {
        public const string ServiceUri = "net.pipe://localhost/ISchedulerClientProcess/";

        public ClientProcessProxy() : 
            base(new NetNamedPipeBinding(), new EndpointAddress(ServiceUri))
        {
        }

        public void ShowMainWindow()
        {
            Channel.ShowMainWindow();
        }

        public void Shutdown()
        { 
            Channel.Shutdown();
        }
    }
}
