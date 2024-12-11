using System.ServiceModel;

namespace Reminder.Launch
{
    [ServiceContract]
    public interface IReminderClientProcess
    {
        [OperationContract(IsOneWay = true)]
        void ShowMainWindow();
    }
}
