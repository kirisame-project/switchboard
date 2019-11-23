using System;
using System.IO;
using System.Threading.Tasks;
using Switchboard.Controllers.WebSocketsX.Facilities.Buffers;

namespace Switchboard.Services.FaceRecognition
{
    internal interface IFaceRecognitionService
    {
        RecognitionTask RequestRecognition(ObjectHolder<MemoryStream> imageStream, Func<Task> onUpdate);
    }
}