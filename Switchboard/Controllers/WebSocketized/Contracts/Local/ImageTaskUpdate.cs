using Switchboard.Services.FaceRecognition;
using Switchboard.Services.Lambda;

namespace Switchboard.Controllers.WebSocketized.Contracts.Local
{
    internal class ImageTaskUpdate : MessageWithPayload<LambdaTask>
    {
        public ImageTaskUpdate(RecognitionTask task) : base((int) OperationCodes.ImageTaskUpdated, task)
        {
        }
    }
}