using System.Threading;
using System.Threading.Tasks;
using Switchboard.Services.FaceRecognition;
using Switchboard.Services.FaceRecognition.Abstractions;

namespace Switchboard.Services.FaceReporting
{
    internal class ReportingConfigurator
    {
        private readonly IReportingService _service;

        public ReportingConfigurator(IReportingService reportingService, RecognitionTaskFactory taskFactory)
        {
            _service = reportingService;
            taskFactory.OnStateChanged += OnStateChanged;
        }

        private void OnStateChanged(object sender, BaseTaskState e)
        {
            if (!(sender is RecognitionTask))
                return;

            var task = (RecognitionTask) sender;
            if (task.SearchTask.State != BaseTaskState.Succeeded)
                return;

            var _ = Task.Run(async () =>
            {
                foreach (var face in task.Faces)
                    await _service.ReportLabelAsync(face.SearchResults[0].Label, face.SearchResults[0].Distance,
                        CancellationToken.None);
            });
        }
    }
}