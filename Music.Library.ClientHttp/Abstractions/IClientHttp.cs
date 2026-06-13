using Music.Library.Shared;

namespace Music.Library.ClientHttp.Abstractions;

public interface IClientHttp
{
    Task CreateLibraryAsync(int userId, CancellationToken cancellationToken);
    Task<List<LibrarySongDTO>?> GetCanzoniUtenteAsync(string token, CancellationToken cancellationToken);
}