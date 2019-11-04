using App.Metrics.Histogram;

namespace Switchboard
{
    public static class MetricsRegistry
    {
        public static readonly HistogramOptions StandardRequestDetectionTime = new HistogramOptions
        {
            Name = "Standard Request Detection Time"
        };

        public static readonly HistogramOptions StandardRequestVectorTime = new HistogramOptions
        {
            Name = "Standard Request Vector Time"
        };

        public static readonly HistogramOptions StandardRequestSearchTime = new HistogramOptions
        {
            Name = "Standard Request Search Time"
        };

        public static readonly HistogramOptions StandardRequestFaceCount = new HistogramOptions
        {
            Name = "Standard Request Face Count"
        };

        public static readonly HistogramOptions StandardRequestStage1Time = new HistogramOptions
        {
            Name = "Standard Request Stage 1 Response Time"
        };

        public static readonly HistogramOptions StandardRequestStage2Time = new HistogramOptions
        {
            Name = "Standard Request Stage 2 Response Time"
        };
    }
}