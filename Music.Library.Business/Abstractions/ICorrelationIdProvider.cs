namespace Music.Library.Business.Abstractions;

/// <summary>
/// Espone il CorrelationId della richiesta corrente al livello Business,
/// che non ha (né deve avere) un riferimento diretto ad HttpContext.
/// </summary>
public interface ICorrelationIdProvider
{
    string? CorrelationId { get; }
}
