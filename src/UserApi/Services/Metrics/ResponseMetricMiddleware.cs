using System.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;

namespace UserApi.Services.Metrics
{
    public class ResponseMetricMiddleware
    {
        private readonly RequestDelegate _request;

        public ResponseMetricMiddleware(RequestDelegate request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public async Task Invoke(HttpContext httpContext, MetricReporter reporter)
        {
            var path = httpContext.Request.Path.Value;
            if (path == "/metrics")
            {
                await _request.Invoke(httpContext);
                return;
            }
            var sw = Stopwatch.StartNew();

            try
            {
                await _request.Invoke(httpContext);
            }
            finally
            {
                sw.Stop();

                var uri = new Uri(httpContext.Request.GetDisplayUrl());
                var endpint = uri.Segments.Length > 0 
                    ? uri.Segments.Length > 1 
                        ? $"{uri.Segments[0]}{uri.Segments[1]}" 
                        : uri.Segments[0] 
                    : "/";

                reporter.RegisterRequestCount(httpContext.Request.Method, endpint, httpContext.Response.StatusCode);
                reporter.RegisterRequestLatency(httpContext.Request.Method, endpint, sw.Elapsed);
            }
        }
    }
}
