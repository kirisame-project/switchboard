using Switchboard.Services.FaceRecognition;

namespace Switchboard.Controllers.WebSocketized.Contracts.Local
{
    internal class ImageTaskUpdate : MessageWithPayload<RecognitionTask>
    {
        public ImageTaskUpdate(RecognitionTask task) : base((int) OperationCodes.ImageTaskUpdated, task)
        {
        }
    }
}