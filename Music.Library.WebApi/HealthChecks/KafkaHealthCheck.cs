using Microsoft.Extensions.Diagnostics.HealthChecks;
using Utility.Kafka.Abstractions.Clients;

namespace MusicLibrary.HealthChecks;

/// <summary>
/// Verifica che il broker Kafka sia raggiungibile, senza specificare un topic
/// (GetMetadata() senza argomenti restituisce solo i metadati del cluster,
/// senza il rischio di creare accidentalmente un topic - vedi commento in AdministatorClient.GetMetadata).
/// </summary>
public class KafkaHealthCheck(IAdministatorClient adminClient) : IHealthCheck
{
    // GetMetadata usa internamente un timeout di 30s: troppo lento per un health check,
    // che deve fallire rapidamente se il broker non è raggiungibile.
    private static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(5);

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var metadataTask = Task.Run(() => adminClient.GetMetadata(), cancellationToken);
            var timeoutTask = Task.Delay(CheckTimeout, cancellationToken);

            var completedTask = await Task.WhenAny(metadataTask, timeoutTask);
            if (completedTask == timeoutTask)
            {
                return HealthCheckResult.Unhealthy($"Timeout ({CheckTimeout.TotalSeconds}s) nella richiesta di metadata a Kafka");
            }

            var metadata = await metadataTask;
            return HealthCheckResult.Healthy($"Kafka raggiungibile, {metadata.Brokers.Count} broker attivi");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Impossibile contattare Kafka", ex);
        }
    }
}
