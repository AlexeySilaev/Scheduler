using System.ServiceModel;

namespace Scheduler.Launch
{
    [ServiceContract]
    public interface ISchedulerClientProcess
    {
        [OperationContract(IsOneWay = true)]
        void ShowMainWindow();
    }
}
