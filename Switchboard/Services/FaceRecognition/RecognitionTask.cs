using System;
using System.IO;
using Switchboard.Services.Lambda;

namespace Switchboard.Services.FaceRecognition
{
    public class RecognitionTask : LambdaTask
    {
        public RecognitionTask(Stream image) : base(image)
        {
        }

        public event EventHandler OnDetectionCompleted;

        public event EventHandler OnSearchCompleted;
    }
}