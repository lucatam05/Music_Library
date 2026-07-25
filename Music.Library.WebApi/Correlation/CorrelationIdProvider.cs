using Music.Library.Business.Abstractions;
using MusicLibrary.Middlewares;

namespace MusicLibrary.Correlation;

public class CorrelationIdProvider(IHttpContextAccessor httpContextAccessor) : ICorrelationIdProvider
{
    public string? CorrelationId => httpContextAccessor.HttpContext?.Items[CorrelationIdMiddleware.HeaderName] as string;
}
