using Microsoft.Extensions.Http.Resilience;
using MusicLibrary.Http;
using Polly;

namespace MusicLibrary;

public static class LibraryResilienceExtensions
{
    public static IServiceCollection AddResilientHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<CorrelationIdDelegatingHandler>();

        services.AddHttpClient<Music.Catalogue.ClientHttp.Abstractions.IClientHttp, Music.Catalogue.ClientHttp.ClientHttp>("CatalogueClient", client =>
            {
                client.BaseAddress = new Uri(configuration["Services:Catalogue"]!);
            })
            .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
            .AddStandardResilienceHandler(ConfigureInternalResilience);

        return services;
    }

    private static void ConfigureInternalResilience(HttpStandardResilienceOptions options)
    {
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.BackoffType = DelayBackoffType.Exponential;
        options.Retry.UseJitter = true;
        options.Retry.Delay = TimeSpan.FromMilliseconds(200);

        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(20);
        options.CircuitBreaker.MinimumThroughput = 4;
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(10);

        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(3);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);
    }
}