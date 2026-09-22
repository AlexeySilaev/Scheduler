using System.ServiceModel;

namespace Reminder.Launch
{
    class ClientProcessServiceHost : ServiceHost
    {
        public ClientProcessServiceHost()
            : base(typeof(ReminderClientProcess))
        {
            AddServiceEndpoint(typeof(IReminderClientProcess), new NetNamedPipeBinding(), ClientProcessProxy.ServiceUri);
        }
    }
}
