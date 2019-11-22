using Switchboard.Services.Lambda;

namespace Switchboard.Controllers.WebSocketized.Contracts.Local
{
    internal class ImageTaskUpdate : MessageWithPayload<LambdaTask>
    {
        public ImageTaskUpdate(LambdaTask task) : base((int) OperationCodes.ImageTaskUpdated, task)
        {
        }
    }
}