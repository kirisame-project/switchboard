using Switchboard.Services.Lambda;

namespace Switchboard.Controllers.WebSocketized.Contracts
{
    public class TaskUpdated : MessageWithPayload<LambdaTask>
    {
        public TaskUpdated(LambdaTask task) : base((int) OperationCodes.TaskUpdated)
        {
            Payload = task;
        }
    }
}