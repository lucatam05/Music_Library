using Music.User.Repository.Model;

namespace Music.User.Repository.Abstractions;

public interface IRepository
{
    public Task<Libraries?> GetLibraryByUserIdAsync(int userId, CancellationToken cancellationToken);
    public Task AddSongToLibraryAsync(int libraryId, string songId, CancellationToken cancellationToken);
    public Task RemoveSongToLibraryAsync(int libraryId, string songId, CancellationToken cancellationToken);
    public Task CreateLibraryAsync(int userId, CancellationToken cancellationToken);
    public Task<List<LibrarySongs>> GetCanzoniByLibreriaAsync(int libraryId, CancellationToken cancellationToken);
}