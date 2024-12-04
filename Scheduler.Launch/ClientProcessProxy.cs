using System.ServiceModel;

namespace Scheduler.Launch
{
    class ClientProcessProxy : ClientBase<ISchedulerClientProcess>, ISchedulerClientProcess
    {
        public ClientProcessProxy() : 
            base(new NetNamedPipeBinding(), new EndpointAddress(ClientProcessServiceHost.ServiceUri))
        {
        }

        public void ShowMainWindow()
        {
            Channel.ShowMainWindow();
        }
    }
}
