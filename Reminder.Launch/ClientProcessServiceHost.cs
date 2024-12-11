using System.ServiceModel;

namespace Reminder.Launch
{
    class ClientProcessServiceHost : ServiceHost
    {
        public ClientProcessServiceHost()
            : base(typeof(ReminderClientProcess))
        {
            AddServiceEndpoint(typeof(IReminderClientProcess), new NetNamedPipeBinding(), ServiceUri);
        }

        public const string ServiceUri = "net.pipe://localhost/ISchedulerClientProcess/";
    }
}
