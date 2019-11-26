using System.Threading;
using System.Threading.Tasks;

namespace Switchboard.Services.FaceReporting
{
    internal interface IReportingService
    {
        Task ReportLabelAsync(int label, double distance, CancellationToken cancellationToken);
    }
}