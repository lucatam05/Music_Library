using Microsoft.Extensions.Options;
using Utility.Kafka.Abstractions.Clients;
using Utility.Kafka.Services;

namespace MusicLibrary.Kafka;

public class LibraryProducerService(
    ILogger<LibraryProducerService> logger,
    IAdministatorClient adminClient,
    IOptions<LibraryKafkaTopics> optionsTopics,
    IOptions<KafkaProducerServiceOptions> optionsProducerService)
    : AbstractProducerService<LibraryKafkaTopics>(logger, adminClient, optionsTopics, optionsProducerService)
{
    protected override Task OperationsAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}