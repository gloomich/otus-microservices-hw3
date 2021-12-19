﻿using System.Diagnostics;

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
                reporter.RegisterRequestCount(httpContext.Request.Method, httpContext.Request.Path, httpContext.Response.StatusCode);
                reporter.RegisterRequestLatency(httpContext.Request.Method, httpContext.Request.Path, sw.Elapsed);
            }
        }
    }
}
