using System;
using System.IO;
using AtomicAkarin.Shirakami.Reflections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Switchboard.Services.FaceRecognition.Abstractions;

namespace Switchboard.Services.FaceRecognition
{
    [Component(ServiceLifetime.Singleton)]
    internal class RecognitionTaskFactory
    {
        private readonly ILogger _logger;

        public RecognitionTaskFactory(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public event EventHandler<BaseTaskState> OnStateChanged;

        public RecognitionTask Create(Stream image)
        {
            var task = new RecognitionTask(image);
            task.OnStateChanged += TaskOnStateChanged;
            return task;
        }

        private void TaskOnStateChanged(object sender, BaseTaskState e)
        {
            try
            {
                OnStateChanged.Invoke(sender, e);
            }
            catch (Exception error)
            {
                _logger.LogError($"RecognitionTask state listener thrown unhandled exception:\n{error}");
            }
        }
    }
}