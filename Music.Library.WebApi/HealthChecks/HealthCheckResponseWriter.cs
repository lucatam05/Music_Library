using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MusicLibrary.HealthChecks;

/// <summary>
/// Formatta il risultato degli health check in JSON, con lo stato di ogni singolo check
/// (utile sia per un umano che ispeziona /health sia per l'healthcheck di Docker).
/// </summary>
public static class HealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                durationMs = entry.Value.Duration.TotalMilliseconds
            })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }
}
