using System.Collections.Generic;
using Switchboard.Common;
using Switchboard.Services.FaceRecognition;
using Switchboard.Services.FaceRecognition.Abstractions;

namespace Switchboard.Metrics
{
    [Component(ComponentLifestyle.Singleton)]
    internal class TaskMetricsConfigurator
    {
        private static readonly MeasurementOptions DetectionTime = new MeasurementOptions
        {
            Name = "task_detection_time"
        };

        private static readonly MeasurementOptions VectorTime = new MeasurementOptions
        {
            Name = "task_vector_time"
        };

        private static readonly MeasurementOptions SearchTime = new MeasurementOptions
        {
            Name = "task_search_time"
        };

        private static readonly MeasurementOptions FaceCount = new MeasurementOptions
        {
            Name = "face_count"
        };

        private readonly MeasurementWriter _metrics;

        public TaskMetricsConfigurator(MeasurementWriterFactory writerFactory, RecognitionTaskFactory taskFactory)
        {
            _metrics = writerFactory.GetInstance(new Dictionary<string, string>
            {
                {"source", "TaskMetricsConfigurator"}
            });
            taskFactory.OnStateChanged += OnStateChanged;
        }

        private void OnStateChanged(object sender, BaseTaskState e)
        {
            if (!(sender is RecognitionTask))
                return;

            var task = (RecognitionTask) sender;
            if (!task.IsCompleted)
                return;

            _metrics.Write(DetectionTime, task.DetectionTask.Time);
            if (task.SearchTask.IsCompleted)
            {
                _metrics.Write(VectorTime, task.VectorizationTask.Time);
                _metrics.Write(SearchTime, task.SearchTask.Time);
            }

            _metrics.Write(FaceCount, task.FaceCount);
        }
    }
}