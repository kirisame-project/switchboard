using System;
using System.IO;
using System.Threading.Tasks;

namespace Switchboard.Services.FaceRecognition
{
    internal interface IFaceRecognitionService
    {
        RecognitionTask RequestRecognition(Stream imageStream, Func<Task> onUpdate);
    }
}