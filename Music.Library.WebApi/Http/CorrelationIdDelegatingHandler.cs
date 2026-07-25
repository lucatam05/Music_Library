using Music.Library.Business.Abstractions;
using MusicLibrary.Middlewares;

namespace MusicLibrary.Http;

/// <summary>
/// Copia l'header di correlazione dalla richiesta in ingresso corrente (se presente)
/// su ogni richiesta HTTP in uscita verso altri servizi interni, così i log restano
/// correlabili tra i vari servizi coinvolti nella stessa catena di chiamate.
/// </summary>
public class CorrelationIdDelegatingHandler(ICorrelationIdProvider correlationIdProvider) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var correlationId = correlationIdProvider.CorrelationId;

        if (!string.IsNullOrWhiteSpace(correlationId) && !request.Headers.Contains(CorrelationIdMiddleware.HeaderName))
        {
            request.Headers.Add(CorrelationIdMiddleware.HeaderName, correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}