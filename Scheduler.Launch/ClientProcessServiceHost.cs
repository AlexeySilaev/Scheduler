using System.ServiceModel;

namespace Scheduler.Launch
{
    class ClientProcessServiceHost : ServiceHost
    {
        public ClientProcessServiceHost()
            : base(typeof(SchedulerClientProcess))
        {
            AddServiceEndpoint(typeof(ISchedulerClientProcess), new NetNamedPipeBinding(), ServiceUri);
        }

        public const string ServiceUri = "net.pipe://localhost/ISchedulerClientProcess/";
    }
}
