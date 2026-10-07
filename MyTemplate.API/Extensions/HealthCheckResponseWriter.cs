using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MyTemplate.API.Extensions;

public static class HealthCheckResponseWriter
{
    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = Math.Round(report.TotalDuration.TotalMilliseconds, 1),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                durationMs = Math.Round(entry.Value.Duration.TotalMilliseconds, 1)
            })
        };

        context.Response.ContentType = "application/json";
        return context.Response.WriteAsJsonAsync(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
}
