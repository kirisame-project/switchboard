using Switchboard.Services.Lambda;

namespace Switchboard.Controllers.WebSocketized.Contracts
{
    public class TaskUpdated : MessageWithPayload<LambdaTask>
    {
        public TaskUpdated(LambdaTask task)
        {
            OperationCode = OperationCode.TaskUpdated;
            Payload = task;
        }
    }
}
