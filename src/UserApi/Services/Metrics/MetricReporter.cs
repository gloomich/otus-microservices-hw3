using Prometheus;

namespace UserApi.Services.Metrics
{
    public class MetricReporter
    {
        private readonly ILogger<MetricReporter> _logger;
        private readonly Counter _requestCount;
        private readonly Histogram _requestLatency;

        public MetricReporter(ILogger<MetricReporter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _requestLatency = Prometheus.Metrics.CreateHistogram(
                "app_request_latency_seconds",
                "Application Request Latency", 
                new HistogramConfiguration
                {
                    LabelNames = new[] { "method", "endpoint" }                   
                });

            _requestCount = Prometheus.Metrics.CreateCounter(
                "app_request_count",
                "Application Request Count",
                new[] { "method", "endpoint", "http_status" });
        }

        public void RegisterRequestLatency(string method, string endpoint, TimeSpan elapsed)
        {
            _requestLatency.Labels(method, endpoint)
                .Observe(elapsed.TotalSeconds);
        }

        public void RegisterRequestCount(string method, string endpoint, int statusCode)
        {
            _requestCount.WithLabels(method, endpoint, statusCode.ToString())
                .Inc();
        }        
    }
}
