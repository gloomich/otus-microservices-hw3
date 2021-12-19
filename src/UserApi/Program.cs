using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;
using UserApi.DataAccess;
using UserApi.Services.Metrics;

if (args.Contains("-m"))
{
    DbExtensions.RunMigrations(args);
    return;
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

DbExtensions.UseUserDb(builder.Services, builder.Configuration);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<UserDbContext>(tags: new[] { "db_context" })
    .ForwardToPrometheus();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddSingleton<MetricReporter>();

var app = builder.Build();

app.UseRouting();

app.UseHttpMetrics();
//app.UseMiddleware<ResponseMetricMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/health/startup", new HealthCheckOptions()
    {
        Predicate = (check) => check.Tags.Contains("db_context"),
        ResultStatusCodes =
        {
            [HealthStatus.Healthy] = StatusCodes.Status200OK,
            [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
        }
    });
    //A failed liveness probe says: The application has crashed. You should shut it down and restart.
    endpoints.MapHealthChecks("/health/live", new HealthCheckOptions()
    {
        Predicate = _ => false,
        ResultStatusCodes =
        {
            [HealthStatus.Healthy] = StatusCodes.Status200OK,
            [HealthStatus.Degraded] = StatusCodes.Status200OK,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
        }
    });
    //A failed readiness probe says: The application is OK but not yet ready to serve traffic.
    endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions()
    {
        Predicate = _ => false,
        ResultStatusCodes =
        {
            [HealthStatus.Healthy] = StatusCodes.Status200OK,
            [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
        }
    });

    endpoints.MapMetrics();
});

app.Run();
