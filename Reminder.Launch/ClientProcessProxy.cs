using System.ServiceModel;

namespace Reminder.Launch
{
    class ClientProcessProxy : ClientBase<IReminderClientProcess>, IReminderClientProcess
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
