using Microsoft.Extensions.Options;
using Music.Library.Repository.Abstractions;
using MusicLibrary.Outbox;
using Utility.Kafka.Abstractions.Clients;
using Utility.Kafka.Services;

namespace MusicLibrary.Kafka;

/// <summary>
/// Poller dell'outbox pattern: ogni ciclo (intervallo configurato in Kafka:ProducerService)
/// legge un batch di messaggi non ancora pubblicati dalla tabella OutboxMessages e li invia a Kafka.
/// Se un messaggio fallisce troppe volte viene marcato "Failed" (dead-letter) ed escluso dai retry futuri.
/// Periodicamente esegue anche il cleanup dei messaggi già pubblicati e più vecchi della retention configurata.
/// </summary>
public class LibraryProducerService(
    ILogger<LibraryProducerService> logger,
    IAdministatorClient adminClient,
    IServiceScopeFactory serviceScopeFactory,
    IProducerClient<string, string> producerClient,
    IOptions<LibraryKafkaTopics> optionsTopics,
    IOptions<KafkaProducerServiceOptions> optionsProducerService,
    IOptions<OutboxOptions> optionsOutbox)
    : AbstractProducerService<LibraryKafkaTopics>(logger, adminClient, optionsTopics, optionsProducerService)
{
    private int _cycleCount;

    protected override async Task OperationsAsync(CancellationToken cancellationToken)
    {
        OutboxOptions outboxOptions = optionsOutbox.Value;

        // IRepository è Scoped: il poller (Singleton) deve aprire un suo scope ad ogni ciclo,
        // esattamente come già fa ConsumerService di Utility.Kafka per i suoi message handler.
        await using AsyncServiceScope scope = serviceScopeFactory.CreateAsyncScope();
        IRepository repository = scope.ServiceProvider.GetRequiredService<IRepository>();

        int processed = await repository.ProcessPendingOutboxMessagesAsync(
            outboxOptions.BatchSize,
            outboxOptions.MaxAttempts,
            async (message, token) =>
            {
                try
                {
                    await producerClient.ProduceAsync(message.Topic, message.Key, message.Payload, token);
                    return true;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex,
                        "Pubblicazione fallita per il messaggio outbox {OutboxMessageId} verso il topic '{Topic}' (tentativo {Attempts})",
                        message.Id, message.Topic, message.Attempts + 1);
                    return false;
                }
            },
            cancellationToken);

        if (processed > 0)
        {
            logger.LogInformation("Outbox: {Count} messaggi presi in carico in questo ciclo", processed);
        }

        _cycleCount++;
        if (_cycleCount % outboxOptions.CleanupEveryNCycles == 0)
        {
            DateTime threshold = DateTime.UtcNow.AddDays(-outboxOptions.RetentionDays);
            int deleted = await repository.DeleteProcessedOutboxMessagesOlderThanAsync(threshold, cancellationToken);
            if (deleted > 0)
            {
                logger.LogInformation(
                    "Outbox cleanup: cancellati {Count} messaggi già pubblicati più vecchi di {RetentionDays} giorni",
                    deleted, outboxOptions.RetentionDays);
            }
        }
    }
}