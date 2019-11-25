using System.Threading;
using System.Threading.Tasks;

namespace Switchboard.Services.FaceRecognition
{
    internal interface IRecognitionTaskRunner
    {
        Task RunTaskAsync(RecognitionTask task, CancellationToken cancellationToken);
    }
}